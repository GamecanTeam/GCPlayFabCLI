using PlayFab.AuthenticationModels;
using PlayFab;

namespace ProductMigration.Utils.Title
{
    class TitleAuthUtil
    {
        public static async Task<string> GetTitleEntityToken(PlayFabApiSettings settings)
        {
            var authApi = new PlayFabAuthenticationInstanceAPI(settings);

            var request = new GetEntityTokenRequest
            {
                Entity = new PlayFab.AuthenticationModels.EntityKey
                {
                    Id = settings.TitleId,
                    Type = "title"
                }
            };

            Dictionary<string, string> extraHeaders = new Dictionary<string, string>
            {
                { "X-SecretKey", settings.DeveloperSecretKey }
            };

            var response = await authApi.GetEntityTokenAsync(request, extraHeaders);

            if (response.Error != null)
            {
                Console.WriteLine($"Failed to fetch entity token. Reason: {response.Error.ErrorMessage}");
                return "";
            }

            return response.Result.EntityToken;
        }
    }

}
