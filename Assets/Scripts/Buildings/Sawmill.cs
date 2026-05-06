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

        MaterialRequirement[] requirements = GetBuildMaterialRequirements(nextLevel);
        return requirements == null || state.HasMaterials(requirements);
    }

    private MaterialRequirement[] GetBuildMaterialRequirements(int level)
    {
        if (Data.buildMaterialRequirements == null || Data.buildMaterialRequirements.Length == 0)
        {
            return null;
        }

        int index = level - 1;
        if (index < 0 || index >= Data.buildMaterialRequirements.Length)
        {
            return null;
        }

        MaterialRequirement[] requirements = Data.buildMaterialRequirements[index];
        return requirements != null && requirements.Length > 0 ? requirements : null;
    }
}
