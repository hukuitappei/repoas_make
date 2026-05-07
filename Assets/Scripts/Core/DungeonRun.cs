public class DungeonRun
{
    public GuildMember Member { get; private set; }
    public int CurrentFloor { get; private set; }
    public int RemainingTurns { get; private set; }

    public DungeonRun(GuildMember member)
    {
        Member = member;
        CurrentFloor = 1;
        RemainingTurns = GameConstants.DUNGEON_TURNS_PER_FLOOR;
    }

    public void Advance(int speedBonus = 0)
    {
        int advance = 1 + (speedBonus > 0 ? speedBonus : 0);
        RemainingTurns -= advance;
    }

    public bool IsFloorCompleted()
    {
        return RemainingTurns <= 0;
    }

    public bool MoveToNextFloor()
    {
        CurrentFloor++;
        if (CurrentFloor > GameConstants.DUNGEON_FLOOR_COUNT)
        {
            return false;
        }

        RemainingTurns = GameConstants.DUNGEON_TURNS_PER_FLOOR;
        return true;
    }
}
