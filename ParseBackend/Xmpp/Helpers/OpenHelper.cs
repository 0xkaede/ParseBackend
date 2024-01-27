using System.Xml.Linq;

namespace ParseBackend.Xmpp
{
    public partial class XmppClient
    {
        private async void Open()
        {
            if (IsAuthenticated)
            {
                Send(new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-framing") + "open",
                    new XAttribute("from", Domain!),
                    new XAttribute("id", Websocket.ConnectionInfo.Id),
                    new XAttribute("version", "1.0"),
                    new XAttribute(XNamespace.Get("xml") + "lang", "en")).ToString());

                Send(new XElement(XNamespace.Get("http://etherx.jabber.org/streams") + "features",
                    new XElement(XNamespace.Get("urn:xmpp:features:rosterver") + "ver"),
                    new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-tls") + "starttls"),
                    new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-bind") + "bind"),
                    new XElement(XNamespace.Get("http://jabber.org/features/compress") + "compression",
                        new XElement("method", "zlib")),
                    new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-session") + "session")).ToString());
            }
        }
    }
}
