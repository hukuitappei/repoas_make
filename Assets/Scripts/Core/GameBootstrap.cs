using UnityEngine;

#pragma warning disable 0649
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private MainGameScreen mainGameScreen;
    [SerializeField] private MapPanel mapPanel;
    [SerializeField] private GuildData[] guildCatalog;
    [SerializeField] private BuildingData[] availableBuildings;
    [SerializeField] private BuildingData[] startingBuildings;
    [SerializeField] private EventData[] eventCatalog;

    private GameManager _gameManager;
    private MapData _mapData;

    public GameManager CurrentGameManager => _gameManager;
    public MapData CurrentMapData => _mapData;

    private void Awake()
    {
        _gameManager = new GameManager();
        _mapData = _gameManager.MapGenerator != null ? _gameManager.MapGenerator.Generate() : null;

        InitializeGuilds();
        InitializeStartingBuildings();
        InitializeEvents();

        if (mainGameScreen != null)
        {
            mainGameScreen.Bind(_gameManager);
            mainGameScreen.SetAvailableBuildings(availableBuildings);
        }

        if (mapPanel != null)
        {
            mapPanel.Bind(_mapData);
        }
    }

    private void InitializeGuilds()
    {
        if (_gameManager == null || _gameManager.State == null || guildCatalog == null)
        {
            return;
        }

        for (int i = 0; i < guildCatalog.Length; i++)
        {
            GuildData data = guildCatalog[i];
            if (data == null)
            {
                continue;
            }

            bool isUnlocked = data.guildType == GuildType.Warrior || string.IsNullOrEmpty(data.unlockResearchNodeId);
            GuildBase guild = CreateGuild(data, isUnlocked);
            if (guild == null)
            {
                continue;
            }

            _gameManager.State.AddGuild(guild);
            if (data.guildType == GuildType.Warrior && guild.IsUnlocked)
            {
                AddInitialMembers(guild, GameConstants.INITIAL_GUILD_MEMBER_COUNT);
            }
        }
    }

    private void InitializeStartingBuildings()
    {
        if (_gameManager == null || _gameManager.State == null || startingBuildings == null)
        {
            return;
        }

        for (int i = 0; i < startingBuildings.Length; i++)
        {
            BuildingData data = startingBuildings[i];
            if (data == null)
            {
                continue;
            }

            BuildingBase building = CreateBuilding(data);
            if (building == null)
            {
                continue;
            }

            _gameManager.State.AddBuilding(building);
            ApplyPersistentStartingEffect(building);
        }
    }

    private void ApplyPersistentStartingEffect(BuildingBase building)
    {
        if (building == null || building.Data == null || _gameManager == null || _gameManager.State == null)
        {
            return;
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
                building.OnTurnStart(_gameManager.State);
                break;
        }
    }

    private void InitializeEvents()
    {
        if (_gameManager == null || _gameManager.EventSystem == null || eventCatalog == null)
        {
            return;
        }

        for (int i = 0; i < eventCatalog.Length; i++)
        {
            _gameManager.EventSystem.RegisterEvent(eventCatalog[i]);
        }
    }

    private void AddInitialMembers(GuildBase guild, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GuildMember member = new GuildMember(guild.GuildName + " Member " + (i + 1), guild.Data, _gameManager.State.Lord.Popularity);
            guild.AddMember(member);
        }
    }

    private static GuildBase CreateGuild(GuildData data, bool isUnlocked)
    {
        switch (data.guildType)
        {
            case GuildType.Warrior:
                return new WarriorGuild(data, isUnlocked);
            case GuildType.Mage:
                return new MageGuild(data, isUnlocked);
            case GuildType.Craftsman:
                return new CraftsmanGuild(data, isUnlocked);
            default:
                return null;
        }
    }

    private static BuildingBase CreateBuilding(BuildingData data)
    {
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
#pragma warning restore 0649
