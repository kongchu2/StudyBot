public static class DateTimeExtension
{
    public static bool IsDateSame(this DateTime datetime, DateTime compare)
    {
        return 
        datetime.Day == compare.Day &&
        datetime.Month == compare.Month && 
        datetime.Year == compare.Year;
    }
}