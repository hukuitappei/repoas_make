public class Market : BuildingBase
{
    public Market(BuildingData data) : base(data)
    {
    }

    public override void OnTurnStart(GameState state)
    {
        if (state == null || !IsActive)
        {
            return;
        }

        state.AddFunds(GetEffectValue());
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
