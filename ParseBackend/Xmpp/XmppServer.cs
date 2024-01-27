using Amazon.Auth.AccessControlPolicy;
using CUE4Parse;
using Fleck;
using Jose;
using Newtonsoft.Json;
using ParseBackend.Models.Content;
using ParseBackend.Models.Database;
using ParseBackend.Models.Xmpp;
using ParseBackend.Utils;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using static ParseBackend.Global;

namespace ParseBackend.Xmpp
{
    public class XmppServer //i hate xmpp stuff :(
    {
        public void StartServer()
        {
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
                if (!room.MucClients.Contains(client))
                    continue;

                room.MucClients.Remove(client);

                if (room.MucClients.Count <= 0)
                    GlobalMucRooms.Remove(room);
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

                        GlobalClients.Add(id, client);
  
                        connection.Send(new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-framing") + "open",
                            new XAttribute("from", client.Domain),
                            new XAttribute("id", id),
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