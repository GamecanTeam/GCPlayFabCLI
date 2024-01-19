using CLI.Models;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;
using Utils.Title;

namespace Exporter.Services
{
    class PlayerExporterService
    {
        private PlayFabServerInstanceAPI _playFabServerInstanceApi;
        private PlayFabApiSettings _currentSetting;
        private bool _bVerbose;

        public PlayerExporterService(string titleId, string devSecretKey, bool bVerbose)
        {
            _bVerbose = bVerbose;

            _currentSetting = new PlayFabApiSettings
            {
                TitleId = titleId,
                DeveloperSecretKey = devSecretKey
            };
        }

        public async Task Login()
        {
            var authContext = new PlayFabAuthenticationContext
            {
                EntityId = _currentSetting.TitleId,
                EntityType = "title",
                EntityToken = await TitleAuthUtil.GetTitleEntityToken(_currentSetting)
            };

            _playFabServerInstanceApi = new PlayFabServerInstanceAPI(_currentSetting, authContext);
        }

        public string GetTitleId()
        {
            return _currentSetting.TitleId;
        }

        public async Task<List<PlayerProfile>> GetAllPlayersInSegmentAsync(string segmentId)
        {
            var request = new GetPlayersInSegmentRequest
            {
                SegmentId = segmentId,
                SecondsToLive = 5400,
                MaxBatchSize = 500
            };

            var response = await _playFabServerInstanceApi.GetPlayersInSegmentAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to fetch players in segment for segmentId {segmentId} in title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            List<PlayerProfile> playerProfiles = response.Result.PlayerProfiles;

            request.ContinuationToken = response.Result.ContinuationToken;
            while (!string.IsNullOrEmpty(request.ContinuationToken))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"\nFetching next page for segment {segmentId} in title {GetTitleId()}. Current num players: {playerProfiles.Count}");

                response = await _playFabServerInstanceApi.GetPlayersInSegmentAsync(request);

                if (response.Error != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\nFailed to fetch next page for segment {segmentId} in title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                    return playerProfiles;
                }

                playerProfiles.AddRange(response.Result.PlayerProfiles);
                request.ContinuationToken = response.Result.ContinuationToken;
            }

            return playerProfiles;
        }

        public async Task<PlayersTitleDataDTO> GetUserDataAsync(string masterPlayerAccountId, List<string> keyNames)
        {
            var request = new GetUserDataRequest
            {
                PlayFabId = masterPlayerAccountId,
                Keys = keyNames
            };

            var response = await _playFabServerInstanceApi.GetUserDataAsync(request);

            if (response.Error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                var logMsg = string.Join(", ", keyNames);
                Console.Write($"\nFailed to fetch user data with keys {logMsg} for player {masterPlayerAccountId} in title {GetTitleId()}. Reason: {response.Error.ErrorMessage}");
                return null;
            }

            PlayersTitleDataDTO playerTitleData = new PlayersTitleDataDTO()
            {
                PlayerId = masterPlayerAccountId,
                PlayerData = new Dictionary<string, string>()
            };

            foreach (KeyValuePair<string, UserDataRecord> kvp in response.Result.Data)
            {
                playerTitleData.PlayerData.Add(kvp.Key, kvp.Value.Value);
            }

            return playerTitleData;
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(string masterPlayerAccountId)
        {
            string userProfileKeyName = "UserProfile";
            List<string> keyNames = new List<string> { userProfileKeyName };

            PlayersTitleDataDTO playerTitleData = await GetUserDataAsync(masterPlayerAccountId, keyNames);

            if (playerTitleData == null || playerTitleData.PlayerData == null || !playerTitleData.PlayerData.ContainsKey(userProfileKeyName))
            {
                return null;
            }

            string userProfileJsonString = playerTitleData.PlayerData[userProfileKeyName];
            if (string.IsNullOrEmpty(userProfileJsonString)) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nUser Profile json is null or empty for player {masterPlayerAccountId} in title {GetTitleId()}.");
                return null;
            }

            UserProfileDTO userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userProfileJsonString);

            if (userProfile == null || !userProfile.IsValid())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\nFailed to properly fetch user profile (key: {userProfileKeyName}) for player {masterPlayerAccountId} in title {GetTitleId()}.");
                return null;
            }

            return userProfile;
        }

        public async Task<List<UserProfileDTO>> GetAllPlayersUserProfilesAsync(string segmentId)
        {
            List<PlayerProfile> playersProfiles = await GetAllPlayersInSegmentAsync(segmentId);

            List<UserProfileDTO> playersUserProfiles = new List<UserProfileDTO>();
            foreach (var playerProfile in playersProfiles)
            {
                var userProfile = await GetUserProfileAsync(playerProfile.PlayerId);

                if (userProfile != null && userProfile.IsValid())
                {
                    playersUserProfiles.Add(userProfile);
                }
            }

            return playersUserProfiles;
        }

        public static void ExportFeedbackToCsv(List<UserProfileDTO> userProfiles, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Writing the header
                    writer.WriteLine("PlayFabId,TotalGamesPlayed,TotalTimePlayed,UserRatingScore");

                    // Writing data
                    foreach (var userProfile in userProfiles)
                    {
                        writer.WriteLine($"{userProfile.PlayFabId},{userProfile.GetTotalGamesPlayed()},{userProfile.GetTotalTimePlayed()},{userProfile.UserRatingScore}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error writing to CSV: " + ex.Message);
            }
        }

    }
}
