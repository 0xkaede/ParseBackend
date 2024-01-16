using Newtonsoft.Json;

namespace ParseBackend.Models.Profile
{
    public class Profile
    {
        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonProperty("rvn")]
        public int Revision { get; set; }

        [JsonProperty("wipeNumber")]
        public int WipeNumber { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("profileId")]
        public string ProfileId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("items")]
        public Dictionary<string, ProfileItem> Items { get; set; }

        [JsonProperty("stats")]
        public ProfileStats Stats { get; set; }

        [JsonProperty("commandRevision")]
        public int CommandRevision { get; set; }
    }

    public class ProfileStats
    {
        [JsonProperty("attributes")]
        public object Attributes { get; set; }
    }

    public class ProfileItem
    {
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [JsonProperty("attributes")]
        public object Attributes { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
