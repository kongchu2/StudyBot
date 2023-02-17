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