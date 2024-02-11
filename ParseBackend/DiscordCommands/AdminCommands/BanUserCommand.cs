using CUE4Parse.UE4.Versions;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using ParseBackend.Enums;
using ParseBackend.Services;
using ParseBackend.Utils;
using ParseBackend.Xmpp.Payloads;
using static ParseBackend.Global;

namespace ParseBackend.DiscordCommands.AdminCommands
{
    public class BanUserCommand : BaseCommand
    {
        public override string Name { get; set; } = "ban";
        public override string Description { get; set; } = "ban a user from game!";
        public override DiscordExecuteType Type { get; set; } = DiscordExecuteType.Mongo;
        public override List<SlashCommandOptionBuilder> Options { get; set; } = new List<SlashCommandOptionBuilder>
        {
            new SlashCommandOptionBuilder
            {
                Name = "user",
                Description = "Pick a user to ban!",
                IsRequired = true,
                Type = ApplicationCommandOptionType.User
            },
            new SlashCommandOptionBuilder
            {
                Name = "reason",
                Description = "Pick a username for your account!",
                IsRequired = true,
                Type = ApplicationCommandOptionType.Number,
                Choices = new List<ApplicationCommandOptionChoiceProperties>
                {
                    new ApplicationCommandOptionChoiceProperties
                    {
                        Name = BannedReason.Exploiting.GetDescription(),
                        Value = (int)BannedReason.Exploiting
                    }
                }
            },
            new SlashCommandOptionBuilder
            {
                Name = "type",
                Description = "what type!",
                IsRequired = true,
                Type = ApplicationCommandOptionType.Number,
                Choices = new List<ApplicationCommandOptionChoiceProperties>
                {
                    new ApplicationCommandOptionChoiceProperties
                    {
                        Name = "Permanent",
                        Value = (int)BannedType.Perm,
                        
                    },
                    new ApplicationCommandOptionChoiceProperties
                    {
                        Name = "MatchMaking",
                        Value = (int)BannedType.MatchMaking
                    },
                }
            },
            new SlashCommandOptionBuilder
            {
                Name = "days",
                Description = "how many days to ban the user",
                IsRequired = true,
                Type = ApplicationCommandOptionType.Integer,
                IsAutocomplete = true,
            }
        };

        public override async Task Execute(SocketSlashCommand command, IMongoService mongoService)
        {
            await command.DeferAsync(true);

            var commandList = command.Data.Options.ToList();

            await command.FollowupAsync("We are changing the users data, Please wait...!", null, false, true);

            var user = commandList.FirstOrDefault(x => x.Name == "user")!.Value! as SocketUser;
            var reason = commandList.FirstOrDefault(x => x.Name == "reason")!.Value!.ObjectToInt();
            var type = commandList.FirstOrDefault(x => x.Name == "type")!.Value!.ObjectToInt();
            var days = commandList.FirstOrDefault(x => x.Name == "days")!.Value!.ObjectToInt();

            var accountId = await mongoService.GetAccountIdFromDiscordId(user.Id.ToString());

            if (accountId is null)
            {
                await command.RespondAsync("User doesnt have an account!", null, false, true);
                return;
            }

            var profiles = await mongoService.GetAllProfileData(accountId);

            profiles.UserData.BannedData.IsBanned = true;
            profiles.UserData.BannedData.DateBanned = CurrentTime();
            profiles.UserData.BannedData.Days = days;
            profiles.UserData.BannedData.Reason = (BannedReason)reason;
            profiles.UserData.BannedData.Type = (BannedType)type;

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

            await command.FollowupAsync("The user was banned!", null, false, true);
        }
    }
}
