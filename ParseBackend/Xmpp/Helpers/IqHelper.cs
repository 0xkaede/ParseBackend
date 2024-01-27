using System.Security.Cryptography;
using System.Xml.Linq;

namespace ParseBackend.Xmpp
{
    public partial class XmppClient
    {
        public void Iq(XElement data)
        {
            switch (data.Attribute("id")!.Value)
            {
                case "_xmpp_bind1":
                    {
                        var resource = data.Elements().Where(x => x.Name.LocalName == "bind").First().Elements().First().Value.ToString();
                        Resource = resource;
                        Jid = $"{UserData.AccountId}@{Domain}/{Resource}";

                        var res = new XElement(XNamespace.Get("jabber:client") + "iq",
                            new XAttribute("id", data.Attribute("id")!.Value),
                            new XAttribute("type", "result"),
                            new XAttribute("to", Jid),
                            new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-bind") + "bind",
                            new XElement("jid", Jid)));

                        Send(res.ToString());
                        break;
                    }
                default:
                    {
                        var res = new XElement(XNamespace.Get("jabber:client") + "iq",
                            new XAttribute("id", data.Attribute("id")!.Value),
                            new XAttribute("type", "result"),
                            new XAttribute("from", Domain!),
                            new XAttribute("to", Jid!));

                        Send(res.ToString());

                        GetPresenceFromFriends();

                        break;
                    }
            
            }
        }
    }
}
