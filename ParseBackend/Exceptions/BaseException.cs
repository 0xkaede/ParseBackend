using Newtonsoft.Json;

namespace ParseBackend.Exceptions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BaseException : Exception
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("errorCode")]
        public string ErrorType { get; set; } = "errors.com.epicgames.common.not_found";

        [JsonProperty("errorMessage")]
        public string ErrorMessage => base.Message;

        [JsonProperty("errorDescription")]
        public string ErrorDescription => base.Message;

        [JsonProperty("numericErrorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty("intent")]
        public string Intent => "prod";

        [JsonProperty("messageVars")]
        public List<string> MessageVars { get; set; } = new List<string>();

        public int StatusCode = 400;

        public BaseException(string code, string message, int numberCode, string err)
            : base(string.Format(message))
        {
            Error = err;
            ErrorType = code;
            ErrorCode = numberCode;
        }
    }
}
