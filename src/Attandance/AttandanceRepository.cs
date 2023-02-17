using Newtonsoft.Json;

class AttandanceRepository
{
    private readonly Dictionary<ulong, List<Attandance>> _dict;
    private readonly string _filePath;

    public AttandanceRepository(string serilalizedFilePath)
    {
        if(File.Exists(serilalizedFilePath))
        {
            var pureContent = File.ReadAllText(serilalizedFilePath);
            var list = JsonConvert.DeserializeObject<List<Attandance>>(pureContent);
            if(list != null)
            {
                _dict = list
                .GroupBy(x => x.UserId)
                .ToDictionary(group => group.Key, group => group.ToList());
            }
        }
        _dict = _dict ?? new Dictionary<ulong, List<Attandance>>();

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
        var list = new List<Attandance>(_dict.Count());
        foreach(var att in _dict)
            list.AddRange(att.Value);
        File.WriteAllText(
            _filePath, 
            JsonConvert.SerializeObject(list)
        );
    }
}