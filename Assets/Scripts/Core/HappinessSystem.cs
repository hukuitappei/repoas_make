public class HappinessSystem
{
    public int Recalculate(GameState state)
    {
        if (state == null)
        {
            return GameConstants.BASE_HAPPINESS;
        }

        int happiness = GameConstants.BASE_HAPPINESS;
        happiness += CalculateFoodBonus(state);
        happiness += CalculateEntertainmentBonus(state);
        happiness += state.HappinessBonus;
        happiness -= CalculateOvercrowdingPenalty(state);

        if (state.Funds < 0)
        {
            happiness += GameConstants.FUNDS_NEGATIVE_PENALTY;
        }

        state.SetHappiness(happiness);
        state.RecordHappinessCrisisState();
        return state.Happiness;
    }

    private int CalculateFoodBonus(GameState state)
    {
        if (state.Population <= 0)
        {
            return 0;
        }

        float ratio = (float)state.Food / state.Population;
        if (ratio <= GameConstants.FOOD_RATIO_BAD_THRESHOLD)
        {
            return GameConstants.FOOD_SHORTAGE_MAX_PENALTY;
        }

        if (ratio >= GameConstants.FOOD_RATIO_GOOD_THRESHOLD)
        {
            return GameConstants.FOOD_SUFFICIENCY_MAX_BONUS;
        }

        float range = GameConstants.FOOD_RATIO_GOOD_THRESHOLD - GameConstants.FOOD_RATIO_BAD_THRESHOLD;
        float normalized = (ratio - GameConstants.FOOD_RATIO_BAD_THRESHOLD) / range;
        int totalRange = GameConstants.FOOD_SUFFICIENCY_MAX_BONUS - GameConstants.FOOD_SHORTAGE_MAX_PENALTY;
        return GameConstants.FOOD_SHORTAGE_MAX_PENALTY + (int)(totalRange * normalized);
    }

    private int CalculateEntertainmentBonus(GameState state)
    {
        int totalLevel = 0;
        for (int i = 0; i < state.Buildings.Count; i++)
        {
            BuildingBase building = state.Buildings[i];
            if (building == null || building.Data == null)
            {
                continue;
            }

            if (building.Data.effectType == BuildingEffectType.HappinessBonus || building.Data.effectType == BuildingEffectType.MoraleBonus)
            {
                totalLevel += building.Level;
            }
        }

        int bonus = totalLevel * GameConstants.ENTERTAINMENT_BONUS_PER_LEVEL;
        return bonus > 25 ? 25 : bonus;
    }

    private int CalculateOvercrowdingPenalty(GameState state)
    {
        if (state.PopulationCapacity <= 0)
        {
            return GameConstants.OVERCROWDING_MAX_PENALTY;
        }

        float ratio = (float)state.Population / state.PopulationCapacity;
        if (ratio <= GameConstants.OVERCROWDING_START_RATIO)
        {
            return 0;
        }

        if (ratio >= 1.2f)
        {
            return GameConstants.OVERCROWDING_MAX_PENALTY;
        }

        float normalized = (ratio - GameConstants.OVERCROWDING_START_RATIO) / (1.2f - GameConstants.OVERCROWDING_START_RATIO);
        return (int)(GameConstants.OVERCROWDING_MAX_PENALTY * normalized);
    }
}
