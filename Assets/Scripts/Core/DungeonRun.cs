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

    public void Advance()
    {
        RemainingTurns--;
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
