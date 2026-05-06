public class RaidResult
{
    public RaidOutcome Outcome { get; private set; }
    public int EnemyPower { get; private set; }
    public int DefensePower { get; private set; }

    public RaidResult(RaidOutcome outcome, int enemyPower, int defensePower)
    {
        Outcome = outcome;
        EnemyPower = enemyPower;
        DefensePower = defensePower;
    }
}
