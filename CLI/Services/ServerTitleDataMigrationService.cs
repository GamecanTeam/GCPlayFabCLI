using PlayFab;

namespace ProductMigration.Services
{
    class ServerTitleDataMigrationService
    {
        ServerTitleDataService _sourceServerTitleDataService;
        ServerTitleDataService _targetServerTitleDataService;
        bool _bVerbose;

        public ServerTitleDataMigrationService(string sourceTitleId, string sourceTitleSecret, string targetTitleId, string targetTitleSecret, bool bVerbose)
        {
            _bVerbose = bVerbose;

            var sourceSettings = new PlayFabApiSettings
            {
                TitleId = sourceTitleId,
                DeveloperSecretKey = sourceTitleSecret
            };

            _sourceServerTitleDataService = new ServerTitleDataService(sourceSettings);

            var targetSettings = new PlayFabApiSettings
            {
                TitleId = targetTitleId,
                DeveloperSecretKey = targetTitleSecret
            };

            _targetServerTitleDataService = new ServerTitleDataService(targetSettings);
        }

        public async Task CopyData(bool bInternalData = false)
        {
            Console.ForegroundColor = ConsoleColor.White;
            string headerMsg = bInternalData ? "Title Data" : "Title Internal Data";
            Console.WriteLine($"\n\nCopying {headerMsg}...");

            var sourceTitleData = bInternalData 
                ? await _sourceServerTitleDataService.GetTitleInternalData(null)
                : await _sourceServerTitleDataService.GetTitleData(null);

            if (sourceTitleData == null || sourceTitleData.Data == null || sourceTitleData.Data.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nThere aren't any title data in title {_sourceServerTitleDataService.GetTitleId()}. Nothing to be done here.");
                return;
            }

            var targetTitleData = bInternalData
                ? await _targetServerTitleDataService.GetTitleInternalData(null)
                : await _targetServerTitleDataService.GetTitleData(null);

            if (targetTitleData != null && targetTitleData.Data != null && targetTitleData.Data.Count > 0)
            {
                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"\nThere are {targetTitleData.Data.Count} keys on {headerMsg} to delete in title {_targetServerTitleDataService.GetTitleId()}.");
                }

                foreach (var kvp in targetTitleData.Data)
                {
                    // deleting data from the target that is not present on source title
                    // other this key will be overridden anyways we don't have to delete it (avoid unnecessary requests)
                    if (!sourceTitleData.Data.ContainsKey(kvp.Key))
                    {
                        if (bInternalData)
                        {
                            await _targetServerTitleDataService.SetTitleInternalData(kvp.Key, null);
                        }
                        else
                        {
                            await _targetServerTitleDataService.SetTitleData(kvp.Key, null);
                        }
                    }
                }
            }

            // sets title data on target with the source data
            foreach (var kvp in sourceTitleData.Data)
            {
                if (bInternalData)
                {
                    await _targetServerTitleDataService.SetTitleInternalData(kvp.Key, kvp.Value);
                }
                else
                {
                    await _targetServerTitleDataService.SetTitleData(kvp.Key, kvp.Value);
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying {headerMsg} has finished.");
        }
    }
}
