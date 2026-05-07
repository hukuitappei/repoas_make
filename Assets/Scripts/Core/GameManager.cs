using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public string LastTurnWarningMessage { get; private set; }

    public GameState State { get; private set; }
    public TurnManager TurnManager { get; private set; }
    public ResourceManager ResourceManager { get; private set; }
    public ResearchTree ResearchTree { get; private set; }
    public EventSystem EventSystem { get; private set; }
    public RaidSystem RaidSystem { get; private set; }
    public DungeonSystem DungeonSystem { get; private set; }
    public GuildManager GuildManager { get; private set; }
    public HappinessSystem HappinessSystem { get; private set; }
    public DevelopmentSystem DevelopmentSystem { get; private set; }
    public MetaProgressionSystem MetaProgressionSystem { get; private set; }
    public ScoreCalculator ScoreCalculator { get; private set; }
    public MapGenerator MapGenerator { get; private set; }
    public MapData CurrentMapData { get; private set; }

    public GameManager()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        State = new GameState();
        ResourceManager = new ResourceManager();
        ResearchTree = new ResearchTree();
        EventSystem = new EventSystem();
        RaidSystem = new RaidSystem();
        DungeonSystem = new DungeonSystem();
        GuildManager = new GuildManager();
        HappinessSystem = new HappinessSystem();
        DevelopmentSystem = new DevelopmentSystem();
        MetaProgressionSystem = new MetaProgressionSystem();
        ScoreCalculator = new ScoreCalculator();
        MapGenerator = new MapGenerator();
        TurnManager = new TurnManager(State, EventSystem, ResourceManager, GuildManager, HappinessSystem, RaidSystem, DungeonSystem, ResearchTree, DevelopmentSystem, CurrentMapData);
    }

    public void SetMapData(MapData mapData)
    {
        CurrentMapData = mapData;
        TurnManager = new TurnManager(State, EventSystem, ResourceManager, GuildManager, HappinessSystem, RaidSystem, DungeonSystem, ResearchTree, DevelopmentSystem, CurrentMapData);
    }

    public void AdvanceTurn()
    {
        if (State.IsGameOver)
        {
            return;
        }

        LastTurnWarningMessage = BuildIdleWarningMessage();
        if (!string.IsNullOrEmpty(LastTurnWarningMessage))
        {
            Debug.LogWarning(LastTurnWarningMessage);
        }

        TurnManager.AdvanceTurn();
        EvaluateGameEnd();
    }

    public bool TryAssignGuildAction(GuildBase guild, GuildMember member, GuildAction action, string targetId = null)
    {
        if (State == null || guild == null || member == null)
        {
            return false;
        }

        if (action == GuildAction.Explore && targetId == GameConstants.EXPLORATION_TARGET_DUNGEON)
        {
            if (!State.IsDungeonExplorationUnlocked || !State.IsInitialRaidOriginExplored)
            {
                return false;
            }
        }

        if (targetId != null)
        {
            member.SetActionTarget(targetId);
        }

        guild.AssignAction(member, action);
        return member.CurrentAction == action && (targetId == null || member.CurrentActionTargetId == targetId);
    }

    public bool TrySetAssignedFoodWorkers(int value)
    {
        return State != null && State.TrySetAssignedFoodWorkers(value);
    }

    public bool TrySetAssignedFundsWorkers(int value)
    {
        return State != null && State.TrySetAssignedFundsWorkers(value);
    }

    public bool TrySetAssignedDevelopmentWorkers(int value)
    {
        return State != null && State.TrySetAssignedDevelopmentWorkers(value);
    }

    public int GetFoodExchangeGainFor100Funds()
    {
        int negotiationBonus = State != null && State.Lord != null
            ? (State.Lord.Negotiation / 10) * GameConstants.FOOD_EXCHANGE_BONUS_PER_10_NEGOTIATION
            : 0;
        return GameConstants.FOOD_EXCHANGE_BASE_FOOD_GAIN + negotiationBonus;
    }

    public bool TryExchangeFundsForFood(out string message)
    {
        if (State == null)
        {
            message = "ゲーム状態が初期化されていません。";
            return false;
        }

        if (!State.TrySpendFunds(GameConstants.FOOD_EXCHANGE_FUNDS_COST))
        {
            message = "資金が足りません。";
            return false;
        }

        int gainedFood = GetFoodExchangeGainFor100Funds();
        State.AddFood(gainedFood);
        message = $"{GameConstants.FOOD_EXCHANGE_FUNDS_COST}資金を{gainedFood}食料に交換しました。";
        return true;
    }

    public GuildMember FindFirstMemberByAction(GuildAction action)
    {
        if (State == null)
        {
            return null;
        }

        for (int i = 0; i < State.Guilds.Count; i++)
        {
            GuildBase guild = State.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            for (int j = 0; j < guild.Members.Count; j++)
            {
                GuildMember member = guild.Members[j];
                if (member != null && member.CurrentAction == action)
                {
                    return member;
                }
            }
        }

        return null;
    }

    public string BuildIdleWarningMessage()
    {
        if (State == null)
        {
            return string.Empty;
        }

        List<string> idleMembers = new List<string>();
        for (int i = 0; i < State.Guilds.Count; i++)
        {
            GuildBase guild = State.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            for (int j = 0; j < guild.Members.Count; j++)
            {
                GuildMember member = guild.Members[j];
                if (member != null && member.CurrentAction == GuildAction.Idle && !member.IsInDungeonRun)
                {
                    idleMembers.Add(member.Name);
                }
            }
        }

        if (idleMembers.Count == 0)
        {
            return string.Empty;
        }

        return "警告: 待機中のメンバーがいます - " + string.Join(", ", idleMembers);
    }

    public bool TryStartDungeonExploration(GuildMember member, out string reason)
    {
        if (State == null || DungeonSystem == null)
        {
            reason = "ゲーム状態が未初期化です。";
            return false;
        }

        if (!State.IsDungeonExplorationUnlocked)
        {
            reason = "ダンジョン探索はまだ解放されていません。";
            return false;
        }

        if (!State.IsInitialRaidOriginExplored)
        {
            reason = "襲撃元の探索が完了していません。";
            return false;
        }

        if (member == null)
        {
            reason = "探索担当メンバーが選ばれていません。";
            return false;
        }

        if (member.CurrentAction != GuildAction.Explore)
        {
            reason = member.Name + " は探索行動になっていません。";
            return false;
        }

        if (member.CurrentActionTargetId != GameConstants.EXPLORATION_TARGET_DUNGEON)
        {
            reason = member.Name + " の探索対象がダンジョンではありません。";
            return false;
        }

        if (DungeonSystem.HasActiveRun(member))
        {
            reason = member.Name + " は既にダンジョン探索中です。";
            return false;
        }

        bool started = DungeonSystem.StartExploration(member);
        reason = started
            ? member.Name + " がダンジョンに突入しました。"
            : "ダンジョン突入を開始できませんでした。";
        return started;
    }

    public void EvaluateGameEnd()
    {
        if (State.IsGameOver)
        {
            return;
        }

        if (State.Population <= 0)
        {
            State.EndGame(false, "人口がゼロになりました。");
            return;
        }

        if (State.FundsDeficitTurnCount >= GameConstants.FUNDS_DEFICIT_DEFEAT_TURNS)
        {
            State.EndGame(false, "資金赤字が長期間続きました。");
            return;
        }

        if (State.HappinessCrisisTurnCount >= GameConstants.HAPPINESS_CRISIS_DEFEAT_TURNS)
        {
            State.EndGame(false, "幸福度危機が長期間続きました。");
            return;
        }

        if (State.CurrentTurn > GameConstants.MAX_TURNS)
        {
            State.EndGame(true, "60ターンを完遂しました。");
        }
    }
    public int GetGuildHireCost(GuildBase guild)
    {
        if (guild == null || guild.Data == null)
        {
            return 0;
        }

        int reductionPercent = State != null ? State.HireCostReductionPercent : 0;
        int baseCost = guild.Data.hireCostFunds;
        return baseCost - baseCost * reductionPercent / 100;
    }

    public bool TryHireGuildMember(GuildBase guild, out string message)
    {
        if (State == null || guild == null || guild.Data == null)
        {
            message = "ギルド加入処理を開始できません。";
            return false;
        }

        if (!guild.IsUnlocked)
        {
            message = guild.GuildName + " は未解放です。";
            return false;
        }

        if (!guild.CanAddMember())
        {
            message = guild.GuildName + " はこれ以上メンバーを追加できません。";
            return false;
        }

        int hireCost = GetGuildHireCost(guild);
        if (!State.TrySpendFunds(hireCost))
        {
            message = guild.GuildName + " の加入資金が不足しています。";
            return false;
        }

        GuildMember member = new GuildMember(BuildGuildMemberName(guild), guild.Data, State.Lord != null ? State.Lord.Popularity : 0);
        if (!guild.AddMember(member))
        {
            State.AddFunds(hireCost);
            message = guild.GuildName + " にメンバーを追加できませんでした。";
            return false;
        }

        message = guild.GuildName + " に " + member.Name + " が加入しました。";
        return true;
    }

    public bool TryBuildOrUpgradeBuilding(BuildingData data, out string message)
    {
        if (State == null || data == null)
        {
            message = "建設データが見つかりません。";
            return false;
        }

        BuildingBase ownedBuilding = FindOwnedBuilding(data.buildingName);
        if (ownedBuilding == null)
        {
            int buildCostFunds = data.GetBuildCostFunds(1);
            MaterialRequirement[] buildRequirements = data.GetBuildMaterialRequirements(1);
            if (!State.TrySpendFunds(buildCostFunds))
            {
                message = data.buildingName + " の建設資金が不足しています。";
                return false;
            }

            if (!State.TryConsumeMaterials(buildRequirements))
            {
                State.AddFunds(buildCostFunds);
                message = data.buildingName + " の建設素材が不足しています。";
                return false;
            }

            BuildingBase newBuilding = CreateBuilding(data);
            if (newBuilding == null)
            {
                State.AddFunds(buildCostFunds);
                message = data.buildingName + " の建設クラスを生成できません。";
                return false;
            }

            State.AddBuilding(newBuilding);
            ApplyImmediateBuildingEffect(newBuilding);
            message = data.buildingName + " を建設しました。";
            return true;
        }

        if (!ownedBuilding.CanUpgrade(State))
        {
            message = ownedBuilding.Name + " は現在アップグレードできません。";
            return false;
        }

        int nextLevel = ownedBuilding.Level + 1;
        int upgradeCostFunds = data.GetBuildCostFunds(nextLevel);
        MaterialRequirement[] upgradeRequirements = data.GetBuildMaterialRequirements(nextLevel);
        if (!State.TrySpendFunds(upgradeCostFunds))
        {
            message = ownedBuilding.Name + " の強化資金が不足しています。";
            return false;
        }

        if (!State.TryConsumeMaterials(upgradeRequirements))
        {
            State.AddFunds(upgradeCostFunds);
            message = ownedBuilding.Name + " の強化素材が不足しています。";
            return false;
        }

        ownedBuilding.Upgrade();
        ApplyImmediateBuildingEffect(ownedBuilding);
        message = ownedBuilding.Name + " を Lv." + ownedBuilding.Level + " に強化しました。";
        return true;
    }

    private BuildingBase FindOwnedBuilding(string buildingName)
    {
        if (State == null || string.IsNullOrEmpty(buildingName))
        {
            return null;
        }

        for (int i = 0; i < State.Buildings.Count; i++)
        {
            BuildingBase building = State.Buildings[i];
            if (building != null && building.Name == buildingName)
            {
                return building;
            }
        }

        return null;
    }

    private void ApplyImmediateBuildingEffect(BuildingBase building)
    {
        if (building == null || State == null || !RequiresImmediateBuildingEffect(building))
        {
            return;
        }

        building.OnTurnStart(State);
    }

    private static bool RequiresImmediateBuildingEffect(BuildingBase building)
    {
        if (building == null || building.Data == null)
        {
            return false;
        }

        switch (building.Data.effectType)
        {
            case BuildingEffectType.ResearchSpeedPercent:
            case BuildingEffectType.DefensePower:
            case BuildingEffectType.WarriorCombatBonus:
            case BuildingEffectType.PopulationCapacity:
            case BuildingEffectType.HireCostReductionPercent:
            case BuildingEffectType.DungeonUnlock:
            case BuildingEffectType.DungeonRewardPercent:
            case BuildingEffectType.HappinessBonus:
            case BuildingEffectType.MoraleBonus:
                return true;
            default:
                return false;
        }
    }

    private static string BuildGuildMemberName(GuildBase guild)
    {
        int sequence = guild.Members.Count + 1;
        return guild.GuildName + " Member " + sequence;
    }

    private static BuildingBase CreateBuilding(BuildingData data)
    {
        if (data == null)
        {
            return null;
        }

        switch (data.effectType)
        {
            case BuildingEffectType.FoodProduction:
                return new Farm(data);
            case BuildingEffectType.WoodProduction:
                return new Sawmill(data);
            case BuildingEffectType.FundsProduction:
                return new Market(data);
            case BuildingEffectType.ResearchSpeedPercent:
                return new Library(data);
            case BuildingEffectType.DefensePower:
                return new Wall(data);
            case BuildingEffectType.WarriorCombatBonus:
                return new Barracks(data);
            case BuildingEffectType.PopulationCapacity:
                return new House(data);
            case BuildingEffectType.HireCostReductionPercent:
                return new Inn(data);
            case BuildingEffectType.DungeonUnlock:
            case BuildingEffectType.DungeonRewardPercent:
                return new DungeonGate(data);
            case BuildingEffectType.HappinessBonus:
                return new Plaza(data);
            case BuildingEffectType.MoraleBonus:
                return new Tavern(data);
            default:
                return null;
        }
    }
}
