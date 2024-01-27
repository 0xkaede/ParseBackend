using ParseBackend;
using ParseBackend.Xmpp;
using System.Net.Mail;

namespace ParseBackend.Models.Xmpp
{
    public class MUCRoom
    {
        public string Name { get; set; }
        public List<XmppClient> MucClients { get; set; } = new();
    }
}
