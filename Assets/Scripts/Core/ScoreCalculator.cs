public class ScoreCalculator
{
    public int Calculate(GameState state)
    {
        if (state == null)
        {
            return 0;
        }

        return state.Population
            + state.Funds / 5
            + state.Buildings.Count * 50
            + state.CompletedResearchNodeIds.Count * 80
            + state.CompletedImportantResearchGroupIds.Count * 150
            + state.ClearedDungeonFloorCount * 120
            + state.RaidWinCount * 100
            + state.PerfectRaidWinCount * 150
            - state.RaidLossCount * 150;
    }
}
