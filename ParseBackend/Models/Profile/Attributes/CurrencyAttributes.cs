using Newtonsoft.Json;

namespace ParseBackend.Models.Profile.Attributes
{
    public class CurrencyAttributes
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
    }
}
