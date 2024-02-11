using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using ParseBackend.Enums;
using ParseBackend.Services;
using ParseBackend.Xmpp.Payloads;
using static ParseBackend.Global;

namespace ParseBackend.DiscordCommands.AdminCommands
{
    public class GrantFullLockerCommand : BaseCommand
    {
        public override string Name { get; set; } = "grantfulllocker";
        public override string Description { get; set; } = "grants the user full locker (every item)!";
        public override DiscordExecuteType Type { get; set; } = DiscordExecuteType.Mongo;
        public override List<SlashCommandOptionBuilder> Options { get; set; } = new List<SlashCommandOptionBuilder>
        {
            new SlashCommandOptionBuilder
            {
                Name = "user",
                Description = "Pick a user to grant full locker!",
                IsRequired = true,
                Type = ApplicationCommandOptionType.User
            }
        };

        public GrantFullLockerCommand() { }

        public override async Task Execute(SocketSlashCommand command, IMongoService mongoService)
        {
            await command.DeferAsync(true);

            var commandList = command.Data.Options.ToList();
            var user = (SocketUser)commandList.FirstOrDefault(x => x.Name == "user")!.Value!;

            var accountId = await mongoService.GetAccountIdFromDiscordId(user.Id.ToString());

            if(accountId is null)
            {
                await command.RespondAsync("User doesnt have an account!", null, false, true);
                return;
            }

            await command.FollowupAsync("We are getting all cosmetic data, please wait...!", null, false, true);

            await mongoService.GrantAthenaFullLockerAsync(accountId);

            await user.SendMessageAsync($"You have been granted full locker from {command.User.Username}.");

            var client = GlobalXmppServer.FindClientFromAccountId(accountId);
            if (client != null)
            {
                client.SendMessage(JsonConvert.SerializeObject(new PayLoad<object>
                {
                    Payload = new object(),
                    Timestamp = CurrentTime(),
                    Type = "com.epicgames.gift.received"
                }));
            }

            await command.FollowupAsync("Granted User Full locker!", null, false, true);
        }
    }
}
