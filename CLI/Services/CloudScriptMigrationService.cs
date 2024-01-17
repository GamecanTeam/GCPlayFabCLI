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

        public async Task Login()
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
                Console.WriteLine($"\nService Ready!");
            }
        }

        public async Task CopyFunctions()
        {
            Console.ForegroundColor = ConsoleColor.White;
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

            // delete old functions in target
            List<FunctionModel> target_allOldFunctions = await _target_cloudScriptService.ListFunctions();
            if (target_allOldFunctions != null && target_allOldFunctions.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n{target_allOldFunctions.Count} functions to delete in title {_target_cloudScriptService.GetTitleId()}:");
                if (_bVerbose)
                {
                    CloudScriptService.PrintFunctions(target_allOldFunctions, _target_cloudScriptService.GetTitleId());
                }

                await _target_cloudScriptService.DeleteFunctions(target_allOldFunctions);
            }

            // create the new functions on target title from source data
            await _target_cloudScriptService.CreateFunctions(sourceFunctionsToBeCopied);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying Functions has finished.");
        }        
    }
}
