using Newtonsoft.Json;

namespace ParseBackend.Models.Profile.Changes
{
    public class BaseProfileChange
    {
        [JsonProperty("changeType")]
        public string ChangeType { get; set; }
    }
}
