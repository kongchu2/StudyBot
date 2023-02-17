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