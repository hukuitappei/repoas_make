public class Sawmill : BuildingBase
{
    public Sawmill(BuildingData data) : base(data)
    {
    }

    public override void OnTurnStart(GameState state)
    {
        if (state == null || !IsActive)
        {
            return;
        }

        int amount = GetEffectValue();
        if (state.MaterialProductionPercentBonus != 0)
        {
            amount += amount * state.MaterialProductionPercentBonus / 100;
        }

        state.AddMaterial(MaterialType.Wood, MaterialGrade.F, amount);
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
