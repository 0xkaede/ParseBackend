using System.ComponentModel;

namespace ParseBackend.Enums
{
    public enum CreateAccountResponse
    {
        [Description("You already have an account!")] DiscordId,

        [Description("The username has already been taken!")] Username,

        [Description("The email has already been taken!")] Email,

        [Description("Your new account has just been created!")] Created,
    }
}
