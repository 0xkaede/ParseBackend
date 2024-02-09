using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService
{
    public class ProfileResponse
    {
        [JsonProperty("profileRevision")]
        public int ProfileRevision { get; set; }

        [JsonProperty("profileId")]
        public string ProfileId { get; set; }

        [JsonProperty("profileChangesBaseRevision")]
        public int ProfileChangesBaseRevisionRevision { get; set; }

        [JsonProperty("profileChanges")]
        public List<object> ProfileChanges { get; set; }

        [JsonProperty("profileCommandRevision")]
        public int ProfileCommandRevision { get; set; }

        [JsonProperty("serverTime")]
        public string ServerTime { get; set; }

        [JsonProperty("responseVersion")]
        public int ResponseVersion { get; set; } = 1;

        [JsonProperty("multiUpdate")]
        public object MultiUpdate { get; set; }

        [JsonProperty("notifications")]
        public List<NotificationsResponse> Notifications { get; set; }
    }

    public class NotificationsResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; }

        [JsonProperty("lootResult")]
        public List<NotificationLoot> NotificationLoots { get; set; }
    }

    public class NotificationLoot
    {
        [JsonProperty("itemType")]
        public string ItemType { get; set; }

        [JsonProperty("itemGuid")]
        public string ItemGuid { get; set; }

        [JsonProperty("itemProfile")]
        public string ItemProfile { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
