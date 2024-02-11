using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using ParseBackend.Enums;
using ParseBackend.Services;
using ParseBackend.Xmpp.Payloads;
using static ParseBackend.Global;

namespace ParseBackend.DiscordCommands.LockerCommands
{
    public class ClearLockerNotifications : BaseCommand
    {
        public override string Name { get; set; } = "clearlockernotifications";
        public override string Description { get; set; } = "Clears all unchecked items in your lockers (also updates in real time)";
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

            await command.FollowupAsync("We are marking every item to seen, Please wait...!", null, false, true);

            var profiles = await mongoService.GetAllProfileData(accountId);

            profiles.AthenaData.SeeEveryItem();

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

            await command.FollowupAsync("Your locker notifications has been cleared!", null, false, true);
        }
    }
}
