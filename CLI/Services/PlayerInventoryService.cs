using PlayFab;
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

        public async Task SetupEconomyApi()
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

        public async Task DeleteInventoryItems(string collectionId, List<string> itemsFriendlyIds)
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

    }
}
