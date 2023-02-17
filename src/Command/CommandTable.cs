using Discord.WebSocket;

class CommandTable
{
    private readonly AttandanceRepository _repository;

    private readonly Dictionary<string, Func<SocketSlashCommand, Task>> _commandTable;

    public CommandTable(AttandanceRepository repository)
    {
        _commandTable = new Dictionary<string, Func<SocketSlashCommand, Task>>()
        {
            ["출석체크"] = DoAttandance
        };

        _repository = repository;
    }

    public bool TryGetValue(string commandName, out Func<SocketSlashCommand, Task>? commandFunc) 
        => _commandTable.TryGetValue(commandName, out commandFunc);

    private async Task DoAttandance(SocketSlashCommand command)
    {
        var attList = _repository.GetAttandances(command.User.Id);
        if(attList.Where(x => x.DateTime.IsDateSame(DateTime.Now)).Count() > 0)
        {
            await command.RespondAsync(
                $"{command.User.Username}님 반가워요!\n" +
                $"오늘은 이미 출석체크 하셨어요!\n" +
                $"현재 연속 출석체크 {_repository.GetAttandanceStreak(command.User.Id)}번! \n" +
                $"총 출석체크 {attList.Count()}번 하셨어요!"
            );
        }
        else
        {
            _repository.AddAttandance(new Attandance(DateTime.Now, command.User.Id));
            int count = 1;
            if(attList.Count > 0)
                count = attList.Count;
            await command.RespondAsync(
                $"{command.User.Username}님 반가워요!\n" +
                $"현재 연속 출석체크 {_repository.GetAttandanceStreak(command.User.Id)}번! \n" +
                $"총 출석체크 {count}번 하셨어요!"
            );
        }
    }
}