using Newtonsoft.Json;

namespace ParseBackend.Models.LightSwitchService
{
    public class FortniteStatus
    {
        [JsonProperty("serviceInstanceId")]
        public string ServiceInstanceId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("maintenanceUri")]
        public string MaintenanceUri { get; set; }

        [JsonProperty("overrideCatalogIds")]
        public List<string> OverrideCatalogIds { get; set; }

        [JsonProperty("allowedActions")]
        public List<string> AllowedActions { get; set; }

        [JsonProperty("banned")]
        public bool Banned { get; set; }

        [JsonProperty("launcherInfoDTO")]
        public LauncherInfoDTO LauncherInfoDTO { get; set; }
    }

    public class LauncherInfoDTO
    {
        [JsonProperty("appName")]
        public string AppName { get; set; }

        [JsonProperty("catalogItemId")]
        public string CatalogItemId { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }
    }
}
