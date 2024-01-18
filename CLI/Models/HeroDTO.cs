using Newtonsoft.Json;

namespace CLI.Models
{
    public class HeroDTO
    {
        [JsonProperty("Class", NullValueHandling = NullValueHandling.Ignore)]
        public string Class { get; set; }

        [JsonProperty("Corporation", NullValueHandling = NullValueHandling.Ignore)]
        public string Corporation { get; set; }

        [JsonProperty("Statistics", NullValueHandling = NullValueHandling.Ignore)]
        public StatisticsDTO Statistics { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Class);
        }
    }
}
