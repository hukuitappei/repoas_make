using UnityEngine;

[CreateAssetMenu(fileName = "ResearchNodeData", menuName = "repoas/ResearchNodeData")]
public class ResearchNodeData : ScriptableObject
{
    public string nodeId;
    public string displayName;
    public string description;
    public string[] prerequisiteNodeIds;
    public string importantResearchGroupId;
    public ResearchImportantGroupTier importantGroupTier;
    public int researchCostFunds;
    public int researchDurationTurns;
    public int requiredWorkers;
    public int requiredFood;
    public MaterialRequirement[] materialRequirements;
    public ResearchEffect[] effects;
}
