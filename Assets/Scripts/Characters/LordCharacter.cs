public class LordCharacter
{
    public int Popularity { get; private set; }
    public int Negotiation { get; private set; }
    public int Luck { get; private set; }
    public int Support { get; private set; }
    public int Thinking { get; private set; }

    public LordCharacter()
    {
        Popularity = 0;
        Negotiation = 0;
        Luck = 0;
        Support = 0;
        Thinking = 0;
    }

    public int GetCostToIncrease(int currentStat)
    {
        return Clamp(currentStat, 0, 100) / 5 + 1;
    }

    public bool TryIncreasePopularity(int availableMetaPoints, out int spentMetaPoints)
    {
        int newValue;
        bool result = TryIncreaseStat(Popularity, availableMetaPoints, out newValue, out spentMetaPoints);
        Popularity = newValue;
        return result;
    }

    public bool TryIncreaseNegotiation(int availableMetaPoints, out int spentMetaPoints)
    {
        int newValue;
        bool result = TryIncreaseStat(Negotiation, availableMetaPoints, out newValue, out spentMetaPoints);
        Negotiation = newValue;
        return result;
    }

    public bool TryIncreaseLuck(int availableMetaPoints, out int spentMetaPoints)
    {
        int newValue;
        bool result = TryIncreaseStat(Luck, availableMetaPoints, out newValue, out spentMetaPoints);
        Luck = newValue;
        return result;
    }

    public bool TryIncreaseSupport(int availableMetaPoints, out int spentMetaPoints)
    {
        int newValue;
        bool result = TryIncreaseStat(Support, availableMetaPoints, out newValue, out spentMetaPoints);
        Support = newValue;
        return result;
    }

    public bool TryIncreaseThinking(int availableMetaPoints, out int spentMetaPoints)
    {
        int newValue;
        bool result = TryIncreaseStat(Thinking, availableMetaPoints, out newValue, out spentMetaPoints);
        Thinking = newValue;
        return result;
    }

    private bool TryIncreaseStat(int currentValue, int availableMetaPoints, out int newValue, out int spentMetaPoints)
    {
        spentMetaPoints = GetCostToIncrease(currentValue);
        if (currentValue >= 100 || availableMetaPoints < spentMetaPoints)
        {
            newValue = currentValue;
            spentMetaPoints = 0;
            return false;
        }

        newValue = Clamp(currentValue + 1, 0, 100);
        return true;
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
