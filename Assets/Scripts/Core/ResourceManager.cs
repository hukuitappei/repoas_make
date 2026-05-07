public class ResourceManager
{
    public void ResolveTurnResources(GameState state)
    {
        if (state == null)
        {
            return;
        }

        int foodProduction = state.BaseFoodProduction + state.AssignedFoodWorkers * GameConstants.FOOD_PRODUCTION_PER_ASSIGNED_WORKER;
        int fundsDelta = state.AssignedFundsWorkers * GameConstants.FUNDS_PRODUCTION_PER_ASSIGNED_WORKER;

        for (int i = 0; i < state.Buildings.Count; i++)
        {
            BuildingBase building = state.Buildings[i];
            if (building == null || !building.IsActive)
            {
                continue;
            }

            fundsDelta -= building.Data != null ? building.Data.GetMaintenanceCostFunds(building.Level) : 0;
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
