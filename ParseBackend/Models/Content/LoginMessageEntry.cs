using Newtonsoft.Json;

namespace ParseBackend.Models.Content
{
    public class LoginMessageEntry : BasePagesEntry
    {
        [JsonProperty("loginmessage")]
        public Message LoginMessage { get; set; }
    }
}