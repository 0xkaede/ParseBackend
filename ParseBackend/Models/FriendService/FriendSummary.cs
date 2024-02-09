using Newtonsoft.Json;
using ParseBackend.Enums;
using System.Text.RegularExpressions;
using static ParseBackend.Global;

namespace ParseBackend.Models.FriendService
{
    public class FriendSummary
    {
        [JsonProperty("friends")]
        public List<Friend> Friends { get; set; } = new List<Friend>();

        [JsonProperty("incoming")]
        public List<Friend> Incoming { get; set; } = new List<Friend>();

        [JsonProperty("outgoing")]
        public List<FriendOld> Outgoing { get; set; } = new List<FriendOld>();

        [JsonProperty("suggested")]
        public List<SuggestedFriend> Suggested { get; set; } = new List<SuggestedFriend>();

        [JsonProperty("blocklist")]
        public List<string> Blocklist { get; set; } = new List<string>();

        [JsonProperty("settings")]
        public Settings Settings { get; set; } = new Settings();
    }

    public class Settings
    {
        [JsonProperty("acceptInvites")]
        public string AcceptInvites { get; set; } = "public";

        [JsonProperty("mutualPrivacy")]
        public string MutualPrivacy { get; set; }
    }

    public class BaseFriend
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }
    }

    public class SuggestedFriend : BaseFriend
    {
        [JsonProperty("mutual")]
        public int Mutual { get; set; }

        [JsonProperty("connections")]
        public object Connections { get; set; }
    }

    public class Friend : BaseFriend
    {
        [JsonProperty("mutual")]
        public int Mutual { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }
    }

    public class AcceptedFriend : Friend
    {
        [JsonProperty("groups")]
        public List<object> Groups { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }
    }
}
