using Fleck;
using Newtonsoft.Json;
using ParseBackend.Models.Other.Database;
using System.Xml.Linq;
using static ParseBackend.Global;

namespace ParseBackend.Xmpp
{
    public sealed partial class XmppClient
    {
        public IWebSocketConnection Websocket { get; set; }
        public string? Domain { get; set; }
        public string? Jid { get; set; }
        public bool IsAuthenticated { get; set; }
        public UserData? UserData { get; set; }
        public string? Resource { get; set; }
        public object? Status { get; set; }
        public bool IsAway { get; set; }
        public Dictionary<string, List<XmppClient>> JoinedMucs { get; set; }
        public string ID => Websocket.ConnectionInfo.Id.ToString();

        public void OnMessage(string message)
        {
            var element = XElement.Parse(message);

            switch (element.Name.LocalName)
            {
                case "open":
                    Open();
                    break;
                case "auth":
                    Auth(element);
                    break;
                case "iq":
                    Iq(element);
                    break;
                case "presence":
                    Presence(element);
                    break;
                case "message":
                    Message(element);
                    break;
            }
        }

        async void Send(string msg) => await Websocket.Send(msg);

        public void GetPresenceFromFriends()
        {
            var friendData = GlobalMongoService.FindFriendsByAccountId(UserData!.AccountId).Result;

            var acceptedFriends = friendData.List.Where(x => x.Status == Enums.FriendsStatus.Accepted).ToList();

            foreach (var friend in acceptedFriends)
            {
                var friendClient = FindClientFromAccountId(friend.AccountId);

                if (friendClient is null)
                    return;

                var res = new XElement(XNamespace.Get("jabber:client") + "presence",
                        new XAttribute("to", Jid!),
                        new XAttribute("from", friendClient.Jid!),
                        new XAttribute("type", "available"));

                if (friendClient.IsAway)
                    res.Add(new XElement("show", "away"));

                res.Add(new XElement("status", JsonConvert.SerializeObject(friendClient.Status)));

                Send(res.ToString().Replace(" xmlns=\"\"", ""));
            }
        }

        public void GetPresenceForFriends(object Status, bool bAway, bool bClose)
        {
            var friendData = GlobalMongoService.FindFriendsByAccountId(UserData!.AccountId).Result;

            var acceptedFriends = friendData.List.Where(x => x.Status == Enums.FriendsStatus.Accepted).ToList();

            foreach (var friend in acceptedFriends)
            {
                var friendClient = FindClientFromAccountId(friend.AccountId);

                if (friendClient is null)
                    return;

                var res = new XElement(XNamespace.Get("jabber:client") + "presence",
                       new XAttribute("to", friendClient.Jid!),
                       new XAttribute("from", Jid!),
                       new XAttribute("type", bClose ? "unavailable" : "available"));

                if (bAway)
                    res.Add(new XElement("show", "away"));

                res.Add(new XElement("status", JsonConvert.SerializeObject(Status)));

                friendClient.Send(res.ToString().Replace(" xmlns=\"\"", ""));
            }
        }

        public XmppClient? FindClientFromAccountId(string AccountId)
        {
            foreach (var Client in GlobalClients.Values)
                if (Client.UserData != null)
                    if (Client.UserData.AccountId == AccountId)
                        return Client;

            return null;
        }

        public void SendMessage(string message)
            => Send(new XElement(XNamespace.Get("jabber:client") + "message",
                new XAttribute("to", Jid!),
                new XAttribute("from", $"xmpp-admin@{Domain}"),
                new XAttribute("id", CreateUuid()),
                new XElement("body", message)).ToString().Replace(" xmlns=\"\"", ""));

        public void SendMessage(string to, string from, string message)
        {
            var client = GlobalXmppServer.FindClientFromAccountId(to.Split("@")[0]);

            if (client != null)
            {
                var Response = new XElement(XNamespace.Get("jabber:client") + "message",
                    new XAttribute("to", to),
                    new XAttribute("from", from),
                    new XAttribute("id", CreateUuid()),
                    new XElement("body", message));

                client.Send(Response.ToString().Replace(" xmlns=\"\"", ""));
            }
        }


    }
}
