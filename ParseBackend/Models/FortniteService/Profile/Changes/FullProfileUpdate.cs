using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Profile.Changes
{
    public class FullProfileUpdate : BaseProfileChange
    {
        [JsonProperty("profile")]
        public Profile Profile { get; set; }

        public FullProfileUpdate() { ChangeType = "fullProfileUpdate"; }
    }
}
