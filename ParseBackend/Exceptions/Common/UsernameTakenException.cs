namespace ParseBackend.Exceptions.Common
{
    public class UsernameTakenException : BaseException
    {
        public UsernameTakenException()
            : base("errors.com.epicgames.account.account_name_taken", "Sorry, that display name is already taken.", 18006, "")
        {
            StatusCode = 400;
        }
    }
}
