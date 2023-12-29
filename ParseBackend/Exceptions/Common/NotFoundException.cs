namespace ParseBackend.Exceptions.Common
{
    public class NotFoundException : BaseException
    {
        public NotFoundException()
            : base("errors.com.epicgames.common.not_found", "Sorry the resource you were trying to find could not be found.", 1004, "")
        {
            StatusCode = 404;
        }
    }
}
