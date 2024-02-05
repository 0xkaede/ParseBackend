using Newtonsoft.Json;
using ParseBackend.Models.Database.Other;
using ParseBackend.Enums;
using static ParseBackend.Global;

namespace ParseBackend.Models.Friends
{
    public class Friend
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

        public Friend() { }

        public Friend(FriendsListData data)
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
