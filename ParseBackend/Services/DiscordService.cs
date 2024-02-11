using Discord;
using Discord.WebSocket;
using ParseBackend.DiscordCommands;
using ParseBackend.DiscordCommands.AccountCommands;
using ParseBackend.DiscordCommands.AdminCommands;
using ParseBackend.DiscordCommands.LockerCommands;
using ParseBackend.Utils;
using static ParseBackend.Global;
using LogLevel = ParseBackend.Utils.LogLevel;

namespace ParseBackend.Services
{
    public interface IDiscordService
    {
        public void Ping();
    }

    public class DiscordService : IDiscordService
    {
        private DiscordSocketClient? _client { get; set; }
        public List<BaseCommand> _commands { get; set; } = new();
        public List<ApplicationCommandProperties> _applicationCommandProperties = new();
        private readonly IMongoService _mongoService;

        public DiscordService(IMongoService mongoService) 
        {
            _mongoService = mongoService;

            var cfg = new DiscordSocketConfig
            {
                UseInteractionSnowflakeDate = false,
            };

            _client = new DiscordSocketClient(cfg);

            _client.Ready += OnReady;
            _client.SlashCommandExecuted += SlashCommandExecuteHandler;

            _client.LoginAsync(TokenType.Bot, Config.DiscordBotToken);
            _client.StartAsync();
        }

        public void Ping() { }

        private async Task SlashCommandExecuteHandler(SocketSlashCommand command)
        {
            var commandClass = _commands.Find(x => x.Name == command.CommandName);

            if (commandClass != null)
            {
                if (commandClass.Type is Enums.DiscordExecuteType.Mongo)
                    await commandClass.Execute(command, _mongoService);
                else
                    await commandClass.Execute(command);
            }
            else
                await command.RespondAsync($"Couldnt find the command {command.CommandName}", null, false, true);
        }


        private async Task RegisterCommand(Type type)
        {
            var command = Activator.CreateInstance(type)! as BaseCommand;

            var commandBuilder = new SlashCommandBuilder();

            commandBuilder.WithName(command!.Name);
            commandBuilder.WithDescription(command!.Description);
            foreach (var aption in command.Options)
            {
                var thing = new SlashCommandOptionBuilder()
                    .WithName("rating")
                    .WithDescription("The rating your willing to give our bot")
                    .WithRequired(true);
                if(aption.Choices != null)
                {
                    foreach (var bOption in aption.Choices)
                        thing.AddChoice(bOption.Name, int.Parse(string.Format("{0}", bOption.Value)));

                }
                
                commandBuilder.AddOption(aption);
            }

            var guild = _client!.GetGuild(1190368305530282034);
            await guild.CreateApplicationCommandAsync(commandBuilder.Build());
            //await _client!.CreateGlobalApplicationCommandAsync(commandBuilder.Build());

            _applicationCommandProperties.Add(commandBuilder.Build());

            //await _client.CreateGlobalApplicationCommandAsync(commandBuilder.Build());
            _commands.Add(command);

            Logger.Log($"Registered Command: {command.Name}", LogLevel.Discord);
        }


        private async Task OnReady()
        {
            await RegisterCommand(typeof(CreateAccountCommand));
            await RegisterCommand(typeof(PingCommand));
            await RegisterCommand(typeof(GrantFullLockerCommand));
            await RegisterCommand(typeof(ClearLockerNotifications));
            await RegisterCommand(typeof(BanUserCommand));
            await RegisterCommand(typeof(CodeCommand));

            var guild = _client!.GetGuild(1190368305530282034);
            await guild!.BulkOverwriteApplicationCommandAsync(_applicationCommandProperties.ToArray());

            Logger.Log($"Discord bot online, Logged in as {_client!.CurrentUser.Username} with {_commands.Count} Commands", LogLevel.Discord);
        }

    }
}
