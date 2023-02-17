static class Validator
{
    public static bool ValidateToken(string token, out string? message)
    {
        message = null;
        List<(Func<string, bool> func, string failedMessage)> list = new() {
            (x => !string.IsNullOrEmpty(x), "token is empty")
        };
        foreach(var tuple in list)
        {
            if(!tuple.func(token)) {
                message = tuple.failedMessage;
                return false;
            }
        }
        return true;
    }
}