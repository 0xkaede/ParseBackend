using CUE4Parse;
using Newtonsoft.Json;
using ParseBackend.Models.Xmpp;
using System.Xml.Linq;
using static ParseBackend.Global;

namespace ParseBackend.Xmpp
{
    public partial class XmppClient
    {
        public void Presence(XElement data)
        {
            bool bHasX = false;
            bool bHasType = false;

            foreach (var element in data.Elements().ToList())
            {
                if (element.Name.LocalName == "x")
                    bHasX = true;
                else if (element.Name.LocalName == "type")
                    bHasType = true;
            }

            if (bHasType)
            {
                if (data.Attribute("to") == null) return;

                var type = data.Attribute("type")!.Value.ToString();
                var to = data.Attribute("to")!.Value.ToString();

                if (type is "unavailable")
                {
                    var name = to.Split("@")[0];

                    var room = GlobalMucRooms.FirstOrDefault(x => x.Name == name);

                    if (room is null)
                        return;

                    var client = room.MucClients.Find(x => x == this);
                    if (client != null)
                        room.MucClients.Remove(client);

                    Send(new XElement(XNamespace.Get("jabber:client") + "presence",
                        new XAttribute("to", Jid!),
                        new XAttribute("from", $"{room.Name}@muc.{Domain!}/{UserData.Username}:{UserData.AccountId}:{Resource}"),
                        new XAttribute("type", "unavailable"),
                        new XElement(XNamespace.Get("http://jabber.org/protocol/muc#user") + "x"),
                        new XElement("items",
                            new XAttribute("nick", $"{UserData.Username}:{UserData.AccountId}:{Resource}"),
                            new XAttribute("jid", Jid!),
                            new XAttribute("role", "none")),
                       new XElement("status",
                            new XAttribute("code", "110")),
                       new XElement("status",
                            new XAttribute("code", "100")),
                       new XElement("status",
                            new XAttribute("code", "170"))).ToString().Replace(" xmlns=\"\"", ""));

                    if (room.MucClients.Count <= 0)
                        GlobalMucRooms.Remove(room);

                    return;
                }
            }

            if (bHasX)
            {
                if (data.Attribute("to") is null)
                    return;

                var to = data.Attribute("to")!.Value.ToString();

                if (!to.Contains("@muc."))
                    return;

                var name = to.Split("@")[0];

                var room = GlobalMucRooms.FirstOrDefault(x => x.Name == name);

                if (room is null)
                    GlobalMucRooms.Add(new MUCRoom { Name = name });

                if (GlobalMucRooms.Find(x => x.MucClients.FindIndex(x => x.Jid!.Split("@")[0] == Jid!.Split("@")[0]) != -1) != null)
                    return;

                GlobalMucRooms.FirstOrDefault(x => x.Name == name)!.MucClients.Add(this);

                Send(new XElement(XNamespace.Get("jabber:client") + "presence",
                    new XAttribute("to", Jid!),
                    new XAttribute("from", $"{name}@muc.{Domain}/{UserData.Username}:{UserData.AccountId}:{Resource}"),
                    new XElement(XNamespace.Get("http://jabber.org/protocol/muc#user") + "x"),
                    new XElement("item",
                    new XAttribute("nick", $"{UserData!.Username}:{UserData!.AccountId}:{Resource}"),
                    new XAttribute("jid", Jid!),
                    new XAttribute("role", "participant"),
                    new XAttribute("affiliation", "none")),
                    new XElement("status",
                    new XAttribute("code", "110")),
                    new XElement("status",
                    new XAttribute("code", "100")),
                    new XElement("status",
                    new XAttribute("code", "170")),
                    new XElement("status",
                    new XAttribute("code", "201"))).ToString().Replace(" xmlns=\"\"", ""));


                var optimize = GlobalMucRooms.FirstOrDefault(x => x.Name == name)!.MucClients;
                foreach (var MucClient in optimize)
                {
                    Send(new XElement(XNamespace.Get("jabber:client") + "presence",
                        new XAttribute("from", $"{name}@muc.{MucClient.Domain!}/{MucClient.UserData!.Username}:{MucClient.UserData!.AccountId}:{MucClient.Resource}"),
                        new XAttribute("to", Jid!),
                        new XElement(XNamespace.Get("http://jabber.org/protocol/muc#user") + "x"),
                        new XElement("items",
                        new XAttribute("nick", $"{MucClient.UserData!.Username}:{MucClient.UserData!.AccountId}:{MucClient.Resource}"),
                        new XAttribute("jid", Jid!),
                        new XAttribute("role", "participant"),
                        new XAttribute("affiliation", "none"))).ToString().Replace(" xmlns=\"\"", ""));

                    if (MucClient == this) return;

                    Send(new XElement(XNamespace.Get("jabber:client") + "presence",
                        new XAttribute("from", $"{name}@muc.{Domain!}/{UserData!.Username}:{UserData!.AccountId}:{Resource}"),
                        new XAttribute("to", MucClient.Jid!),
                        new XElement(XNamespace.Get("http://jabber.org/protocol/muc#user") + "x"),
                        new XElement("items",
                        new XAttribute("nick", $"{MucClient.UserData!.Username}:{MucClient.UserData!.AccountId}:{MucClient.Resource}"),
                        new XAttribute("jid", Jid!),
                        new XAttribute("role", "participant"),
                        new XAttribute("affiliation", "none"))).ToString().Replace(" xmlns=\"\"", ""));
                }

                return;
            }

            bool bHasStatus = false;

            foreach (var element in data.Elements())
                if (element.Name.LocalName == "status")
                    bHasStatus = true;

            if (!bHasStatus)
                return;

            var statusElement = data.Elements().Where(x => x.Name.LocalName == "status").First();
            if (statusElement == null) return;


            var statusString = statusElement.Value.ToString();
            Status = JsonConvert.DeserializeObject(statusString);
            IsAway = data.Elements().ToList().FindIndex(x => x.Name.LocalName == "show") == -1 ? false : true;


            var resg = new XElement(XNamespace.Get("jabber:client") + "presence",
                new XAttribute("from", Jid!),
                new XAttribute("to", Jid!.Split("/")[0]),
                new XElement("status", JsonConvert.SerializeObject(Status)));

            if (IsAway)
                resg.Add(new XElement("show", "away"));

            Send(resg.ToString().Replace(" xmlns=\"\"", ""));

            GetPresenceForFriends(Status, IsAway, false);
            GetPresenceFromFriends();
        }
    }
}
