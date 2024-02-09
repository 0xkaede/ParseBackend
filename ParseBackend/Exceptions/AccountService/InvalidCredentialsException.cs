namespace ParseBackend.Exceptions.AccountService
{
    public class InvalidCredentialsException : BaseException
    {
        public InvalidCredentialsException()
            : base("errors.com.epicgames.account.invalid_account_credentials", "Your username and/or password are incorrect. Please check them and try again.", 18031, "invalid_grant")
        {
            StatusCode = 401;
        }
    }
}
