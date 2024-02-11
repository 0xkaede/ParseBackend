using Discord.WebSocket;
using Discord;
using ParseBackend.Utils;
using ParseBackend.Services;
using ParseBackend.Enums;

namespace ParseBackend.DiscordCommands
{
    public class BaseCommand
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual List<SlashCommandOptionBuilder> Options { get; set; }
        public virtual DiscordExecuteType Type { get; set; }

        public BaseCommand() { }

        public virtual async Task Execute(SocketSlashCommand command, IMongoService mongoService)
            => Logger.Log("Discord tried to execute nothing");

        public virtual async Task Execute(SocketSlashCommand command)
        => Logger.Log("Discord tried to execute nothing");
    }
}
