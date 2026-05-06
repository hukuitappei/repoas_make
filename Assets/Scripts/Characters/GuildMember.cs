public class GuildMember
{
    public string Name { get; private set; }
    public GuildType GuildType { get; private set; }
    public int Level { get; private set; }
    public int Experience { get; private set; }
    public int CombatPower { get; private set; }
    public int SkillPower { get; private set; }
    public int CombatPowerGrowth { get; private set; }
    public int SkillPowerGrowth { get; private set; }
    public GuildAction CurrentAction { get; private set; }
    public bool IsAvailable { get; private set; }
    public int TemporaryCombatPenaltyPercent { get; private set; }
    public int TemporaryCombatPenaltyRemainingTurns { get; private set; }
    public bool IsInDungeonRun { get; private set; }

    public int RequiredExperience => Level * 100;
    public int CurrentCombatPower => ApplyPenalty(CombatPower, TemporaryCombatPenaltyPercent);
    public int CurrentSkillPower => ApplyPenalty(SkillPower, TemporaryCombatPenaltyPercent);

    public GuildMember(string name, GuildData guildData, int lordPopularity)
    {
        Name = string.IsNullOrEmpty(name) ? "Guild Member" : name;
        GuildType = guildData != null ? guildData.guildType : GuildType.Warrior;
        Level = 1;
        Experience = 0;
        int popularityBonus = ClampMin(lordPopularity, 0) / 10;
        CombatPower = (guildData != null ? guildData.baseCombatPower : 0) + popularityBonus;
        SkillPower = (guildData != null ? guildData.baseSkillPower : 0) + popularityBonus;
        CombatPowerGrowth = guildData != null ? guildData.combatPowerGrowth : 0;
        SkillPowerGrowth = guildData != null ? guildData.skillPowerGrowth : 0;
        CurrentAction = GuildAction.Idle;
        IsAvailable = true;
    }

    public void AssignAction(GuildAction action)
    {
        CurrentAction = action;
        IsAvailable = action == GuildAction.Idle;
    }

    public void ClearAction()
    {
        CurrentAction = GuildAction.Idle;
        IsAvailable = true;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Experience += amount;
        while (Level < 10 && Experience >= RequiredExperience)
        {
            Experience -= RequiredExperience;
            Level++;
            CombatPower += CombatPowerGrowth;
            SkillPower += SkillPowerGrowth;
        }
    }

    public void SetInDungeonRun(bool value)
    {
        IsInDungeonRun = value;
    }

    public void ApplyTemporaryCombatPenaltyPercent(int penaltyPercent, int turns)
    {
        if (penaltyPercent <= 0 || turns <= 0)
        {
            return;
        }

        if (penaltyPercent > TemporaryCombatPenaltyPercent || TemporaryCombatPenaltyRemainingTurns <= 0)
        {
            TemporaryCombatPenaltyPercent = penaltyPercent;
        }

        if (turns > TemporaryCombatPenaltyRemainingTurns)
        {
            TemporaryCombatPenaltyRemainingTurns = turns;
        }
    }

    public void AdvanceTurnStatus()
    {
        if (TemporaryCombatPenaltyRemainingTurns <= 0)
        {
            return;
        }

        TemporaryCombatPenaltyRemainingTurns--;
        if (TemporaryCombatPenaltyRemainingTurns <= 0)
        {
            TemporaryCombatPenaltyPercent = 0;
        }
    }

    private static int ApplyPenalty(int baseValue, int penaltyPercent)
    {
        if (penaltyPercent <= 0)
        {
            return baseValue;
        }

        return baseValue - baseValue * penaltyPercent / 100;
    }

    private static int ClampMin(int value, int min)
    {
        return value < min ? min : value;
    }
}
