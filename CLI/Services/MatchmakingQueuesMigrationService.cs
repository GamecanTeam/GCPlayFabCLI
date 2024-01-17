using PlayFab;
using PlayFab.MultiplayerModels;
using ProductMigration.Utils.Title;

namespace ProductMigration.Services
{
    class MatchmakingQueuesMigrationService
    {
        private MatchmakingQueuesService _source_matchmakingQueuesService;
        private MatchmakingQueuesService _target_matchmakingQueuesService;
        private bool _bVerbose = false;
        string _sourceTitleId;
        string _sourceTitleSecret;
        string _targetTitleId;
        string _targetTitleSecret;

        

        public MatchmakingQueuesMigrationService(string sourceTitleId, string sourceTitleSecret, string targetTitleId, string targetTitleSecret, bool bVerbose)
        {
            _bVerbose = bVerbose;
            _sourceTitleId = sourceTitleId;
            _sourceTitleSecret = sourceTitleSecret;
            _targetTitleId = targetTitleId;
            _targetTitleSecret = targetTitleSecret;            
        }

        public async Task LoginAsync()
        {
            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nSetting up titles credentials...");
            }

            var sourceTitleSettings = new PlayFabApiSettings
            {
                TitleId = _sourceTitleId,
                DeveloperSecretKey = _sourceTitleSecret
            };

            var sourceAuthContext = new PlayFabAuthenticationContext
            {
                EntityId = sourceTitleSettings.TitleId,
                EntityType = "title",
                EntityToken = await TitleAuthUtil.GetTitleEntityToken(sourceTitleSettings)
            };

            _source_matchmakingQueuesService = new MatchmakingQueuesService(sourceTitleSettings, sourceAuthContext);

            var targetTitleSettings = new PlayFabApiSettings
            {
                TitleId = _targetTitleId,
                DeveloperSecretKey = _targetTitleSecret
            };

            var targetAuthContext = new PlayFabAuthenticationContext
            {
                EntityId = targetTitleSettings.TitleId,
                EntityType = "title",
                EntityToken = await TitleAuthUtil.GetTitleEntityToken(targetTitleSettings)
            };

            _target_matchmakingQueuesService = new MatchmakingQueuesService(targetTitleSettings, targetAuthContext);

            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nService Ready!");
            }
        }

        public async Task CopyMatchmakingQueuesConfigAsync()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying Matchmaking Queues");

            List<MatchmakingQueueConfig> allMatchmakingQueuesFromSource = await _source_matchmakingQueuesService.ListMatchmakingQueuesAsync();

            if (allMatchmakingQueuesFromSource == null || allMatchmakingQueuesFromSource.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\nNo matchmaking queues available in source title {_source_matchmakingQueuesService.GetTitleId()}. Nothing to be done here.");
                return;
            }

            //List<MatchmakingQueueConfig> quuesToBeCopied = new List<MatchmakingQueueConfig>();
            //foreach (var queueCfg in allMatchmakingQueuesFromSource)
            //{
            //    MatchmakingQueueConfig queueToCopy = await _source_matchmakingQueuesService.GetMatchmakingQueueAsync(queueCfg.Name);
            //    if (queueToCopy != null) 
            //    {
            //        quuesToBeCopied.Add(queueToCopy);
            //    }
            //}

            //if (quuesToBeCopied == null || quuesToBeCopied.Count == 0)
            //{
            //    Console.ForegroundColor = ConsoleColor.DarkRed;
            //    Console.WriteLine($"\nNo matchmaking queues available in source title {_source_matchmakingQueuesService.GetTitleId()}. Nothing to be done here.");
            //    return;
            //}

            List<MatchmakingQueueConfig> allMatchmakingQueuesFromTarget = await _target_matchmakingQueuesService.ListMatchmakingQueuesAsync();

            if (allMatchmakingQueuesFromTarget != null && allMatchmakingQueuesFromTarget.Count > 0)
            {
                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"\n{allMatchmakingQueuesFromTarget.Count} to delete from title {_target_matchmakingQueuesService.GetTitleId()}.");
                    MatchmakingQueuesService.PrintMatchmakingQueuesConfig(allMatchmakingQueuesFromTarget);
                }

                foreach (var mmQueueConfig in allMatchmakingQueuesFromTarget)
                {
                    await _target_matchmakingQueuesService.RemoveMatchmakingQueueAsync(mmQueueConfig.Name);
                }
            }

            foreach (var mmQueueConfig in allMatchmakingQueuesFromSource)
            {
                await _target_matchmakingQueuesService.SetMatchmakingQueueAsync(mmQueueConfig);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying Matchmaking Queues has finished.");
        }
    }
}
