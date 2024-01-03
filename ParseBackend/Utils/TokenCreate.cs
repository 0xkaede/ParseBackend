using Jose;
using Newtonsoft.Json;
using ParseBackend.Models.Database;
using static ParseBackend.Global;

namespace ParseBackend.Utils
{
    public static class TokenCreate
    {
        public static string CreateAccess(UserData user, string clientId, string grant_type, string deviceId, int expiresIn)
        {
            var td = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");
            var token = JWT.Encode(new AccessToken
            {
                App = "fortnite",
                Sub = user.AccountId,
                Dvid = deviceId,
                Mver = false,
                Clid = clientId,
                Dn = user.Username,
                Am = grant_type,
                P = CreateUuid(),
                Iai = user.AccountId,
                Sec = 1,
                Clsvc = "fortnite",
                T = "s",
                Ic = true,
                Jti = CreateUuid(),
                CreationDate = td,
                HoursExpire = expiresIn,
            }, JWT_SECRET, JwsAlgorithm.none);

            //var enc = JWT.Encode(token, Utils.JWT_SECRET, JwsAlgorithm.none);
            //var dec = JWT.Decode(enc);
            //Console.WriteLine(dec);

            TokensUtils.AccessTokens.Add(new AccessTokensGlobal { AccessToken = "eg1~" + token, AccountId = user.AccountId });

            return token;
        }

        public static string CreateRefresh(UserData user, string clientId, string grant_type, string deviceId, int expiresIn)
        {
            var token = JWT.Encode(new RefreshToken
            {
                Sub = user.AccountId,
                Dvid = deviceId,
                T = "r",
                Clid = clientId,
                Am = grant_type,
                Jti = CreateUuid(),
                CreationDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                HoursExpire = expiresIn
            }, JWT_SECRET, JwsAlgorithm.none);

            TokensUtils.RefreshTokens.Add(new AccessTokensGlobal { AccessToken = "eg1~" + token, AccountId = user.AccountId });

            return token;
        }
    }

    public class AccessToken
    {
        [JsonProperty("app")]
        public string App { get; set; }

        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("dvid")]
        public string Dvid { get; set; }

        [JsonProperty("mver")]
        public bool Mver { get; set; }

        [JsonProperty("clid")]
        public string Clid { get; set; }

        [JsonProperty("dn")]
        public string Dn { get; set; }

        [JsonProperty("am")]
        public string Am { get; set; }

        [JsonProperty("p")]
        public string P { get; set; }

        [JsonProperty("iai")]
        public string Iai { get; set; }

        [JsonProperty("sec")]
        public int Sec { get; set; }

        [JsonProperty("clsvc")]
        public string Clsvc { get; set; }

        [JsonProperty("t")]
        public string T { get; set; }

        [JsonProperty("ic")]
        public bool Ic { get; set; }

        [JsonProperty("jti")]
        public string Jti { get; set; }

        [JsonProperty("creation_date")]
        public string CreationDate { get; set; } = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");

        [JsonProperty("hours_expire")]
        public int HoursExpire { get; set; } = 8;
    }

    public class RefreshToken
    {
        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("dvid")]
        public string Dvid { get; set; }

        [JsonProperty("t")]
        public string T { get; set; }

        [JsonProperty("clid")]
        public string Clid { get; set; }

        [JsonProperty("am")]
        public string Am { get; set; }

        [JsonProperty("jti")]
        public string Jti { get; set; }

        [JsonProperty("creation_date")]
        public string CreationDate { get; set; } = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");

        [JsonProperty("hours_expire")]
        public int HoursExpire { get; set; } = 24;
    }
}
