public abstract class BuildingBase
{
    public string Name { get; protected set; }
    public int Level { get; protected set; }
    public int MaxLevel { get; protected set; }
    public BuildingData Data { get; protected set; }
    public bool IsActive { get; private set; }

    protected BuildingBase(BuildingData data)
    {
        Data = data;
        Name = data != null ? data.buildingName : string.Empty;
        Level = 1;
        MaxLevel = data != null ? data.maxLevel : 1;
        IsActive = true;
    }

    public abstract void OnTurnStart(GameState state);
    public abstract void OnTurnEnd(GameState state);
    public abstract bool CanUpgrade(GameState state);

    public virtual void Upgrade()
    {
        if (Level < MaxLevel)
        {
            Level++;
        }
    }

    public void IncreaseMaxLevel(int amount)
    {
        if (amount > 0)
        {
            MaxLevel += amount;
        }
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    protected int GetEffectValue()
    {
        return Data != null ? Data.GetEffectValue(Level) : 0;
    }

    protected int GetMaintenanceCostFunds()
    {
        return Data != null ? Data.GetMaintenanceCostFunds(Level) : 0;
    }
}
