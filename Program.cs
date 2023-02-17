using Discord;
using Discord.WebSocket;

class Program
{
    private readonly DiscordSocketClient _client;
    private readonly CommandTable _commandTable;

    private readonly string _token;
    private readonly ulong _guildId;

    private const string DataSavePath = "save";

    static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public Program()
    {
        _token = Environment.GetEnvironmentVariable("token") ?? throw new Exception("Token is null, Set the environment variable. variable name:token");
        _guildId = ulong.Parse(Environment.GetEnvironmentVariable("guild") ?? throw new Exception("Guild id is null, Set the environment variable. variable name:guild"));

        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.MessageContent
        };
        _client = new DiscordSocketClient(config);
        _commandTable = new CommandTable(new AttandanceRepository(DataSavePath));
        

        if(!Validator.ValidateToken(_token, out var err))
        {
            throw new Exception($"Failed to Validate: {err}");
        }
        
        _client.Log += Log;
        _client.Ready += Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        if(_commandTable.TryGetValue(command.CommandName, out var func))
        {
            await func!.Invoke(command);
        }
        else
        {
            throw new Exception($"not defined command. :{command.CommandName}");
        }
    }

    public async Task MainAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }   

    private async Task Ready()
    {
        Console.WriteLine($"{_client.CurrentUser} 연결됨!");

        var commands = await _client.GetGlobalApplicationCommandsAsync();
        commands.ToList().ForEach(async x => await x.DeleteAsync());

        var guild = _client.GetGuild(_guildId);
        
        CommandCreator.CreateGuildCommands()
        .ForEach(async command => await guild.CreateApplicationCommandAsync(command));
    }    
}