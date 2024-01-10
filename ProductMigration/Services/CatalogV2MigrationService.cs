using PlayFab;
using PlayFab.EconomyModels;
using ProductMigration.Services.CatalogsV2;
using ProductMigration.Utils.Title;

namespace ProductMigration.Services
{
    class CatalogV2MigrationService
    {
        private CatalogV2Service _source_catalogV2Service;
        private CatalogV2Service _target_catalogV2Service;
        private bool _bVerbose = false;
        string _sourceTitleId;
        string _sourceTitleSecret;
        string _targetTitleId;
        string _targetTitleSecret;

        public CatalogV2MigrationService(string sourceTitleId, string sourceTitleSecret, string targetTitleId, string targetTitleSecret, bool bVerbose)
        {
            _bVerbose = bVerbose;
            _sourceTitleId = sourceTitleId;
            _sourceTitleSecret = sourceTitleSecret;
            _targetTitleId = targetTitleId;
            _targetTitleSecret = targetTitleSecret;            
        }

        public async Task Setup()
        {
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

            _source_catalogV2Service = new CatalogV2Service(sourceTitleSettings, sourceAuthContext);

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

            _target_catalogV2Service = new CatalogV2Service(targetTitleSettings, targetAuthContext);
        }

        public async Task CopyCatalogV2()
        {
            // fetch all catalog items from the source title
            List<CatalogItem> source_allItems = await _source_catalogV2Service.SearchItems();
            List<CatalogItem> source_catalogItems = source_allItems.Where(x => x.Type == "catalogItem").ToList(); // TODO: how to handle bundles and currency?

            if (_bVerbose)
            {
                Console.WriteLine($"\n\nNumber of catalog items available in the source title: {source_catalogItems.Count}");
                CatalogV2Service.PrintCatalogItems(source_catalogItems);
            }

            List<CatalogItem> target_allItems = await _target_catalogV2Service.SearchItems();
            List<CatalogItem> target_catatologItems = target_allItems.Where(x => x.Type == "catalogItem").ToList(); // TODO: how to handle "currency", "bundle" and "store"?
            // delete only items that the target has and the source doesn't have
            List<CatalogItem> itemsToDelete = target_catatologItems.Where(targetItem => !source_catalogItems.Any(sourceItem => sourceItem.Id == targetItem.Id)).ToList();
            // create only items that target doesn't have it yet
            List<CatalogItem> itemsToCreate = source_catalogItems.Where(sourceItem => !target_catatologItems.Any(targetItem => targetItem.Id == sourceItem.Id)).ToList();
            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\nNumber of catalog items to delete from the target title: {itemsToDelete.Count}\n");
                CatalogV2Service.PrintCatalogItems(itemsToDelete);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n\nNumber of catalog items to create in the target title: {itemsToCreate.Count}\n");
                CatalogV2Service.PrintCatalogItems(itemsToCreate);
            }
            await _target_catalogV2Service.DeleteItems(itemsToDelete);
            await _target_catalogV2Service.CreateItems(itemsToCreate);
        }
    }
}
