using PlayFab;
using PlayFab.ServerModels;

namespace ProductMigration.Services
{
    class ServerTitleDataService
    {
        private PlayFabServerInstanceAPI _playFabServerInstanceApi;
        private PlayFabApiSettings _currentSetting;

        public ServerTitleDataService(PlayFabApiSettings settings)
        {
            _currentSetting = settings;

            _playFabServerInstanceApi = new PlayFabServerInstanceAPI(settings);
        }

        public string GetTitleId()
        {
            return _currentSetting.TitleId;
        }

        public async Task<GetTitleDataResult> GetTitleData(List<string> keys)
        {
            var request = new GetTitleDataRequest
            {
                Keys = keys != null && keys.Count > 0 ? keys : null,

            };

            var response = await _playFabServerInstanceApi.GetTitleDataAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string keyNames = keys != null && keys.Count > 0 ? string.Join(", ", keys) : "empty";
                Console.Write($"\nFailed to fetch title data for title {GetTitleId()} (keys: {keyNames}). Reason: {response.Error.ErrorMessage}");
                return null;
            }

            return response.Result;
        }

        public async Task<GetTitleDataResult> GetTitleInternalData(List<string> keys)
        {
            var request = new GetTitleDataRequest
            {
                Keys = keys != null && keys.Count > 0 ? keys : null,

            };

            var response = await _playFabServerInstanceApi.GetTitleInternalDataAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string keyNames = keys != null && keys.Count > 0 ? string.Join(", ", keys) : "empty";
                Console.Write($"\nFailed to fetch title internal data for title {GetTitleId()} (keys: {keyNames}). Reason: {response.Error.ErrorMessage}");
                return null;
            }

            return response.Result;
        }

        public async Task<bool> SetTitleData(string key, string value)
        {
            // Setting value to null will remove the key
            var request = new SetTitleDataRequest { Key = key, Value = value };
            var response = await _playFabServerInstanceApi.SetTitleDataAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to set title data for title {GetTitleId()} with key: {key}, value: {value}). Reason: {response.Error.ErrorMessage}");
                return false;
            }

            return true;
        }

        public async Task<bool> SetTitleInternalData(string key, string value)
        {
            // Setting value to null will remove the key
            var request = new SetTitleDataRequest { Key = key, Value = value };
            var response = await _playFabServerInstanceApi.SetTitleInternalDataAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to set title internal data for title {GetTitleId()} with key: {key}, value: {value}). Reason: {response.Error.ErrorMessage}");
                return false;
            }

            return true;
        }
    }
}
