using Newtonsoft.Json;
using ParseBackend.Enums;
using static ParseBackend.Global;
using ParseBackend.Models.Other.Database.Other;

namespace ParseBackend.Models.FriendService
{
    public class FriendOld
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        public FriendOld() { }

        public FriendOld(FriendsListData data)
        {
            switch (data.Status)
            {
                case FriendsStatus.Accepted:
                    Status = "ACCEPTED";
                    Direction = "OUTBOUND";
                    break;
                case FriendsStatus.Incoming:
                    Status = "PENDING";
                    Direction = "INBOUND";
                    break;
                case FriendsStatus.Outgoing:
                    Status = "PENDING";
                    Direction = "OUTBOUND";
                    break;
                case FriendsStatus.Blocked:
                    Status = "*";
                    Direction = "*";
                    break;
            }

            AccountId = data.AccountId;
            Created = data.Created.AddDays(-10).TimeToString();
            Favorite = false;
        }
    }
}
