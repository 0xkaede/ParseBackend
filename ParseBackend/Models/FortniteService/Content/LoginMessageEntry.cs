using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Content
{
    public class LoginMessageEntry : BasePagesEntry
    {
        [JsonProperty("loginmessage")]
        public Message LoginMessage { get; set; }
    }
}