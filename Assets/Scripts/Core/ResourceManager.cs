public class ResourceManager
{
    public void ResolveTurnResources(GameState state)
    {
        if (state == null)
        {
            return;
        }

        int foodProduction = state.BaseFoodProduction;
        int fundsDelta = 0;

        for (int i = 0; i < state.Buildings.Count; i++)
        {
            BuildingBase building = state.Buildings[i];
            if (building == null)
            {
                continue;
            }

            building.OnTurnStart(state);
            if (building.IsActive)
            {
                fundsDelta -= building.Data != null ? building.Data.GetMaintenanceCostFunds(building.Level) : 0;
            }
        }

        if (state.FoodProductionPercentBonus != 0)
        {
            foodProduction += foodProduction * state.FoodProductionPercentBonus / 100;
        }

        state.AddFood(foodProduction);
        state.AddFood(-(state.Population * GameConstants.FOOD_CONSUMPTION_PER_POP));

        if (state.Food <= 0 && state.Population > 0)
        {
            int populationLoss = state.Population / 100;
            state.AddPopulation(-(populationLoss > 0 ? populationLoss : 1));
        }

        state.AddFunds(fundsDelta);
        state.RecordFundsDeficitState();
    }
}
