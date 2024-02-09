using CUE4Parse;
using Discord;
using System.Xml.Linq;
using static ParseBackend.Global;

namespace ParseBackend.Xmpp
{
    public partial class XmppClient
    {
        private void Message(XElement data)
        {
            bool bHasBody = false;
            bool bHasTo = data.Attribute("to") is null ? false : true;

            foreach (var element in data.Elements())
                if(element.Name.LocalName is "body")
                    bHasBody = true;

            if (!bHasBody)
                return;

            var type = data.Attribute("type") is null 
                ? "" : data.Attribute("type")!.Value.ToString();

            switch(type)
            {
                case "chat":
                    {
                        var to = data.Attribute("to")!.Value.ToString();
                        var client = GlobalXmppServer.FindClientFromAccountId(to.Split("@")[0]);

                        if (client is null)
                            return;

                        client.Send(new XElement(XNamespace.Get("jabber:client") + "message",
                            new XAttribute("to", client.Jid!),
                            new XAttribute("from", Jid!),
                            new XAttribute("type", "chat"),
                            new XElement("body", data.Elements().Where(x => x.Name.LocalName == "body").First()!.Value)).ToString().Replace(" xmlns=\"\"", ""));

                        break;
                    }
                case "groupchat":
                    {
                        var to = data.Attribute("to")!.Value.ToString();
                        var name = to.Split("@")[0];
                        var room = GlobalMucRooms.FirstOrDefault(x => x.Key == name).Value;

                        if (room is null) 
                            return;

                        foreach(var client in room)
                        {
                            client.Send(new XElement(XNamespace.Get("jabber:client") + "message",
                                new XAttribute("to", client.Jid!),
                                new XAttribute("from", $"{room}@muc.{Domain}/{UserData.Username}:{UserData.AccountId}:{Resource}"),
                                new XAttribute("type", "groupchat"),
                                new XElement("body", data.Elements().Where(x => x.Name.LocalName == "body").First()!.Value)).ToString().Replace(" xmlns=\"\"", ""));
                        }

                        break;
                    }
                default:
                    if(bHasTo)
                    {
                        var to = data.Attribute("to")!.Value.ToString();
                        var msg = data.Elements().Where(x => x.Name.LocalName == "body").First()!.Value.ToString();

                        SendMessage(to, Jid!, msg);
                    }
                    break;
            }

            
        }

    }
}
