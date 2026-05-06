using System.Collections.Generic;

public abstract class GuildBase
{
    private readonly List<GuildMember> _members;

    public string GuildName { get; protected set; }
    public IReadOnlyList<GuildMember> Members => _members;
    public int MaxMembers { get; protected set; }
    public GuildData Data { get; protected set; }
    public bool IsUnlocked { get; private set; }

    protected GuildBase(GuildData data, bool isUnlocked)
    {
        Data = data;
        GuildName = data != null ? data.guildName : string.Empty;
        MaxMembers = data != null ? data.maxMembers : 0;
        IsUnlocked = isUnlocked;
        _members = new List<GuildMember>();
    }

    public abstract void AssignAction(GuildMember member, GuildAction action);
    public abstract void ResolveActions(GameState state);
    public abstract int CalculateCombatPower();

    public void Unlock()
    {
        IsUnlocked = true;
    }

    public void AddMaxMembers(int amount)
    {
        if (amount > 0)
        {
            MaxMembers += amount;
        }
    }

    public bool CanAddMember()
    {
        return IsUnlocked && _members.Count < MaxMembers;
    }

    public bool AddMember(GuildMember member)
    {
        if (member == null || !CanAddMember())
        {
            return false;
        }

        _members.Add(member);
        return true;
    }

    protected bool ContainsMember(GuildMember member)
    {
        return member != null && _members.Contains(member);
    }
}
