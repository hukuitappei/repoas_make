public class Plaza : BuildingBase
{
    private int _appliedHappinessBonus;

    public Plaza(BuildingData data) : base(data)
    {
    }

    public override void OnTurnStart(GameState state)
    {
        if (state == null)
        {
            return;
        }

        int currentBonus = IsActive ? GetEffectValue() : 0;
        state.AddHappinessBonus(currentBonus - _appliedHappinessBonus);
        _appliedHappinessBonus = currentBonus;
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
}
