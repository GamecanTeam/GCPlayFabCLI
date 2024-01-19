using PlayFab;
using PlayFab.EconomyModels;
using Utils.Title;

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

        List<CatalogItem> _allCatalogItemsFromSource;
        List<CatalogItem> _allCatalogItemsFromTarget;
        List<CatalogItem> _allOldCatalogItemsFromTarget;

        public CatalogV2MigrationService(string sourceTitleId, string sourceTitleSecret, string targetTitleId, string targetTitleSecret, bool bVerbose)
        {
            _bVerbose = bVerbose;
            _sourceTitleId = sourceTitleId;
            _sourceTitleSecret = sourceTitleSecret;
            _targetTitleId = targetTitleId;
            _targetTitleSecret = targetTitleSecret;

            _allCatalogItemsFromSource = new List<CatalogItem>();
            _allCatalogItemsFromTarget = new List<CatalogItem>();
            _allOldCatalogItemsFromTarget = new List<CatalogItem>();
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

            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nService Ready!");
            }
        }

        public async Task CopyCatalogV2()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying CatalogV2 from title {_sourceTitleId} to {_targetTitleId}...");

            // fetch all catalog items from the source title
            _allCatalogItemsFromSource = await _source_catalogV2Service.SearchItems();
            List<CatalogItem> sourceItems = _allCatalogItemsFromSource.Where(x => x.Type == "catalogItem" || x.Type == "currency").ToList(); // bundles and stores items will be handled after creating the catalog items and currencies

            if (_bVerbose)
            {
                CatalogV2Service.PrintCatalogItems(sourceItems);
            }

            _allOldCatalogItemsFromTarget = await _target_catalogV2Service.SearchItems();
            List<CatalogItem> targetItems = _allOldCatalogItemsFromTarget.Where(x => x.Type == "catalogItem" || x.Type == "currency").ToList();
            // delete only items that the target has and the source doesn't have
            List<CatalogItem> itemsToDelete = targetItems.Where(targetItem => !sourceItems.Any(sourceItem => sourceItem.DefaultStackId == targetItem.DefaultStackId)).ToList(); // since stack id is the same as friendly id in our design we're going to rely on it in here due to it's simplicit to fetch xD
            // create only items that target doesn't have it yet
            List<CatalogItem> itemsToCreate = sourceItems.Where(sourceItem => !targetItems.Any(targetItem => targetItem.DefaultStackId == sourceItem.DefaultStackId)).ToList();            

            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nNumber of catalog items to delete from the target title: {itemsToDelete.Count}\n");
                CatalogV2Service.PrintCatalogItems(itemsToDelete);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nNumber of catalog items to create in the target title: {itemsToCreate.Count}\n");
                CatalogV2Service.PrintCatalogItems(itemsToCreate);
            }

            if (itemsToDelete.Count > 0)
            {
                await _target_catalogV2Service.DeleteItems(itemsToDelete);
            }

            if (itemsToCreate.Count > 0)
            {
                // update price options
                bool bShouldCreatePriceOptions = false;
                foreach (CatalogItem item in itemsToCreate) 
                {
                    if (item.PriceOptions != null && item.PriceOptions.Prices != null && item.PriceOptions.Prices.Count > 0)
                    {
                        bShouldCreatePriceOptions = true;
                        var newPriceOptions = BuildCatalogPriceOptionsFromSource(item.PriceOptions);
                        item.PriceOptions = newPriceOptions;
                    }           
                }

                await _target_catalogV2Service.CreateItems(itemsToCreate, false, bShouldCreatePriceOptions);
            }

            _allCatalogItemsFromTarget = await _target_catalogV2Service.SearchItems(); // cache all items here so we avoid fetching them again for bundles and stores

            await CopyBundles();
            await CopyStores();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying CatalogV2 finished!");
        }

        public async Task CopyBundles()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying bundles...");

            // delete bundles catalog items since we're about to create new ones
            List<CatalogItem> target_OldBundlesCatalogItems = _allOldCatalogItemsFromTarget.Where(x => x.Type == "bundle").ToList();
            if (target_OldBundlesCatalogItems.Count > 0)
            {
                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\nDeleting {target_OldBundlesCatalogItems.Count} old bundles from target title {_target_catalogV2Service.GetTitleId()}");
                    CatalogV2Service.PrintCatalogItems(target_OldBundlesCatalogItems);
                }

                await _target_catalogV2Service.DeleteItems(target_OldBundlesCatalogItems);
            }

            List<CatalogItem> source_bundlesCatalogItems = _allCatalogItemsFromSource.Where(x => x.Type == "bundle").ToList();

            if (source_bundlesCatalogItems.Count == 0)
            {
                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\nAll good. No bundles available to be copied from the source title {_source_catalogV2Service.GetTitleId()}");
                }
                return;
            }            

            // item references workaround
            // 1 - fetch the friendly id for each referenced item in the item bundle/store from the source title
            // 2 - fetch the respective catalog item from the target using the source item friendly id
            // 3 - add the referenced item to the new bundle/store to be created on the target title
            List<CatalogItem> target_bundlesCatalogItems = new List<CatalogItem>();
            foreach (var item in source_bundlesCatalogItems)
            {
                string currentBundleItemFriendlyId = CatalogV2Service.GetFriendlyId(item);

                if (item.ItemReferences == null)
                {
                    if (_bVerbose)
                    {                        
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\n{item.Type} {currentBundleItemFriendlyId} ({item.Id}) doesn't have item references and will be skipped.");
                    }
                    continue;
                }

                List<CatalogItemReference> targetItemReferences = new List<CatalogItemReference>();

                // fetching the friendly id for each of the "item reference" so we can fetch the Id for the "target item references"
                foreach (var itemRef in item.ItemReferences)
                {
                    // NOTE: old but gold, we're using the cached values to avoid another request which speeds up the migration considerable
                    //CatalogItem srcCatItem = await _source_catalogV2Service.GetItem(itemRef.Id);
                    CatalogItem? srcCatItem = _allCatalogItemsFromSource.Find(catItem => catItem.Id == itemRef.Id);
                    
                    if (srcCatItem == null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\nFailed to find item for item ref: {itemRef.Id} from source title {_source_catalogV2Service.GetTitleId()}");
                        continue; // skip this one
                    }

                    string srcFriendlyId = CatalogV2Service.GetFriendlyId(srcCatItem);

                    if (_bVerbose)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"\nFetching correct item Id of item {srcFriendlyId} for the bundle {currentBundleItemFriendlyId} in target title {_target_catalogV2Service.GetTitleId()}");
                    }

                    // NOTE: old but gold, we're using the cached values to avoid another request which speeds up the migration considerable
                    //CatalogItem targetCatItem = await _target_catalogV2Service.GetItem(new CatalogAlternateId
                    //{
                    //    Type = "FriendlyId",
                    //    Value = srcFriendlyId
                    //});
                    CatalogItem? targetCatItem = _allCatalogItemsFromTarget.Find(catItem => catItem.DefaultStackId == srcFriendlyId);
                    if (targetCatItem == null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\nFailed to find item {srcFriendlyId} from target title {_target_catalogV2Service.GetTitleId()}");
                        continue; // skip this one
                    }

                    targetItemReferences.Add(new CatalogItemReference { Amount = itemRef.Amount, Id = targetCatItem.Id });
                }

                // create new price options if any
                if (item.PriceOptions != null && item.PriceOptions.Prices != null && item.PriceOptions.Prices.Count > 0)
                {
                    CatalogPriceOptions newTargetPriceOptions = BuildCatalogPriceOptionsFromSource(item.PriceOptions);
                    item.PriceOptions = newTargetPriceOptions;
                }

                // set the new references
                item.ItemReferences = targetItemReferences.Count > 0 ? targetItemReferences : null;
                target_bundlesCatalogItems.Add(item);
            }

            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{target_bundlesCatalogItems.Count} bundle items to be created in the target title {_target_catalogV2Service.GetTitleId()}");
                CatalogV2Service.PrintCatalogItems(target_bundlesCatalogItems);
            }

            await _target_catalogV2Service.CreateItems(target_bundlesCatalogItems, true, true);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying Bundles finished!");
        }

        // NOTE: this could have been put together with bundles since it has similar operations, but now we're not making any extra requests anymore so we separated it just to simplify things
        public async Task CopyStores()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying Stores...");

            // delete stores catalog items since we're about to create new ones
            List<CatalogItem> target_OldStoresCatalogItems = _allOldCatalogItemsFromTarget.Where(x => x.Type == "store").ToList();
            if (target_OldStoresCatalogItems.Count > 0)
            {
                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\nDeleting {target_OldStoresCatalogItems.Count} old stores from target title {_target_catalogV2Service.GetTitleId()}");
                    CatalogV2Service.PrintCatalogItems(target_OldStoresCatalogItems);
                }

                await _target_catalogV2Service.DeleteItems(target_OldStoresCatalogItems);
            }

            List<CatalogItem> source_storesCatalogItems = _allCatalogItemsFromSource.Where(x => x.Type == "store").ToList();

            if (source_storesCatalogItems.Count == 0)
            {
                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\nAll good. No stores available to be copied from the source title {_source_catalogV2Service.GetTitleId()}");
                }
                return;
            }            

            List<CatalogItem> target_storesCatalogItems = new List<CatalogItem>();
            foreach (var item in source_storesCatalogItems)
            {
                string currentBundleItemFriendlyId = CatalogV2Service.GetFriendlyId(item);

                if (item.ItemReferences == null)
                {
                    if (_bVerbose)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\n{item.Type} {currentBundleItemFriendlyId} ({item.Id}) doesn't have item references and will be skipped.");
                    }
                    continue;
                }

                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\nFetching items references for {item.Type} {currentBundleItemFriendlyId} ({item.Id}) from source title {_source_catalogV2Service.GetTitleId()}");
                }

                List<CatalogItemReference> targetItemReferences = new List<CatalogItemReference>();

                // fetching the friendly id for each of the "item reference" so we can fetch the Id for the "target item references"
                foreach (var itemRef in item.ItemReferences)
                {
                    CatalogItem? srcCatItem = _allCatalogItemsFromSource.Find(catItem => catItem.Id == itemRef.Id);

                    if (srcCatItem == null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\nFailed to find item for item ref: {itemRef.Id} from source title {_source_catalogV2Service.GetTitleId()}");
                        continue; // skip this one
                    }

                    string srcFriendlyId = CatalogV2Service.GetFriendlyId(srcCatItem);

                    CatalogItem? targetCatItem = _allCatalogItemsFromTarget.Find(catItem => catItem.DefaultStackId == srcFriendlyId);
                    if (targetCatItem == null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"\nFailed to find item {srcFriendlyId} from target title {_target_catalogV2Service.GetTitleId()}");
                        continue; // skip this one
                    }

                    // NOTE: create the correct catalog price options with the target items id but using the other values from the source
                    CatalogPriceOptions priceOptions = BuildCatalogPriceOptionsFromSource(itemRef.PriceOptions);
                    if (priceOptions.Prices.Count > 0)
                    {
                        var catalogItemReference = new CatalogItemReference
                        {
                            Amount = itemRef.Amount,
                            Id = targetCatItem.Id,
                            PriceOptions = priceOptions
                        };

                        targetItemReferences.Add(catalogItemReference);
                    }
                }                

                // set the new references
                item.ItemReferences = targetItemReferences.Count > 0 ? targetItemReferences : null;
                target_storesCatalogItems.Add(item);
            }

            if (_bVerbose)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{target_storesCatalogItems.Count} store items to be created in the target title {_target_catalogV2Service.GetTitleId()}");
                CatalogV2Service.PrintCatalogItems(target_storesCatalogItems);
            }

            await _target_catalogV2Service.CreateItems(target_storesCatalogItems, true, true);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\nCopying Stores finished!");
        }

        private CatalogPriceOptions BuildCatalogPriceOptionsFromSource(CatalogPriceOptions sourceCatalogPriceOptions)
        {
            // NOTE: create the correct catalog price options with the target items id but using the other values from the source
            List<CatalogPrice> prices = new List<CatalogPrice>();
            foreach (var price in sourceCatalogPriceOptions.Prices)
            {
                List<CatalogPriceAmount> catalogPriceAmounts = new List<CatalogPriceAmount>();
                foreach (var amount in price.Amounts)
                {
                    CatalogItem? tempSrcCatItem = _allCatalogItemsFromSource.Find(tempItem => tempItem.Id == amount.ItemId);
                    if (tempSrcCatItem != null)
                    {
                        string friendlyId = CatalogV2Service.GetFriendlyId(tempSrcCatItem);
                        CatalogItem? tempTargetCatItem = _allCatalogItemsFromTarget.Find(tempItem => tempItem.DefaultStackId == tempSrcCatItem.DefaultStackId);
                        if (tempTargetCatItem != null)
                        {
                            catalogPriceAmounts.Add(new CatalogPriceAmount { Amount = amount.Amount, ItemId = tempTargetCatItem.Id });
                        }
                    }
                }

                if (catalogPriceAmounts.Count > 0)
                {
                    prices.Add(new CatalogPrice { Amounts = catalogPriceAmounts, UnitAmount = price.UnitAmount, UnitDurationInSeconds = price.UnitDurationInSeconds });
                }
            }

            return new CatalogPriceOptions { Prices = prices };
        }
    }
}
