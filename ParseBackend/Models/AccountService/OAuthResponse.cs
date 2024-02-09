using Newtonsoft.Json;

namespace ParseBackend.Models.AccountService
{
    public class OAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; internal set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; internal set; }

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; internal set; }

        [JsonIgnore]
        public bool HasSessionExpired => DateTime.UtcNow.CompareTo(ExpiresAt) > 0;

        [JsonProperty("token_type")]
        public string TokenType { get; internal set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; internal set; }

        [JsonProperty("refresh_expires")]
        public int RefreshExpires { get; internal set; }

        [JsonProperty("refresh_expires_at")]
        public DateTime RefreshExpiresAt { get; internal set; }

        [JsonIgnore]
        public bool HasRefreshExpired => DateTime.UtcNow.CompareTo(RefreshExpiresAt) > 0;

        [JsonProperty("account_id")]
        public string AccountId { get; internal set; }

        [JsonProperty("client_id")]
        public string ClientId { get; internal set; }

        [JsonProperty("internal_client")]
        public bool InternalClient { get; internal set; }

        [JsonProperty("client_service")]
        public string ClientService { get; internal set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; internal set; }

        [JsonProperty("app")]
        public string App { get; internal set; }

        [JsonProperty("in_app_id")]
        public string InAppId { get; internal set; }

        [JsonProperty("device_id")]
        public string DeviceId { get; internal set; }
    }
}
