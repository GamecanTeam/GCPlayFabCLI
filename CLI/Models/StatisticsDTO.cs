using Newtonsoft.Json;

namespace CLI.Models
{
    public class StatisticsDTO
    {
        [JsonProperty("TotalTimePlayed", NullValueHandling = NullValueHandling.Ignore)]
        public float TotalTimePlayed { get; set; } // total number of seconds

        [JsonProperty("TotalGamesPlayed", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalGamesPlayed { get; set; }

        [JsonProperty("TotalWins", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalWins { get; set; }

        [JsonProperty("TotalLosses", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalLosses { get; set; }

        [JsonProperty("TotalKills", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalKills { get; set; }

        [JsonProperty("TotalDeaths", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalDeaths { get; set; }

        [JsonProperty("Offensive", NullValueHandling = NullValueHandling.Ignore)]
        public float Offensive { get; set; }

        [JsonProperty("Defensive", NullValueHandling = NullValueHandling.Ignore)]
        public float Defensive { get; set; }

        [JsonProperty("TotalHealingDone", NullValueHandling = NullValueHandling.Ignore)]
        public float TotalHealingDone { get; set; }

        [JsonProperty("MaxKillStreak", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxKillStreak { get; set; }

        [JsonProperty("TotalMoveDistance", NullValueHandling = NullValueHandling.Ignore)]
        public float TotalMoveDistance { get; set; }

        [JsonProperty("TotalShotsHit", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalShotsHit { get; set; }

        [JsonProperty("TotalSoloKills", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalSoloKills { get; set; }

        [JsonProperty("WeaponAccuracy", NullValueHandling = NullValueHandling.Ignore)]
        public float WeaponAccuracy { get; set; }

        [JsonProperty("TotalDoubleJumps", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalDoubleJumps { get; set; }

        [JsonProperty("TotalAssists", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalAssists { get; set; }

        [JsonProperty("TotalDamageDone", NullValueHandling = NullValueHandling.Ignore)]
        public float TotalDamageDone { get; set; }

        [JsonProperty("TotalShotsFired", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalShotsFired { get; set; }

        [JsonProperty("TotalHeadShotsEliminations", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalHeadShotsEliminations { get; set; }

        [JsonProperty("TotalFlagCarrierKills", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalFlagCarrierKills { get; set; }

        [JsonProperty("TotalFlagDelivered", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalFlagDelivered { get; set; }

        [JsonProperty("TotalFlagReturned", NullValueHandling = NullValueHandling.Ignore)]
        public int TotalFlagReturned { get; set; }

        [JsonProperty("TotalShieldDamageBlocked", NullValueHandling = NullValueHandling.Ignore)]
        public float TotalShieldDamageBlocked { get; set; }

        public bool IsValid()
        {
            return TotalTimePlayed > 0.0;
        }
    }
}
