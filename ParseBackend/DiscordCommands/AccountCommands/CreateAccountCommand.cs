using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using ParseBackend.Enums;
using ParseBackend.Services;
using ParseBackend.Utils;
using static ParseBackend.Global;

namespace ParseBackend.DiscordCommands.AccountCommands
{
    public class CreateAccountCommand : BaseCommand
    {
        public override string Name { get; set; } = "create";
        public override string Description { get; set; } = "Creates a users account!";

        public override DiscordExecuteType Type { get; set; } = DiscordExecuteType.Mongo;
        public override List<SlashCommandOptionBuilder> Options { get; set; } = new List<SlashCommandOptionBuilder>
        {
            new SlashCommandOptionBuilder()
            {
                Name = "username",
                Description = "Pick a username for your account!",
                IsRequired = true,
                Type = ApplicationCommandOptionType.String
            },
            new SlashCommandOptionBuilder()
            {
                Name = "email",
                Description = "Pick a email for your account!",
                IsRequired = true,
                Type = ApplicationCommandOptionType.String
            },
            new SlashCommandOptionBuilder()
            {
                Name = "password",
                Description = "Pick a secure password!",
                IsRequired = true,
                Type = ApplicationCommandOptionType.String
            }

        };

        public override async Task Execute(SocketSlashCommand command, IMongoService mongoService)
        {
            await command.DeferAsync(true);

            var commandList = command.Data.Options.ToList();
            var username = commandList.FirstOrDefault(x => x.Name == "username")!.Value.ToString()!;
            var email = commandList.FirstOrDefault(x => x.Name == "email")!.Value.ToString()!;
            var password = commandList.FirstOrDefault(x => x.Name == "password")!.Value.ToString()!;

            if (username.Length < 4 || username.Length > 16)
            {
                await command.RespondAsync("Your username is too short or long (4-16 characters).", null, false, true);
                return;
            }


            var msg = await mongoService.CreateAccount(email, password, username, command.User.Id.ToString());

            await command.FollowupAsync(msg.GetDescription(), null, false, true);
        }
    }
}
