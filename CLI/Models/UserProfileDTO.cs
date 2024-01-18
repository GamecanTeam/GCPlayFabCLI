using Newtonsoft.Json;

namespace CLI.Models
{
    public class UserProfileDTO
    {
        [JsonProperty("PlayFabId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlayFabId { get; set; }

        [JsonProperty("UserName", NullValueHandling = NullValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty("SkillRate", NullValueHandling = NullValueHandling.Ignore)]
        public float SkillRate { get; set; }

        [JsonProperty("Level", NullValueHandling = NullValueHandling.Ignore)]
        public float Level { get; set; }

        [JsonProperty("ExperiencePoints", NullValueHandling = NullValueHandling.Ignore)]
        public float ExperiencePoints { get; set; }

        [JsonProperty("Banner", NullValueHandling = NullValueHandling.Ignore)]
        public string Banner { get; set; }

        [JsonProperty("Heroes", NullValueHandling = NullValueHandling.Ignore)]
        public List<HeroDTO> Heroes { get; set; }

        [JsonProperty("UserRatingScore", NullValueHandling = NullValueHandling.Ignore)]
        public float UserRatingScore { get; set; } // this is a user feedback score that is gathered once (ranging from 1 to 10, 0 means never done it)

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(PlayFabId);
        }

        public int GetTotalGamesPlayed()
        {
            return Heroes.Sum(hero => hero.Statistics.TotalGamesPlayed);
        }

        public float GetTotalTimePlayed()
        {
            return Heroes.Sum(hero => hero.Statistics.TotalTimePlayed);
        }
    }
}
