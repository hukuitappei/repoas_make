using System.Collections.Generic;

public class GameState
{
    private readonly Dictionary<MaterialType, Dictionary<MaterialGrade, int>> _materials;
    private readonly List<BuildingBase> _buildings;
    private readonly List<GuildBase> _guilds;
    private readonly HashSet<string> _completedResearchNodeIds;
    private readonly HashSet<string> _completedImportantResearchGroupIds;
    private readonly HashSet<string> _completedNode0ResearchGroupIds;
    private readonly HashSet<string> _completedInitialResearchGroupIds;
    private readonly HashSet<string> _completedUpperResearchGroupIds;
    private readonly Dictionary<MaterialGrade, int> _specialItems;

    public int CurrentTurn { get; private set; }
    public int Food { get; private set; }
    public int Funds { get; private set; }
    public int Population { get; private set; }
    public int AssignedFoodWorkers { get; private set; }
    public int AssignedFundsWorkers { get; private set; }
    public int AssignedDevelopmentWorkers { get; private set; }
    public int Happiness { get; private set; }
    public int PopulationCapacity { get; private set; }
    public int BaseFoodProduction { get; private set; }
    public int FoodProductionPercentBonus { get; private set; }
    public int MaterialProductionPercentBonus { get; private set; }
    public int DefensePowerBonus { get; private set; }
    public int ResearchSpeedPercentBonus { get; private set; }
    public int TurnResearchSpeedBonusPercent { get; private set; }
    public int HireCostReductionPercent { get; private set; }
    public int DungeonRewardPercentBonus { get; private set; }
    public int HappinessBonus { get; private set; }
    public int GuildMoraleBonus { get; private set; }
    public bool IsCombatSupportUnlocked { get; private set; }
    public int HouseCapacityBonusPercent { get; private set; }
    public int DungeonFloorSpeedBonus { get; private set; }
    public int SpentMetaPoints { get; private set; }
    public int FundsDeficitTurnCount { get; private set; }
    public int HappinessCrisisTurnCount { get; private set; }
    public int RaidWinCount { get; private set; }
    public int PerfectRaidWinCount { get; private set; }
    public int RaidLossCount { get; private set; }
    public int SubsequentRaidWinCount { get; private set; }
    public int SubsequentPerfectRaidWinCount { get; private set; }
    public int LastRaidTurn { get; private set; }
    public int ClearedDungeonFloorCount { get; private set; }
    public bool IsInitialRaidOriginExplored { get; private set; }
    public int InitialRaidOriginExplorationProgress { get; private set; }
    public bool HasFirstRaidOccurred { get; private set; }
    public bool HasFirstRaidCleared { get; private set; }
    public bool IsDungeonExplorationUnlocked { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsVictory { get; private set; }
    public string GameEndReason { get; private set; }
    public LordCharacter Lord { get; private set; }

    public IReadOnlyList<BuildingBase> Buildings => _buildings;
    public IReadOnlyList<GuildBase> Guilds => _guilds;
    public IReadOnlyCollection<string> CompletedResearchNodeIds => _completedResearchNodeIds;
    public IReadOnlyCollection<string> CompletedImportantResearchGroupIds => _completedImportantResearchGroupIds;
    public IReadOnlyCollection<string> CompletedNode0ResearchGroupIds => _completedNode0ResearchGroupIds;
    public IReadOnlyCollection<string> CompletedInitialResearchGroupIds => _completedInitialResearchGroupIds;
    public IReadOnlyCollection<string> CompletedUpperResearchGroupIds => _completedUpperResearchGroupIds;
    public int FreePopulation => ClampMin(Population - AssignedFoodWorkers - AssignedFundsWorkers - AssignedDevelopmentWorkers, 0);

    public GameState()
    {
        _materials = new Dictionary<MaterialType, Dictionary<MaterialGrade, int>>();
        _buildings = new List<BuildingBase>();
        _guilds = new List<GuildBase>();
        _completedResearchNodeIds = new HashSet<string>();
        _completedImportantResearchGroupIds = new HashSet<string>();
        _completedNode0ResearchGroupIds = new HashSet<string>();
        _completedInitialResearchGroupIds = new HashSet<string>();
        _completedUpperResearchGroupIds = new HashSet<string>();
        _specialItems = new Dictionary<MaterialGrade, int>();
        Lord = new LordCharacter();

        CurrentTurn = 1;
        Food = GameConstants.STARTING_FOOD;
        Funds = GameConstants.STARTING_FUNDS;
        Population = GameConstants.STARTING_POPULATION;
        AssignedFoodWorkers = GameConstants.STARTING_ASSIGNED_FOOD_WORKERS;
        AssignedFundsWorkers = GameConstants.STARTING_ASSIGNED_FUNDS_WORKERS;
        AssignedDevelopmentWorkers = GameConstants.STARTING_ASSIGNED_DEVELOPMENT_WORKERS;
        Happiness = GameConstants.BASE_HAPPINESS;
        PopulationCapacity = GameConstants.STARTING_POPULATION;

        AddMaterial(MaterialType.Stone, MaterialGrade.F, GameConstants.STARTING_F_STONE);
        AddMaterial(MaterialType.Wood, MaterialGrade.F, GameConstants.STARTING_F_WOOD);
        AddMaterial(MaterialType.Metal, MaterialGrade.F, GameConstants.STARTING_F_METAL);
        AddMaterial(MaterialType.Foodstuff, MaterialGrade.F, GameConstants.STARTING_F_FOODSTUFF);
        AddMaterial(MaterialType.Magic, MaterialGrade.F, GameConstants.STARTING_F_MAGIC);
    }

    public void AdvanceTurn()
    {
        CurrentTurn++;
    }

    public void AddFood(int amount)
    {
        Food = ClampMin(Food + amount, 0);
    }

    public bool TrySpendFood(int amount)
    {
        if (amount < 0 || Food < amount)
        {
            return false;
        }

        Food -= amount;
        return true;
    }

    public void AddFunds(int amount)
    {
        Funds += amount;
    }

    public bool TrySpendFunds(int amount)
    {
        if (amount < 0 || Funds < amount)
        {
            return false;
        }

        Funds -= amount;
        return true;
    }

    public void AddPopulation(int amount)
    {
        Population = ClampMin(Population + amount, 0);
        NormalizeAssignedWorkers();
    }

    public bool TrySetAssignedFoodWorkers(int value)
    {
        return TrySetAssignedWorkers(value, AssignedFundsWorkers, AssignedDevelopmentWorkers);
    }

    public bool TrySetAssignedFundsWorkers(int value)
    {
        return TrySetAssignedWorkers(AssignedFoodWorkers, value, AssignedDevelopmentWorkers);
    }

    public bool TrySetAssignedDevelopmentWorkers(int value)
    {
        return TrySetAssignedWorkers(AssignedFoodWorkers, AssignedFundsWorkers, value);
    }

    public void SetHappiness(int value)
    {
        Happiness = Clamp(value, 0, 100);
    }

    public void AddPopulationCapacity(int amount)
    {
        PopulationCapacity = ClampMin(PopulationCapacity + amount, 0);
    }

    public void AddBaseFoodProduction(int amount)
    {
        BaseFoodProduction += amount;
    }

    public void AddFoodProductionPercentBonus(int amount)
    {
        FoodProductionPercentBonus += amount;
    }

    public void AddMaterialProductionPercentBonus(int amount)
    {
        MaterialProductionPercentBonus += amount;
    }

    public void AddDefensePowerBonus(int amount)
    {
        DefensePowerBonus += amount;
    }

    public void AddResearchSpeedPercentBonus(int amount)
    {
        ResearchSpeedPercentBonus += amount;
    }

    public void AddTurnResearchSpeedBonusPercent(int amount)
    {
        TurnResearchSpeedBonusPercent += amount;
    }

    public void ResetTurnResearchSpeedBonus()
    {
        TurnResearchSpeedBonusPercent = 0;
    }

    public void AddHireCostReductionPercent(int amount)
    {
        HireCostReductionPercent = Clamp(HireCostReductionPercent + amount, 0, 100);
    }

    public void AddDungeonRewardPercentBonus(int amount)
    {
        DungeonRewardPercentBonus += amount;
    }

    public void AddHappinessBonus(int amount)
    {
        HappinessBonus += amount;
    }

    public void AddGuildMoraleBonus(int amount)
    {
        GuildMoraleBonus += amount;
    }

    public void UnlockCombatSupport()
    {
        IsCombatSupportUnlocked = true;
    }

    public void AddHouseCapacityBonusPercent(int amount)
    {
        HouseCapacityBonusPercent += amount;
    }

    public void AddDungeonFloorSpeedBonus(int amount)
    {
        if (amount > 0)
        {
            DungeonFloorSpeedBonus += amount;
        }
    }

    public void UnlockDungeonExploration()
    {
        IsDungeonExplorationUnlocked = true;
    }

    public void AddBuilding(BuildingBase building)
    {
        if (building != null)
        {
            _buildings.Add(building);
        }
    }

    public void AddGuild(GuildBase guild)
    {
        if (guild != null)
        {
            _guilds.Add(guild);
        }
    }

    public void CompleteResearchNode(string nodeId)
    {
        if (!string.IsNullOrEmpty(nodeId))
        {
            _completedResearchNodeIds.Add(nodeId);
        }
    }

    public bool IsResearchNodeCompleted(string nodeId)
    {
        return !string.IsNullOrEmpty(nodeId) && _completedResearchNodeIds.Contains(nodeId);
    }

    public void CompleteImportantResearchGroup(string groupId, ResearchImportantGroupTier tier)
    {
        if (string.IsNullOrEmpty(groupId))
        {
            return;
        }

        _completedImportantResearchGroupIds.Add(groupId);
        if (tier == ResearchImportantGroupTier.Node0)
        {
            _completedNode0ResearchGroupIds.Add(groupId);
        }
        else if (tier == ResearchImportantGroupTier.Initial)
        {
            _completedInitialResearchGroupIds.Add(groupId);
        }
        else if (tier == ResearchImportantGroupTier.Upper)
        {
            _completedUpperResearchGroupIds.Add(groupId);
        }
    }

    public void AddMaterial(MaterialType type, MaterialGrade grade, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!_materials.TryGetValue(type, out Dictionary<MaterialGrade, int> byGrade))
        {
            byGrade = new Dictionary<MaterialGrade, int>();
            _materials[type] = byGrade;
        }

        byGrade.TryGetValue(grade, out int currentAmount);
        byGrade[grade] = currentAmount + amount;
    }

    public int GetMaterialAmount(MaterialType type, MaterialGrade grade)
    {
        if (!_materials.TryGetValue(type, out Dictionary<MaterialGrade, int> byGrade))
        {
            return 0;
        }

        byGrade.TryGetValue(grade, out int amount);
        return amount;
    }

    public int GetAvailableMaterialAmount(MaterialType type, MaterialGrade minimumGrade)
    {
        int total = 0;
        for (int grade = (int)minimumGrade; grade <= (int)MaterialGrade.S; grade++)
        {
            total += GetMaterialAmount(type, (MaterialGrade)grade);
        }

        return total;
    }

    public bool HasMaterials(MaterialRequirement[] requirements)
    {
        if (requirements == null)
        {
            return true;
        }

        for (int i = 0; i < requirements.Length; i++)
        {
            MaterialRequirement requirement = requirements[i];
            if (GetAvailableMaterialAmount(requirement.Type, requirement.MinimumGrade) < requirement.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool TryConsumeMaterials(MaterialRequirement[] requirements)
    {
        if (!HasMaterials(requirements))
        {
            return false;
        }

        if (requirements == null)
        {
            return true;
        }

        for (int i = 0; i < requirements.Length; i++)
        {
            ConsumeMaterial(requirements[i]);
        }

        return true;
    }

    public void MarkInitialRaidOriginExplored()
    {
        IsInitialRaidOriginExplored = true;
        InitialRaidOriginExplorationProgress = 100;
    }

    public void AddInitialRaidOriginExplorationProgress(int amount)
    {
        InitialRaidOriginExplorationProgress = Clamp(InitialRaidOriginExplorationProgress + amount, 0, 100);
        if (InitialRaidOriginExplorationProgress >= 100)
        {
            IsInitialRaidOriginExplored = true;
        }
    }

    public void MarkFirstRaidOccurred()
    {
        HasFirstRaidOccurred = true;
    }

    public void MarkFirstRaidCleared()
    {
        HasFirstRaidCleared = true;
    }

    public void RecordRaidResult(bool isWin, bool isPerfectWin, bool isFirstRaid)
    {
        LastRaidTurn = CurrentTurn;
        if (isWin)
        {
            RaidWinCount++;
            if (isPerfectWin)
            {
                PerfectRaidWinCount++;
            }

            if (!isFirstRaid)
            {
                SubsequentRaidWinCount++;
                if (isPerfectWin)
                {
                    SubsequentPerfectRaidWinCount++;
                }
            }

            return;
        }

        RaidLossCount++;
    }

    public void AddClearedDungeonFloor()
    {
        ClearedDungeonFloorCount++;
    }

    public void AddSpecialItem(MaterialGrade rarity, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _specialItems.TryGetValue(rarity, out int currentAmount);
        _specialItems[rarity] = currentAmount + amount;
    }

    public int GetSpecialItemCount(MaterialGrade rarity)
    {
        _specialItems.TryGetValue(rarity, out int amount);
        return amount;
    }

    public void RecordFundsDeficitState()
    {
        FundsDeficitTurnCount = Funds < 0 ? FundsDeficitTurnCount + 1 : 0;
    }

    public void RecordHappinessCrisisState()
    {
        HappinessCrisisTurnCount = Happiness < 20 ? HappinessCrisisTurnCount + 1 : 0;
    }

    public int ConsumeTurnResearchSpeedBonusPercent()
    {
        int bonus = TurnResearchSpeedBonusPercent;
        TurnResearchSpeedBonusPercent = 0;
        return bonus;
    }

    public void EndGame(bool isVictory, string reason)
    {
        IsGameOver = true;
        IsVictory = isVictory;
        GameEndReason = reason;
    }

    public bool TrySpendMetaPoints(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        SpentMetaPoints += amount;
        return true;
    }

    private bool TrySetAssignedWorkers(int foodWorkers, int fundsWorkers, int developmentWorkers)
    {
        if (foodWorkers < 0
            || fundsWorkers < 0
            || developmentWorkers < 0
            || foodWorkers > GameConstants.MAX_ASSIGNED_FOOD_WORKERS
            || fundsWorkers > GameConstants.MAX_ASSIGNED_FUNDS_WORKERS)
        {
            return false;
        }

        if (foodWorkers + fundsWorkers + developmentWorkers > Population)
        {
            return false;
        }

        AssignedFoodWorkers = foodWorkers;
        AssignedFundsWorkers = fundsWorkers;
        AssignedDevelopmentWorkers = developmentWorkers;
        return true;
    }

    private void NormalizeAssignedWorkers()
    {
        AssignedFoodWorkers = Clamp(AssignedFoodWorkers, 0, GameConstants.MAX_ASSIGNED_FOOD_WORKERS);
        AssignedFundsWorkers = Clamp(AssignedFundsWorkers, 0, GameConstants.MAX_ASSIGNED_FUNDS_WORKERS);
        AssignedDevelopmentWorkers = ClampMin(AssignedDevelopmentWorkers, 0);

        int totalAssigned = AssignedFoodWorkers + AssignedFundsWorkers + AssignedDevelopmentWorkers;
        if (totalAssigned <= Population)
        {
            return;
        }

        int overflow = totalAssigned - Population;
        int reducedFromDevelopment = overflow < AssignedDevelopmentWorkers ? overflow : AssignedDevelopmentWorkers;
        AssignedDevelopmentWorkers -= reducedFromDevelopment;
        overflow -= reducedFromDevelopment;

        int reducedFromFunds = overflow < AssignedFundsWorkers ? overflow : AssignedFundsWorkers;
        AssignedFundsWorkers -= reducedFromFunds;
        overflow -= reducedFromFunds;

        int reducedFromFood = overflow < AssignedFoodWorkers ? overflow : AssignedFoodWorkers;
        AssignedFoodWorkers -= reducedFromFood;
    }

    private void ConsumeMaterial(MaterialRequirement requirement)
    {
        int remainingAmount = requirement.Amount;
        for (int grade = (int)requirement.MinimumGrade; grade <= (int)MaterialGrade.S && remainingAmount > 0; grade++)
        {
            MaterialGrade materialGrade = (MaterialGrade)grade;
            int currentAmount = GetMaterialAmount(requirement.Type, materialGrade);
            int consumedAmount = currentAmount < remainingAmount ? currentAmount : remainingAmount;
            if (consumedAmount <= 0)
            {
                continue;
            }

            _materials[requirement.Type][materialGrade] = currentAmount - consumedAmount;
            remainingAmount -= consumedAmount;
        }
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }

    private static int ClampMin(int value, int min)
    {
        return value < min ? min : value;
    }
}
