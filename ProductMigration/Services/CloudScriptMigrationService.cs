using PlayFab;
using PlayFab.CloudScriptModels;
using ProductMigration.Utils.Title;

namespace ProductMigration.Services
{
    class CloudScriptMigrationService
    {
        private bool _bVerbose;
        private string _sourceTitleId;
        private string _sourceTitleSecret;
        private string _targetTitleId;
        private string _targetTitleSecret;

        private CloudScriptService _source_cloudScriptService;
        private CloudScriptService _target_cloudScriptService;

        public CloudScriptMigrationService(string sourceTitleId, string sourceTitleSecret, string targetTitleId, string targetTitleSecret, bool bVerbose)
        {
            _bVerbose = bVerbose;
            _sourceTitleId = sourceTitleId;
            _sourceTitleSecret = sourceTitleSecret;
            _targetTitleId = targetTitleId;
            _targetTitleSecret = targetTitleSecret;
        }

        public async Task Init()
        {
            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nInitializing Cloud Script Migration Service...");
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

            _source_cloudScriptService = new CloudScriptService(sourceTitleSettings, sourceAuthContext);

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

            _target_cloudScriptService = new CloudScriptService(targetTitleSettings, targetAuthContext);

            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nInitializiation finished!");
            }
        }

        public async Task CopyFunctions()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n\nCopying Functions");

            if (_source_cloudScriptService == null || _target_cloudScriptService == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nCopying Functions has failed. Make sure to call Init() first!");
                return;
            }

            List<FunctionModel> sourceAllFunctions_meta = await _source_cloudScriptService.ListFunctions();
            if (sourceAllFunctions_meta == null || sourceAllFunctions_meta.Count == 0) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nThere aren't any functions on title {_source_cloudScriptService.GetTitleId()}. Nothing to be done here.");
                return; 
            }

            List<GCCloudScriptFunction> sourceFunctionsToBeCopied = await _source_cloudScriptService.GetFunctions(sourceAllFunctions_meta);

            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n{sourceFunctionsToBeCopied.Count} functions to be copied from title {_source_cloudScriptService.GetTitleId()}:");
                CloudScriptService.PrintFunctions(sourceFunctionsToBeCopied, _source_cloudScriptService.GetTitleId());
            }

            List<FunctionModel> targetAllFunctions = await _target_cloudScriptService.ListFunctions();
            if (targetAllFunctions != null && targetAllFunctions.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n{targetAllFunctions.Count} functions to delete in title {_target_cloudScriptService.GetTitleId()}:");
                if (_bVerbose)
                {
                    CloudScriptService.PrintFunctions(targetAllFunctions, _target_cloudScriptService.GetTitleId());
                }
            }            

            // TODO:
            // delete old functions in target
            // copy functions from source to target


            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n\nCopying Functions has finished.");
        }        
    }
}
