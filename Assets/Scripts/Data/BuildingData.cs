using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "repoas/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public BuildingEffectType effectType;
    public int maxLevel;
    public int[] buildCostFunds;
    public int[] buildCostMaterials;
    public MaterialRequirement[] level1MaterialRequirements;
    public MaterialRequirement[] level2MaterialRequirements;
    public MaterialRequirement[] level3MaterialRequirements;
    public int[] maintenanceCostFunds;
    public int[] effectValues;

    public int GetEffectValue(int level)
    {
        return GetLevelValue(effectValues, level);
    }

    public int GetMaintenanceCostFunds(int level)
    {
        return GetLevelValue(maintenanceCostFunds, level);
    }

    public int GetBuildCostFunds(int level)
    {
        return GetLevelValue(buildCostFunds, level);
    }

    public MaterialRequirement[] GetBuildMaterialRequirements(int level)
    {
        if (level == 1) return level1MaterialRequirements;
        if (level == 2) return level2MaterialRequirements;
        if (level == 3) return level3MaterialRequirements;
        return null;
    }

    private static int GetLevelValue(int[] values, int level)
    {
        if (values == null || values.Length == 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(level - 1, 0, values.Length - 1);
        return values[index];
    }
}
