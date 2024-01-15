using PlayFab;
using PlayFab.MultiplayerModels;

namespace ProductMigration.Services
{
    class MatchmakingQueuesService
    {
        private PlayFabMultiplayerInstanceAPI _playFabMultiplayerApi;
        private PlayFabApiSettings _currentSetting;

        public MatchmakingQueuesService(PlayFabApiSettings settings, PlayFabAuthenticationContext authContext)
        {
            _currentSetting = settings;

            _playFabMultiplayerApi = new PlayFabMultiplayerInstanceAPI(settings, authContext);
        }

        public string GetTitleId()
        {
            return _currentSetting.TitleId;
        }

        public async Task<List<MatchmakingQueueConfig>> ListMatchmakingQueuesAsync()
        {
            var request = new ListMatchmakingQueuesRequest();

            var response = await _playFabMultiplayerApi.ListMatchmakingQueuesAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to fetch matchmaking queues for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            return response.Result.MatchMakingQueues;
        }

        public async Task RemoveMatchmakingQueueAsync(string queueName)
        {
            var request = new RemoveMatchmakingQueueRequest
            {
                QueueName = queueName
            };

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"\nRemoving matchmaking queue {queueName} for title {GetTitleId()}.");

            var response = await _playFabMultiplayerApi.RemoveMatchmakingQueueAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to remove matchmaking queue {queueName} for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
            }
        }

        public async Task SetMatchmakingQueueAsync(MatchmakingQueueConfig matchmakingQueueConfig)
        {
            var request = new SetMatchmakingQueueRequest
            {
                MatchmakingQueue = new MatchmakingQueueConfig
                {
                    DifferenceRules = matchmakingQueueConfig.DifferenceRules,
                    MatchTotalRules = matchmakingQueueConfig.MatchTotalRules,
                    MaxMatchSize = matchmakingQueueConfig.MaxMatchSize,
                    MaxTicketSize = matchmakingQueueConfig.MaxTicketSize,
                    Name = matchmakingQueueConfig.Name,
                    RegionSelectionRule = matchmakingQueueConfig.RegionSelectionRule,
                    SetIntersectionRules = matchmakingQueueConfig.SetIntersectionRules,
                    StatisticsVisibilityToPlayers = matchmakingQueueConfig.StatisticsVisibilityToPlayers,
                    StringEqualityRules = matchmakingQueueConfig.StringEqualityRules,
                    TeamDifferenceRules = matchmakingQueueConfig.TeamDifferenceRules,
                    Teams = matchmakingQueueConfig.Teams,
                    TeamSizeBalanceRule = matchmakingQueueConfig.TeamSizeBalanceRule,
                    TeamTicketSizeSimilarityRule = matchmakingQueueConfig.TeamTicketSizeSimilarityRule
                }
            };

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write($"\nAdding matchmaking queue {matchmakingQueueConfig.Name} for title {GetTitleId()}.");

            var response = await _playFabMultiplayerApi.SetMatchmakingQueueAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to set matchmaking queue {matchmakingQueueConfig.Name} for title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
            }
        }

        public static void PrintMatchmakingQueuesConfig(List<MatchmakingQueueConfig> matchmakingQueueConfigs)
        {
            if (matchmakingQueueConfigs == null || matchmakingQueueConfigs.Count == 0)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\n{matchmakingQueueConfigs.Count} queues:");

            foreach (var mmQueueConfig in matchmakingQueueConfigs)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"\nMatchmaking Queue {mmQueueConfig.Name}.");
            }
        }
    }
}
