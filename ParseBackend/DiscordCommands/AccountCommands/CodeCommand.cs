using Discord;
using Discord.WebSocket;
using ParseBackend.Enums;
using ParseBackend.Services;

namespace ParseBackend.DiscordCommands.AccountCommands
{
    public class CodeCommand : BaseCommand
    {
        public override string Name { get; set; } = "code";
        public override string Description { get; set; } = "creates a exchange code to login in with!";
        public override DiscordExecuteType Type { get; set; } = DiscordExecuteType.Mongo;
        public override List<SlashCommandOptionBuilder> Options { get; set; } = new List<SlashCommandOptionBuilder>();

        public override async Task Execute(SocketSlashCommand command, IMongoService mongoService)
        {
            await command.DeferAsync(true);

            var user = command.User;

            var accountId = await mongoService.GetAccountIdFromDiscordId(user.Id.ToString());

            if (accountId is null)
            {
                await command.RespondAsync("User doesnt have an account!", null, false, true);
                return;
            }

            await command.FollowupAsync("We are marking you a code, Please wait...!", null, false, true);

            var code = await mongoService.CreateExchangeCode(accountId);

            await command.FollowupAsync($"Here is your code: ||{code}|| Your code will expire in 5 miniutes and is one time use!", null, false, true);

        }
    }
}
