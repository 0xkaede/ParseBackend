using CUE4Parse;
using Newtonsoft.Json;
using ParseBackend.Enums;
using ParseBackend.Models.Database.Other;
using ParseBackend.Models.Friends;
using ParseBackend.Xmpp.Payloads;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IFriendService
    {
        public Task<bool> SendFriendRequest(string fromId, string toId);
        public Task<bool> AcceptFriendRequest(string fromId, string toId);
    }

    public class FriendService : IFriendService
    {
        private readonly IMongoService _mongoService;

        public FriendService(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task<bool> ValidateFriendAdd(string accountId, string friendId)
        {
            var sender = await _mongoService.FindFriendsByAccountId(accountId);
            var recever = await _mongoService.FindFriendsByAccountId(friendId);
            if (sender is null || recever is null) return false;

            if (sender.List.Where(x => x.Status is FriendsStatus.Accepted).FirstOrDefault(x => x.AccountId == recever.AccountId) != null ||
                recever.List.Where(x => x.Status is FriendsStatus.Accepted).FirstOrDefault(x => x.AccountId == sender.AccountId) != null)
                return false;

            if (sender.List.Where(x => x.Status is FriendsStatus.Blocked).FirstOrDefault(x => x.AccountId == recever.AccountId) != null ||
                recever.List.Where(x => x.Status is FriendsStatus.Blocked).FirstOrDefault(x => x.AccountId == sender.AccountId) != null)
                return false;

            return true;
        }

        public async Task<bool> SendFriendRequest(string fromId, string toId)
        {
            if (!await ValidateFriendAdd(fromId, toId)) return false;

            var fromFriends = await _mongoService.FindFriendsByAccountId(fromId);
            var toFriends = await _mongoService.FindFriendsByAccountId(toId);

            var outGoingModel = new FriendsListData
            {
                AccountId = toId,
                Status = FriendsStatus.Outgoing,
                Created = DateTime.Now
            };

            var outClient = GlobalXmppServer.FindClientFromAccountId(fromId);

            if(outClient !=  null)
            {
                var payloadOut = new PayLoad<Xmpp.Payloads.Friend>
                {
                    Payload = new Xmpp.Payloads.Friend
                    {
                        AccountId = toId,
                        Created = CurrentTime(),
                        Status = "PENDING",
                        Direction = "OUTBOUND",
                        Favorite = false
                    },
                    Timestamp = CurrentTime(),
                    Type = "com.epicgames.friends.core.apiobjects.Friend",
                };

                outClient.SendMessage(JsonConvert.SerializeObject(payloadOut));
            }

            var inCommongModel = new FriendsListData
            {
                AccountId = fromId,
                Status = FriendsStatus.Incoming,
                Created = DateTime.Now
            };
     
            var inClient = GlobalXmppServer.FindClientFromAccountId(toId);

            if(inClient != null)
            {
                var payloadIn = new PayLoad<Xmpp.Payloads.Friend>
                {
                    Payload = new Xmpp.Payloads.Friend
                    {
                        AccountId = fromId,
                        Created = CurrentTime(),
                        Status = "PENDING",
                        Direction = "INBOUND",
                        Favorite = false
                    },
                    Timestamp = CurrentTime(),
                    Type = "com.epicgames.friends.core.apiobjects.Friend",
                };

                inClient.SendMessage(JsonConvert.SerializeObject(payloadIn));
            }


            _mongoService.AddItemToFriends(toId, inCommongModel);
            _mongoService.AddItemToFriends(fromId, outGoingModel);

            return true;
        }

        public async Task<bool> AcceptFriendRequest(string fromId, string toId)
        {
            if (!await ValidateFriendAdd(fromId, toId)) return false;

            var fromFriends = await _mongoService.FindFriendsByAccountId(fromId);
            var toFriends = await _mongoService.FindFriendsByAccountId(toId);

            var incomingFind = fromFriends.List.Where(x => x.Status is FriendsStatus.Incoming)
                .FirstOrDefault(x => x.AccountId == toFriends.AccountId);

            if(incomingFind != null)
            {
                incomingFind.Status = FriendsStatus.Accepted;
                incomingFind.Created = DateTime.Now;

                var client = GlobalXmppServer.FindClientFromAccountId(fromId);

                if(client != null)
                {
                    var payload = new PayLoad<Xmpp.Payloads.Friend>
                    {
                        Payload = new Xmpp.Payloads.Friend
                        {
                            AccountId = toId,
                            Created = CurrentTime(),
                            Status = "ACCEPTED",
                            Direction = "OUTBOUND",
                            Favorite = false
                        },
                        Timestamp = CurrentTime(),
                        Type = "com.epicgames.friends.core.apiobjects.Friend",
                    };
                    client!.SendMessage(JsonConvert.SerializeObject(payload));
                    client.GetPresenceFromFriends();
                }
            }

            var toFind = toFriends.List.Where(x => x.Status is FriendsStatus.Outgoing)
                    .FirstOrDefault(x => x.AccountId == fromId);

            if (toFind != null)
            {
                toFind.Status = FriendsStatus.Accepted;
                toFind.Created = DateTime.Now;

                var client = GlobalXmppServer.FindClientFromAccountId(toId);

                if (client != null)
                {
                    var payload = new PayLoad<Xmpp.Payloads.Friend>
                    {
                        Payload = new Xmpp.Payloads.Friend
                        {
                            AccountId = fromId,
                            Status = "ACCEPTED",
                            Direction = "OUTBOUND",
                            Created = CurrentTime(),
                            Favorite = false,
                        },
                        Timestamp = CurrentTime(),
                        Type = "com.epicgames.friends.core.apiobjects.Friend",
                    };
                    client.SendMessage(JsonConvert.SerializeObject(payload));

                    client.GetPresenceFromFriends();
                }
            }

            if(incomingFind != null && toFind != null)
            {
                _mongoService.UpdateFriendsInList(toId, fromId, toFind);
                _mongoService.UpdateFriendsInList(fromId, toId, incomingFind);
            }

            return true;
        }
    }
}
