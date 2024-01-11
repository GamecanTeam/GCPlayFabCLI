using PlayFab;
using PlayFab.EconomyModels;
using System.Text;

namespace ProductMigration.Services.CatalogsV2
{
    class CatalogV2Service
    {
        private PlayFabEconomyInstanceAPI _playFabEconomyApi;
        private PlayFabApiSettings _currentSetting;

        public CatalogV2Service(PlayFabApiSettings settings, PlayFabAuthenticationContext authContext)
        {
            _currentSetting = settings;
            _playFabEconomyApi = new PlayFabEconomyInstanceAPI(settings, authContext);
        }

        public string GetTitleId()
        {
            return _currentSetting.TitleId;
        }

        public async Task<List<CatalogItem>> SearchItems()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nSearching for all the catalog items for title {_currentSetting.TitleId} has started...\n");

            var request = new SearchItemsRequest
            {
                Count = 50,
                Entity = new EntityKey
                { 
                    Id = _currentSetting.TitleId,
                    Type = "title"
                }
            };
            
            var response = await _playFabEconomyApi.SearchItemsAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to fetch the catalog items. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            List<CatalogItem> items = response.Result.Items;

            // keep fetching the catalog if we have a continuation token
            request.ContinuationToken = response.Result.ContinuationToken;
            while (!string.IsNullOrEmpty(response.Result.ContinuationToken))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\nFetching next page of the catalog.");
                response = await _playFabEconomyApi.SearchItemsAsync(request);

                if (response.Error != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to fetch next page of the catalog items. Reason: {response.Error.ErrorMessage}");
                    return items;
                }

                items.AddRange(response.Result.Items);
                request.ContinuationToken = response.Result.ContinuationToken;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nSearch Items Finished!\n");

            return items;
        }

        public async Task DeleteItems(List<CatalogItem> itemsToDelete)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nDeletion of {itemsToDelete.Count} catalog items for title {_currentSetting.TitleId} has started...\n");

            while (itemsToDelete.Count > 0) 
            {
                CatalogItem currentItem = itemsToDelete[0];
                string friendlyId = GetFriendlyId(currentItem);

                var request = new DeleteItemRequest
                {
                    Entity = new PlayFab.EconomyModels.EntityKey
                    {
                        Id = _currentSetting.TitleId,
                        Type = "title"
                    },
                    Id = currentItem.Id
                };

                var reponse = await _playFabEconomyApi.DeleteItemAsync(request);

                if (reponse.Error != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to delete item {friendlyId} (StackId: {currentItem.DefaultStackId}, Id: {currentItem.Id}). Reason: {reponse.Error.ErrorMessage}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"\nItem deleted: {friendlyId} (StackId: {currentItem.DefaultStackId}, Id: {currentItem.Id}).");
                }

                // Remove the deleted item from the list
                itemsToDelete.RemoveAt(0);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nDelete Items Finished!\n");
        }

        public async Task CreateItems(List<CatalogItem> itemsToCreate, bool bAddItemRefs = false, bool bAddPriceOptions = false)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nCreation of {itemsToCreate.Count} new items for title {_currentSetting.TitleId} has started...");

            while (itemsToCreate.Count > 0)
            {
                CatalogItem currentItem = itemsToCreate[0];
                string friendlyId = GetFriendlyId(currentItem);

                var request = new CreateDraftItemRequest
                {
                    Item = new CatalogItem
                    {
                        Type = currentItem.Type,
                        AlternateIds = currentItem.AlternateIds,
                        Title = currentItem.Title,
                        Description = currentItem.Description,
                        CreatorEntity = new EntityKey
                        {
                            Id = _currentSetting.TitleId,
                            Type = "title"
                        },
                        IsHidden = currentItem.IsHidden,
                        DefaultStackId = currentItem.DefaultStackId,
                        Platforms = currentItem.Platforms,
                        Tags = currentItem.Tags,
                        StartDate = currentItem.StartDate,
                        Contents = currentItem.Contents,
                        ItemReferences = bAddItemRefs ? currentItem.ItemReferences : null, // caller should be responsible for setting the correct IDs
                        PriceOptions = bAddPriceOptions ? currentItem.PriceOptions : null, // caller should be responsible for setting the correct price options with respective item IDs
                        DisplayProperties = currentItem.DisplayProperties,
                    },
                    Publish = true
                };
                
                var reponse = await _playFabEconomyApi.CreateDraftItemAsync(request);

                if (reponse.Error != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to create item {friendlyId} (StackId: {currentItem.DefaultStackId}). Reason: {reponse.Error.ErrorMessage}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;                    
                    Console.Write($"\nItem {friendlyId} (StackId: {currentItem.DefaultStackId}) created.");
                }

                itemsToCreate.RemoveAt(0);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nCreate Items Finished!\n");
        }

        public async Task<CatalogItem> GetItem(string itemId)
        {
            var request = new GetItemRequest
            {
                Id = itemId,
                Entity = new EntityKey
                { 
                    Id = _currentSetting.TitleId,
                    Type = "title"
                }
            };

            var response = await _playFabEconomyApi.GetItemAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to get item {itemId}. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nItem {itemId} fetched successfully. StackId: {response.Result.Item.DefaultStackId}");

            return response.Result.Item;
        }

        public async Task<CatalogItem> GetItem(CatalogAlternateId alternateId)
        {
            var request = new GetItemRequest
            {
                AlternateId = alternateId,
                Entity = new EntityKey
                {
                    Id = _currentSetting.TitleId,
                    Type = "title"
                }
            };

            var response = await _playFabEconomyApi.GetItemAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to get item {alternateId.Value} (id type: {alternateId.Type}). Reason: {response.Error.ErrorMessage}");
                return null;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nItem {alternateId.Type} fetched successfully. Id: {response.Result.Item.Id} (StackId: {response.Result.Item.DefaultStackId})");

            return response.Result.Item;
        }

        public static string GetFriendlyId(CatalogItem item)
        {
            // we know for now we use just one single friendly id, so we're going to aggregate this list
            return item.AlternateIds.Where(obj => obj.Type == "FriendlyId").Aggregate(new StringBuilder(), (sb, obj) => sb.Append(obj.Value), sb => sb.ToString());
        }

        public static void PrintCatalogItems(List<CatalogItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\n{items.Count} items:");

            foreach (var item in items)
            {                
                string friendlyId = GetFriendlyId(item);

                if (item.Type == "catalogItem")
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\nType: {item.Type} - FriendlyId: {friendlyId} (StackId: {item.DefaultStackId})");
                }
                else if (item.Type == "currency")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nType: {item.Type} - FriendlyId: {friendlyId} (StackId: {item.DefaultStackId})");
                }
                else if (item.Type == "bundle")
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    int numItemRefs = item.ItemReferences != null ? item.ItemReferences.Count : 0;
                    Console.WriteLine($"\nType: {item.Type} - FriendlyId: {friendlyId} (StackId: {item.DefaultStackId}) with {numItemRefs} items:");
                    if (item.ItemReferences != null)
                    {
                        foreach (var refItem in item.ItemReferences)
                        {
                            Console.WriteLine($"\n\tAmount: {refItem.Amount}, Id: {refItem.Id}");
                        }
                    }
                }
                else if (item.Type == "store")
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    int numItemRefs = item.ItemReferences != null ? item.ItemReferences.Count : 0;
                    Console.WriteLine($"\nType: {item.Type} - FriendlyId: {friendlyId} (StackId: {item.DefaultStackId}) with {numItemRefs} items:");
                    if (item.ItemReferences != null)
                    {
                        foreach (var refItem in item.ItemReferences)
                        {
                            Console.WriteLine($"\n\tAmount: {refItem.Amount}, Id: {refItem.Id}");
                            if (refItem.PriceOptions != null)
                            {
                                foreach (var price in refItem.PriceOptions.Prices)
                                {
                                    foreach (var amount in price.Amounts)
                                    {
                                        Console.WriteLine($"\n\t\tAmount: {amount.Amount}, Id: {amount.ItemId}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nType: {item.Type} not properly handled yet... - FriendlyId: {friendlyId} (StackId: {item.DefaultStackId})");
                }                
            }
        }
    }
}
