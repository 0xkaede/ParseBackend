using Discord.WebSocket;

namespace ParseBackend.Services
{
    public interface IDiscordService
    {

    }

    public class DiscordService : IDiscordService
    {
        public DiscordSocketClient? Client { get; set; }
        //public List<BaseCommand> Commands { get; set; } = new();


        public DiscordService() { }


    }
}
