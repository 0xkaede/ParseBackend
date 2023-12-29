namespace ParseBackend.Exceptions.Common
{
    public class UnhandledErrorException : BaseException
    {
        public UnhandledErrorException(string id)
            : base("errors.com.epicgames.common.not_found", "Not the lama you was looking for.", 1005, "Lama Error")
        {
            StatusCode = 500;
        }
    }
}
