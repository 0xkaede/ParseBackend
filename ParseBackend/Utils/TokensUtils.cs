namespace ParseBackend.Utils
{
    public class TokensUtils
    {
        public static List<AccessTokensGlobal> AccessTokens = new List<AccessTokensGlobal>();
        public static List<AccessTokensGlobal> RefreshTokens = new List<AccessTokensGlobal>();
    }

    public class AccessTokensGlobal
    {
        public string AccessToken { get; set; }
        public string AccountId { get; set; }
    }
}
