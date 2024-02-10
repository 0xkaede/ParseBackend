using Jose;
using Newtonsoft.Json;
using ParseBackend.Exceptions.AccountService;

namespace ParseBackend.Utils
{
    public class ContextUtils
    {
        public static AccessToken VerifyToken(HttpContext context)
        {
            //if (IsDebug())
             //   return;

            if (context.Request.Headers.Authorization.ToString() is null ||
                !context.Request.Headers.Authorization.ToString().Contains("bearer eg1~"))
            {
                throw new InvalidTokenException();
            }

            try
            {
                var token = context.Request.Headers.Authorization.ToString().Replace("bearer eg1~", "");

                var decodedAccess = JsonConvert.DeserializeObject<AccessToken>(JWT.Decode(token));

                var findToken = TokensUtils.AccessTokens.FirstOrDefault(x => x.AccessToken == $"eg1~{token}");

                if (findToken is null)
                {
                    throw new InvalidTokenException();
                }

                return decodedAccess;
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException();
            }
        }
    }
}
