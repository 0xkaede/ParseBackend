using Newtonsoft.Json;

namespace ParseBackend.Models.Profile.Changes
{
    public class StatModified : BaseProfileChange
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        public StatModified() { ChangeType = "statModified"; }
    }
}
