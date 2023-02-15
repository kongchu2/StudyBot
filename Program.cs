using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

class Program
{
    private readonly DiscordSocketClient _client;
    private readonly AttandanceRepository _repository;

    private string _token;
    private string? _saveFile = Environment.GetEnvironmentVariable("file") ?? "save";
    private ulong _guildId;

    

    static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public Program()
    {
        _token = Environment.GetEnvironmentVariable("token") ?? throw new Exception("Token is null");
        if(!ulong.TryParse(Environment.GetEnvironmentVariable("guild"), out _guildId))
        {
            throw new Exception("guild Id is null");
        }
        


        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.MessageContent
        };
        _client = new DiscordSocketClient(config);
        _repository = new AttandanceRepository(_saveFile);
        


        _client.Log += Log;
        _client.Ready += Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        if(command.CommandName == "출석체크")
        {
            var attList = _repository.GetAttandances(command.User.Id);
            if(attList.Where(x => x.DateTime.IsDateSame(DateTime.Now)).Count() > 0)
            {
                await command.RespondAsync($"{command.User.Username}님 반가워요!\n오늘은 이미 출석체크 하셨어요!\n현재 연속 출석체크 {_repository.GetAttandanceStreak(command.User.Id)}번! \n총 출석체크 {attList.Count()}번 하셨어요!");
            }
            else
            {
                _repository.AddAttandance(new Attandance(DateTime.Now, command.User.Id));
                await command.RespondAsync($"{command.User.Username}님 반가워요!\n현재 연속 출석체크 {_repository.GetAttandanceStreak(command.User.Id)}번! \n총 출석체크 {attList.Count()+1}번 하셨어요!");
            }
        }
    }

    public async Task MainAsync()
    {
        if(string.IsNullOrEmpty(_token))
            throw new Exception("Token is empty.");

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

        var guild = _client.GetGuild(_guildId);

        var attandanceCommandBuilder = new SlashCommandBuilder();
        attandanceCommandBuilder.WithName("출석체크");
        attandanceCommandBuilder.WithDescription("하루에 한 번, 출석체크를 진행할 수 있어요!");

        //var TimerCommandBuilder = new SlashCommandBuilder()
        //.WithName("타이머")
        //.WithDescription("설정한 시간 뒤에 메시지를 보내드릴게요!");


        await guild.CreateApplicationCommandAsync(attandanceCommandBuilder.Build());
    }
}
class Attandance
{
    public readonly DateTime DateTime;
    public readonly ulong UserId;

    public Attandance(DateTime dateTime, ulong userId)
    {
        DateTime = dateTime;
        UserId = userId;
    }
}
class AttandanceRepository
{
    private readonly Dictionary<ulong, List<Attandance>> _dict = new Dictionary<ulong, List<Attandance>>();
    private readonly string _filePath;

    public AttandanceRepository(string serilalizedFilePath)
    {
        if(File.Exists(serilalizedFilePath))
        {
            var pureContent = File.ReadAllText(serilalizedFilePath);
            var list = JsonConvert.DeserializeObject<List<Attandance>>(pureContent);
            if(list != null)
            {
                list.GroupBy(x => x.UserId)
                .ToList()
                .ForEach(x => _dict.Add(x.Key, x.ToList()));
            }
        }

        this._filePath = serilalizedFilePath;
    }

    public List<Attandance> GetAttandances(ulong UserId)
    {
        if(_dict.TryGetValue(UserId, out var att))
        {
            return att;
        }
        else
        {
            return new List<Attandance>();
        }
    }
    public void AddAttandance(Attandance att)
    {
        if(!_dict.ContainsKey(att.UserId))
        {
            _dict[att.UserId] = new List<Attandance>();
        }
        _dict[att.UserId].Add(att);

        Save();
    }

    public int GetAttandanceStreak(ulong UserId)
    {
        var list = GetAttandances(UserId)
        .Reverse<Attandance>();

        if(!list.First().DateTime.IsDateSame(DateTime.Now))
            return 0;

        return DateCalculator.GetDateStreak(list.Select(x => x.DateTime).ToList());
    }

    private void Save()
    {
        var list = new List<Attandance>();
        foreach(var tuple in _dict)
        {
            list.AddRange(tuple.Value);
        }
        File.WriteAllText(_filePath, JsonConvert.SerializeObject(list));
    }
}
class DateCalculator
{
    public static int GetDateStreak(List<DateTime> list)
    {
        if(list.Count() == 0)
        {
            return 0;
        }

        list = list.Select(x=>x.Date).ToList();

        int count = 1;
        DateTime before = list.First();
        foreach(var current in list.Skip(1))
        {
            if(before.Subtract(current) <= TimeSpan.FromDays(1))
                count++;
            else
                break;
            before = current;
        }

        return count;
    }
}
static class DateTimeExtension
{
    public static bool IsDateSame(this DateTime datetime, DateTime compare)
    {
        return 
        datetime.Day == compare.Day &&
        datetime.Month == compare.Month && 
        datetime.Year == compare.Year;
    }
}