using Fleck;
using ParseBackend.Utils;
using System.Xml.Linq;
using static ParseBackend.Global;

namespace ParseBackend.Xmpp
{
    public class XmppServer //i hate xmpp stuff :(
    {
        public void StartServer()
        {
            FleckLog.LogAction = (level, message, ex) => { };

            var server = new WebSocketServer($"ws://0.0.0.0:443"); //credits to elixir.api
            server.Start(s =>
            {
                s.OnOpen = () => OnOpen(s);
                s.OnClose = () => OnClose(s);
                s.OnMessage = (message) => OnMessage(s, message);
            });

            Logger.Log($"Xmpp started on port {server.Port}", Utils.LogLevel.Xmpp);
        }

        private void OnOpen(IWebSocketConnection connection)
        {
        }

        private void OnClose(IWebSocketConnection connection)
        {
            var id = connection.ConnectionInfo.Id.ToString();

            var client = GlobalClients.FirstOrDefault(x => x.Key == id).Value;

            if (client is null)
                return;

            foreach (var room in GlobalMucRooms)
            {
                if (!room.Value.Contains(client))
                    continue;

                room.Value.Remove(client);

                if (room.Value.Count <= 0)
                    GlobalMucRooms.Remove(id);
            }

            GlobalClients.Remove(id);
        }

        private void OnMessage(IWebSocketConnection connection, string message)
        {
            var element = XElement.Parse(message);
            var id = connection.ConnectionInfo.Id.ToString();
            var client = GlobalClients.FirstOrDefault(x => x.Key == id).Value;

            switch(element.Name.LocalName)
            {
                case "open":
                    {
                        if (client != null)
                        {
                            client.OnMessage(message);
                            return;
                        }

                        client = new XmppClient
                        {
                            Websocket = connection,
                            Domain = element.Attribute("to")!.Value,
                            Jid = "",
                            IsAuthenticated = false
                        };

                        GlobalClients.Add(client.Websocket.ConnectionInfo.Id.ToString(), client);
  
                        connection.Send(new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-framing") + "open",
                            new XAttribute("from", client.Domain),
                            new XAttribute("id", client.Websocket.ConnectionInfo.Id),
                            new XAttribute("version", "1.0"),
                            new XAttribute(XNamespace.Get("xml") + "lang", "en")).ToString());

                        connection.Send(new XElement(XNamespace.Get("http://etherx.jabber.org/streams") + "features",
                            new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-sasl") + "mechanisms",
                            new XElement("mechanism", "PLAIN")),
                            new XElement(XNamespace.Get("urn:xmpp:features:rosterver") + "ver"),
                            new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-tls") + "starttls"),
                            new XElement(XNamespace.Get("http://jabber.org/features/compress") + "compression",
                            new XElement("method", "zlib")),
                            new XElement(XNamespace.Get("http://jabber.org/features/iq-auth") + "auth")).ToString());
                        break;
                    }
                default:
                    {
                        client.OnMessage(message);
                        break;
                    }
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

    }
}