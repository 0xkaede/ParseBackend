using Newtonsoft.Json;
using ParseBackend.Models.Profile;

namespace ParseBackend.Models.Profile.Changes
{
    public class FullProfileUpdate : BaseProfileChange
    {
        [JsonProperty("profile")]
        public Profile Profile { get; set; }

        public FullProfileUpdate() { ChangeType = "fullProfileUpdate"; }
    }
}
