using CUE4Parse;
using Jose;
using Newtonsoft.Json;
using ParseBackend.Utils;
using System.Buffers.Text;
using System.Xml.Linq;
using static ParseBackend.Global;

namespace ParseBackend.Xmpp
{
    public partial class XmppClient
    {
        private async void Auth(XElement data)
        {
            var decoded = data.Value.DecodeBase64();
            var split = decoded.Split("\u0000");

            var accountId = split[1];
            var token = split[2];

            UserData = await GlobalMongoService.FindUserByAccountId(accountId);

            var errres = new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-sasl") + "failure",
                    new XElement("not-authorized",
                    new XElement("text",
                    new XAttribute(XNamespace.Get("xml") + "lang", "en"),
                    new XAttribute("text", "Password not verified")))).ToString();

            if (UserData is null)
            {
                Send(errres);
                return;
            }

            var accessToken = token.Replace("bearer eg1~", "");

            accessToken = accessToken.Contains("eg1~") ? accessToken.Replace("eg1~", "") : accessToken;

            var decodedAccess = JsonConvert.DeserializeObject<AccessToken>(JWT.Decode(accessToken));

            if(decodedAccess is null || decodedAccess.Sub != accountId)
            {
                Send(errres);
                return;
            }

            IsAuthenticated = true;

            Logger.Log($"User \"{UserData.Username}\" has logged in!", Utils.LogLevel.Xmpp);

            var res = new XElement(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-sasl") + "success");
            Send(res.ToString());
        }
    }
}
