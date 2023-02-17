using Discord;

class CommandCreator
{
    static public List<SlashCommandProperties> CreateGuildCommands()
    {
        return new List<SlashCommandProperties>()
        {
            new SlashCommandBuilder()
            .WithName("출석체크")
            .WithDescription("하루에 한 번, 출석체크를 진행할 수 있어요!")
            .Build()
        };
    }
}