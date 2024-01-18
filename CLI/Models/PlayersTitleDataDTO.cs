using Newtonsoft.Json;

namespace CLI.Models
{
    public class PlayersTitleDataDTO
    {
        [JsonProperty("PlayerId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlayerId { get; set; }

        [JsonProperty("PlayerData", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> PlayerData { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(PlayerId);
        }
    }
}
