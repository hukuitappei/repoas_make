public class MetaProgressionSystem
{
    public int CalculateEarnedMetaPoints(GameState state)
    {
        if (state == null)
        {
            return 5;
        }

        int points = 0;
        if (state.HasFirstRaidOccurred)
        {
            points += 5;
        }

        if (state.HasFirstRaidCleared)
        {
            points += 5;
        }

        points += state.SubsequentPerfectRaidWinCount * 3;
        points += (state.SubsequentRaidWinCount - state.SubsequentPerfectRaidWinCount) * 2;
        points += state.CompletedNode0ResearchGroupIds.Count * 1;
        points += state.CompletedInitialResearchGroupIds.Count * 2;
        points += state.CompletedUpperResearchGroupIds.Count * 4;
        points += CalculateSpecialItemPoints(state);

        return points < 5 ? 5 : points;
    }

    public int GetLordStatIncreaseCost(int currentStat)
    {
        int clamped = currentStat < 0 ? 0 : currentStat;
        clamped = clamped > 100 ? 100 : clamped;
        return clamped / 5 + 1;
    }

    private int CalculateSpecialItemPoints(GameState state)
    {
        return state.GetSpecialItemCount(MaterialGrade.F)
            + state.GetSpecialItemCount(MaterialGrade.E) * 2
            + state.GetSpecialItemCount(MaterialGrade.D) * 3
            + state.GetSpecialItemCount(MaterialGrade.C) * 4
            + state.GetSpecialItemCount(MaterialGrade.B) * 5
            + state.GetSpecialItemCount(MaterialGrade.A) * 7
            + state.GetSpecialItemCount(MaterialGrade.S) * 10;
    }
}
