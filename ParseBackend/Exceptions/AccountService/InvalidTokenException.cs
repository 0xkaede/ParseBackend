namespace ParseBackend.Exceptions.AccountService
{
    public class InvalidTokenException : BaseException
    {
        public InvalidTokenException()
            : base("errors.com.epicgames.account.token", "Sorry, your token has expired please relaunch the game client.", 18006, "")
        {
            StatusCode = 400;
        }
    }
}
