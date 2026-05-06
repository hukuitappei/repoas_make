using UnityEngine;

[CreateAssetMenu(fileName = "GuildData", menuName = "repoas/GuildData")]
public class GuildData : ScriptableObject
{
    public string guildName;
    public GuildType guildType;
    public string unlockResearchNodeId;
    public int maxMembers;
    public int hireCostFunds;
    public int baseCombatPower;
    public int baseSkillPower;
    public int combatPowerGrowth;
    public int skillPowerGrowth;
    public GuildAction[] specializedActions;

    public bool IsSpecializedAction(GuildAction action)
    {
        if (specializedActions == null)
        {
            return false;
        }

        for (int i = 0; i < specializedActions.Length; i++)
        {
            if (specializedActions[i] == action)
            {
                return true;
            }
        }

        return false;
    }
}
