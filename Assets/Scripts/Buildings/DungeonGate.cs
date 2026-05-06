public class DungeonGate : BuildingBase
{
    private int _appliedDungeonRewardPercentBonus;

    public DungeonGate(BuildingData data) : base(data)
    {
    }

    public override void OnTurnStart(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (IsActive)
        {
            state.UnlockDungeonExploration();
        }

        int currentBonus = IsActive ? GetRewardBonus() : 0;
        state.AddDungeonRewardPercentBonus(currentBonus - _appliedDungeonRewardPercentBonus);
        _appliedDungeonRewardPercentBonus = currentBonus;
    }

    public override void OnTurnEnd(GameState state)
    {
    }

    public override bool CanUpgrade(GameState state)
    {
        if (state == null || Data == null || Level >= MaxLevel)
        {
            return false;
        }

        int nextLevel = Level + 1;
        if (state.Funds < Data.GetBuildCostFunds(nextLevel))
        {
            return false;
        }

        MaterialRequirement[] requirements = Data.GetBuildMaterialRequirements(nextLevel);
        return requirements == null || state.HasMaterials(requirements);
    }

    private int GetRewardBonus()
    {
        return Level <= 1 ? 0 : GetEffectValue();
    }

}
