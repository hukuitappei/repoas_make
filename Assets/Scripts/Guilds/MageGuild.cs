public class MageGuild : GuildBase
{
    private static readonly System.Random Random = new System.Random();
    private const int SPECIALIZED_SUCCESS_EXPERIENCE = 30;
    private const int SPECIALIZED_FAILURE_EXPERIENCE = 15;
    private const int NON_SPECIALIZED_SUCCESS_EXPERIENCE = 15;
    private const int NON_SPECIALIZED_FAILURE_EXPERIENCE = 5;

    public MageGuild(GuildData data, bool isUnlocked = false) : base(data, isUnlocked)
    {
    }

    public override void AssignAction(GuildMember member, GuildAction action)
    {
        if (!CanAssign(member, action))
        {
            return;
        }

        member.AssignAction(action);
    }

    public override void ResolveActions(GameState state)
    {
        for (int i = 0; i < Members.Count; i++)
        {
            GuildMember member = Members[i];
            if (member == null)
            {
                continue;
            }

            member.AdvanceTurnStatus();
            if (member.CurrentAction == GuildAction.Idle || member.IsInDungeonRun)
            {
                continue;
            }

            GuildAction action = member.CurrentAction;
            bool isSuccess = ApplyActionResult(state, member, action);
            member.AddExperience(GetExperience(action, isSuccess, state != null ? state.GuildMoraleBonus : 0));
        }
    }

    public override int CalculateCombatPower()
    {
        int combatPower = 0;
        for (int i = 0; i < Members.Count; i++)
        {
            GuildMember member = Members[i];
            if (member != null && CanContributeCombatPower(member))
            {
                combatPower += member.CurrentCombatPower;
            }
        }

        return combatPower;
    }

    private bool CanAssign(GuildMember member, GuildAction action)
    {
        if (!IsUnlocked || !ContainsMember(member))
        {
            return false;
        }

        return action == GuildAction.Idle || member.IsAvailable;
    }

    private bool ApplyActionResult(GameState state, GuildMember member, GuildAction action)
    {
        if (state == null)
        {
            return false;
        }

        if (action == GuildAction.Research)
        {
            state.AddTurnResearchSpeedBonusPercent(member.CurrentSkillPower / 2);
            return true;
        }

        if (action == GuildAction.Explore && member.CurrentActionTargetId == GameConstants.EXPLORATION_TARGET_RAID_ORIGIN)
        {
            bool isSuccess = Random.NextDouble() < GameConstants.EXPLORATION_SUCCESS_RATE;
            state.AddInitialRaidOriginExplorationProgress(isSuccess ? GameConstants.EXPLORATION_SUCCESS_PROGRESS : GameConstants.EXPLORATION_FAILURE_PROGRESS);
            return isSuccess;
        }

        return true;
    }

    private int GetExperience(GuildAction action, bool isSuccess, int moraleBonus)
    {
        if (IsSpecializedAction(action))
        {
            return (isSuccess ? SPECIALIZED_SUCCESS_EXPERIENCE : SPECIALIZED_FAILURE_EXPERIENCE) + moraleBonus;
        }

        return (isSuccess ? NON_SPECIALIZED_SUCCESS_EXPERIENCE : NON_SPECIALIZED_FAILURE_EXPERIENCE) + moraleBonus;
    }

    private bool IsSpecializedAction(GuildAction action)
    {
        if (Data != null)
        {
            return Data.IsSpecializedAction(action);
        }

        return action == GuildAction.Research || action == GuildAction.Explore;
    }

    private static bool CanContributeCombatPower(GuildMember member)
    {
        return member.CurrentAction == GuildAction.Idle
            || member.CurrentAction == GuildAction.Defend
            || member.CurrentAction == GuildAction.Explore;
    }
}
