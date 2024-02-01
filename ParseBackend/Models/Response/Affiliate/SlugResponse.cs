using Newtonsoft.Json;

namespace ParseBackend.Models.Response.Affiliate
{
    public class SlugResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        public SlugResponse(string code, string displayName)
        {
            Id = code;
            Slug = code;
            DisplayName = code;
            Status = "ACTIVE";
            Verified = true;
        }
    }
}
