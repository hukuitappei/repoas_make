using UnityEngine;

[CreateAssetMenu(fileName = "MetaSkillData", menuName = "repoas/MetaSkillData")]
public class MetaSkillData : ScriptableObject
{
    public string skillId;
    public string displayName;
    public string description;
    public MetaSkillEffectType effectType;
    public MaterialType materialType;
    public int costPerLevel;
    public int maxLevel;
    public int effectValuePerLevel;
}
