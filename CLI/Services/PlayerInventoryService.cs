using PlayFab;
using PlayFab.EconomyModels;
using Utils.Title;

namespace CLI.Services
{
    class PlayerInventoryService
    {
        private PlayFabEconomyInstanceAPI _playFabEconomyApi;
        private string _titlePlayerAccountId;
        private string _titleId;
        private string _devTitleSecretKey;
        private bool _bVerbose;

        public PlayerInventoryService(string titleId, string devTitleSecretKey, string titlePlayerAccountId, bool bVerbose)
        {
            _titleId = titleId;
            _devTitleSecretKey = devTitleSecretKey;
            _titlePlayerAccountId = titlePlayerAccountId;
            _bVerbose = bVerbose;
        }

        public string GetTitleId()
        {
            return _titleId;
        }

        public async Task SetupEconomyApiAsync()
        {
            var _currentSetting = new PlayFabApiSettings
            {
                TitleId = _titleId,
                DeveloperSecretKey = _devTitleSecretKey
            };

            var authContext = new PlayFabAuthenticationContext
            {
                EntityId = _currentSetting.TitleId,
                EntityType = "title",
                EntityToken = await TitleAuthUtil.GetTitleEntityToken(_currentSetting)
            };

            _playFabEconomyApi = new PlayFabEconomyInstanceAPI(_currentSetting, authContext);
        }
        
        public async Task DeleteInventoryItemsAsync(string collectionId, List<string> itemsFriendlyIds)
        {
            foreach (var itemId in itemsFriendlyIds)
            { 
                var request = new PlayFab.EconomyModels.DeleteInventoryItemsRequest
                {
                    Entity = new PlayFab.EconomyModels.EntityKey 
                    { 
                        Type = "title_player_account",
                        Id = _titlePlayerAccountId
                    },
                    CollectionId = collectionId,
                    Item = new PlayFab.EconomyModels.InventoryItemReference
                    { 
                        AlternateId = new PlayFab.EconomyModels.AlternateId 
                        { 
                            Value = itemId,
                            Type = "FriendlyId"
                        }
                    }
                };

                if (_bVerbose)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"\nDeleting item {itemId} from player ({_titlePlayerAccountId}) inventory collection {collectionId} in title {_titleId}.");
                }
                var response = await _playFabEconomyApi.DeleteInventoryItemsAsync(request);

                if (response.Error != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to delete item {itemId}. Reason: {response.Error.ErrorMessage}");
                }
            }
        }

        public async Task BatchDeleteInventoryItemsAsync(string collectionId, List<string> itemsToDelete)
        {
            while (itemsToDelete.Count > 0)
            {
                int numberOfItemsPerChunk = 10; // playfab only allows 10 operations at a time
                List<string> chunk = itemsToDelete.Take(numberOfItemsPerChunk).ToList();
                var chunkItemsIds = string.Join(", ", chunk);

                List<InventoryOperation> inventoryOperations = new List<InventoryOperation>();

                foreach (var item in chunk)
                {
                    var inventoryOperation = new InventoryOperation
                    {
                        Delete = new DeleteInventoryItemsOperation
                        {
                            Item = new InventoryItemReference
                            {
                                AlternateId = new AlternateId
                                {
                                    Type = "FriendlyId",
                                    Value = item
                                }
                            }
                        }
                    };

                    inventoryOperations.Add(inventoryOperation);
                }

                var request = new ExecuteInventoryOperationsRequest
                {
                    Entity = new PlayFab.EconomyModels.EntityKey
                    {
                        Id = _titlePlayerAccountId,
                        Type = "title_player_account"
                    },
                    Operations = inventoryOperations,
                    CollectionId = collectionId
                };

                var response = await _playFabEconomyApi.ExecuteInventoryOperationsAsync(request);

                if (response.Error != null) 
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to delete batch of items in chunk: {chunkItemsIds}. Reason: {response.Error.ErrorMessage}\nYOU MUST CALL IT AGAIN FOR THIS CHUNK WITHOUT THE FAULTY ITEM!");
                    itemsToDelete = itemsToDelete.Skip(numberOfItemsPerChunk).ToList();
                    continue;
                }

                itemsToDelete = itemsToDelete.Skip(numberOfItemsPerChunk).ToList();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"\nDeleted chunk of items: {chunkItemsIds}. (Chunk had {chunk.Count} items, {itemsToDelete.Count} items left.)");
            }
        }
              
        public async Task<List<InventoryItem>> GetInventoryItemsAsync(string collectionId)
        {
            var request = new GetInventoryItemsRequest
            {
                Entity = new EntityKey
                {
                    Id = _titlePlayerAccountId,
                    Type = "title_player_account"
                },
                Count = 50,
                CollectionId = collectionId
            };

            var response = await _playFabEconomyApi.GetInventoryItemsAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to get inventory items from collection {collectionId}. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            List<InventoryItem> items = response.Result.Items;

            request.ContinuationToken = response.Result.ContinuationToken;
            while (!string.IsNullOrEmpty(request.ContinuationToken))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\nFetching next page...");
                response = await _playFabEconomyApi.GetInventoryItemsAsync(request);

                if (response.Error != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to fetch next page of the inventory items. Reason: {response.Error.ErrorMessage}");
                    return items;
                }

                items.AddRange(response.Result.Items);
                request.ContinuationToken = response.Result.ContinuationToken;
            }

            return items;
        }

        public async Task DeleteAllInventoryItemsAsync(string collectionId)
        {
            var allItems = await GetInventoryItemsAsync(collectionId);

            if (allItems == null || allItems.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"\nThere are no items to delete for player {_titlePlayerAccountId} in collection {collectionId} for title {GetTitleId()}");
                return;
            }

            List<string> itemsIds = allItems.Select(item => item.Id).ToList();

            await BatchDeleteInventoryItemsAsync(collectionId, itemsIds);
        }
    }
}
