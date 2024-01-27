using Newtonsoft.Json;
using ParseBackend.Enums;
using ParseBackend.Models.Database.Other;
using System.Text.RegularExpressions;
using static ParseBackend.Global;

namespace ParseBackend.Models.Friends
{
    public class FriendSummary
    {
        [JsonProperty("friends")]
        public List<FriendListSummary> Friends { get; set; } = new List<FriendListSummary>();

        [JsonProperty("incoming")]
        public List<FriendListSummary> Incoming { get; set; } = new List<FriendListSummary>();

        [JsonProperty("outgoing")]
        public List<FriendListSummary> Outgoing { get; set; } = new List<FriendListSummary>();

        [JsonProperty("suggested")]
        public List<FriendListSummary> Suggested { get; set; } = new List<FriendListSummary>();

        [JsonProperty("blocklist")]
        public List<FriendListSummary> Blocklist { get; set; } = new List<FriendListSummary>();

        [JsonProperty("settings")]
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>() { { "acceptInvites", "public" } };
    }

    public class FriendListSummary
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("groups", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> Groups { get; set; }

        [JsonProperty("mutual", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Mutual { get; set; }

        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Alias { get; set; }

        [JsonProperty("note", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Note { get; set; }

        [JsonProperty("favorite", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Favorite { get; set; }

        [JsonProperty("created", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Created { get; set; }
    }
}
