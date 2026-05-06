public class House : BuildingBase
{
    private int _appliedPopulationCapacityBonus;

    public House(BuildingData data) : base(data)
    {
    }

    public override void OnTurnStart(GameState state)
    {
        if (state == null || !IsActive)
        {
            return;
        }

        int currentBonus = GetEffectValue();
        state.AddPopulationCapacity(currentBonus - _appliedPopulationCapacityBonus);
        _appliedPopulationCapacityBonus = currentBonus;
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
