using Newtonsoft.Json;
using ParseBackend.Enums;
using ParseBackend.Models.Other.Database.Other;
using ParseBackend.Xmpp.Payloads;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IFriendService
    {
        public Task<bool> SendFriendRequest(string fromId, string toId);
        public Task<bool> AcceptFriendRequest(string fromId, string toId);
        public Task<bool> DeleteFriend(string fromId, string toId);
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
            var sendersProfiles = await _mongoService.GetAllProfileData(accountId);
            var receiverProfiles = await _mongoService.GetAllProfileData(friendId);

            var sender = sendersProfiles.FriendsData;
            var recever = receiverProfiles.FriendsData;
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

            var sendersProfiles = await _mongoService.GetAllProfileData(fromId);
            var receiverProfiles = await _mongoService.GetAllProfileData(toId);

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

            receiverProfiles.FriendsData.List.Add(inCommongModel);
            sendersProfiles.FriendsData.List.Add(outGoingModel);

            return true;
        }

        public async Task<bool> AcceptFriendRequest(string fromId, string toId)
        {
            if (!await ValidateFriendAdd(fromId, toId)) return false;

            var fromFriends = await _mongoService.GetAllProfileData(fromId);
            var toFriends = await _mongoService.GetAllProfileData(toId);

            var incomingFind = fromFriends.FriendsData.List.Where(x => x.Status is FriendsStatus.Incoming)
                .FirstOrDefault(x => x.AccountId == toFriends.FriendsData.AccountId);

            if (incomingFind != null)
            {
                fromFriends.FriendsData.List.Where(x => x.Status is FriendsStatus.Incoming)
                    .FirstOrDefault(x => x.AccountId == toFriends.FriendsData.AccountId)!.Status = FriendsStatus.Accepted;

                fromFriends.FriendsData.List.Where(x => x.Status is FriendsStatus.Incoming)
                    .FirstOrDefault(x => x.AccountId == toFriends.FriendsData.AccountId)!.Created = DateTime.Now;

                var client = GlobalXmppServer.FindClientFromAccountId(fromId);

                if(client != null)
                {
                    var payload = new PayLoad<Friend>
                    {
                        Payload = new Friend
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

            var toFind = toFriends.FriendsData.List.Where(x => x.Status is FriendsStatus.Outgoing)
                    .FirstOrDefault(x => x.AccountId == fromId);

            if (toFind != null)
            {
                toFriends.FriendsData.List.Where(x => x.Status is FriendsStatus.Outgoing)
                    .FirstOrDefault(x => x.AccountId == fromId)!.Status = FriendsStatus.Accepted;

                toFriends.FriendsData.List.Where(x => x.Status is FriendsStatus.Outgoing)
                    .FirstOrDefault(x => x.AccountId == fromId)!.Created = DateTime.Now;

                var client = GlobalXmppServer.FindClientFromAccountId(toId);

                if (client != null)
                {
                    var payload = new PayLoad<Friend>
                    {
                        Payload = new Friend
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

            return true;
        }

        public async Task<bool> DeleteFriend(string fromId, string toId)
        {
            var fromFriends = await _mongoService.GetAllProfileData(fromId);
            var toFriends = await _mongoService.GetAllProfileData(toId);

            if (fromFriends == toFriends)
                return false;

            var findFromFriends = fromFriends.FriendsData.List.Where(x => x.Status != FriendsStatus.Blocked).FirstOrDefault(x => x.AccountId == toFriends.FriendsData.AccountId);
            var findToFriends = toFriends.FriendsData.List.Where(x => x.Status != FriendsStatus.Blocked).FirstOrDefault(x => x.AccountId == fromFriends.FriendsData.AccountId);

            if (findFromFriends is null)
                return false;

            fromFriends.FriendsData.List.Remove(findFromFriends);

            if (findToFriends != null)
                toFriends.FriendsData.List.Remove(findToFriends);

            var fromClient = GlobalXmppServer.FindClientFromAccountId(fromFriends.FriendsData.AccountId);
            if(fromClient != null)
            {
                var payload = new PayLoad<Reason>
                {
                    Payload = new Reason
                    {
                        AccountId = toId,
                        Reasoning = "DELETED"
                    },
                    Timestamp = CurrentTime(),
                    Type = "com.epicgames.friends.core.apiobjects.FriendRemoval",
                };
                fromClient.SendMessage(JsonConvert.SerializeObject(payload));

                fromClient.GetPresenceFromFriends();
            }

            var toClient = GlobalXmppServer.FindClientFromAccountId(toFriends.FriendsData.AccountId);
            if (toClient != null)
            {
                var payload = new PayLoad<Reason>
                {
                    Payload = new Reason
                    {
                        AccountId = fromId,
                        Reasoning = "DELETED"
                    },
                    Timestamp = CurrentTime(),
                    Type = "com.epicgames.friends.core.apiobjects.FriendRemoval",
                };
                toClient.SendMessage(JsonConvert.SerializeObject(payload));

                toClient.GetPresenceFromFriends();
            }


            return true;
        }
    }
}
