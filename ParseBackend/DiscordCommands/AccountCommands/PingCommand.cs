using Discord.WebSocket;
using Discord;
using ParseBackend.Enums;
using ParseBackend.Services;

namespace ParseBackend.DiscordCommands.AccountCommands
{
    public class PingCommand : BaseCommand
    {
        public override string Name { get; set; } = "ping";
        public override string Description { get; set; } = "hi there!";

        public override DiscordExecuteType Type { get; set; } = DiscordExecuteType.Mongo;
        public override List<SlashCommandOptionBuilder> Options { get; set; } = new List<SlashCommandOptionBuilder>();

        public override async Task Execute(SocketSlashCommand command, IMongoService mongoService)
        {
            await command.RespondAsync("pong");
        }
    }
}
