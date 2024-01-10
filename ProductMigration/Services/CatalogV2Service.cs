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

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"\nDeleting item: {currentItem.Id}");                

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
                    Console.Write($"\nFailed to delete item {currentItem.Id}. Reason: {reponse.Error.ErrorMessage}");
                }

                // Remove the deleted item from the list
                itemsToDelete.RemoveAt(0);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nDelete Items Finished!\n");
        }

        public async Task CreateItems(List<CatalogItem> itemsToCreate)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nCreation of {itemsToCreate.Count} new items for title {_currentSetting.TitleId} has started...");

            while (itemsToCreate.Count > 0)
            {
                CatalogItem currentItem = itemsToCreate[0];

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"\nCreating item: {currentItem.Id}");

                var request = new CreateDraftItemRequest
                {
                    Item = new CatalogItem
                    {
                        Type = currentItem.Type,
                        Title = currentItem.Title,
                        AlternateIds = currentItem.AlternateIds,
                        DefaultStackId = currentItem.DefaultStackId,
                        CreatorEntity = new EntityKey
                        {
                            Id = _currentSetting.TitleId,
                            Type = "title"
                        },
                        DisplayProperties = currentItem.DisplayProperties                        
                    },
                    Publish = true
                };
                
                var reponse = await _playFabEconomyApi.CreateDraftItemAsync(request);

                if (reponse.Error != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to create item {currentItem.Id}. Reason: {reponse.Error.ErrorMessage}");
                }

                itemsToCreate.RemoveAt(0);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"\nCreate Items Finished!\n");
        }

        public static void PrintCatalogItems(List<CatalogItem> items)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n\n{items.Count} items:");

            foreach (var item in items)
            {
                if (item.Type == "catalogItem")
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (item.Type == "currency")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (item.Type == "bundle")
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                }
                else if (item.Type == "store")
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                string friendlyId = item.AlternateIds.Where(obj => obj.Type == "FriendlyId").Aggregate(new StringBuilder(), (sb, obj) => sb.Append(obj.Value), sb => sb.ToString());
                Console.WriteLine($"\nType: {item.Type} - FriendlyId: {friendlyId} (StackId: {item.DefaultStackId})");
            }
        }
    }
}
