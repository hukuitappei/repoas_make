public class TurnManager
{
    private readonly GameState _state;
    private readonly EventSystem _eventSystem;
    private readonly ResourceManager _resourceManager;
    private readonly GuildManager _guildManager;
    private readonly HappinessSystem _happinessSystem;
    private readonly RaidSystem _raidSystem;
    private readonly DungeonSystem _dungeonSystem;
    private readonly ResearchTree _researchTree;
    private readonly DevelopmentSystem _developmentSystem;
    private readonly MapData _mapData;

    public TurnManager(
        GameState state,
        EventSystem eventSystem,
        ResourceManager resourceManager,
        GuildManager guildManager,
        HappinessSystem happinessSystem,
        RaidSystem raidSystem,
        DungeonSystem dungeonSystem,
        ResearchTree researchTree,
        DevelopmentSystem developmentSystem,
        MapData mapData)
    {
        _state = state;
        _eventSystem = eventSystem;
        _resourceManager = resourceManager;
        _guildManager = guildManager;
        _happinessSystem = happinessSystem;
        _raidSystem = raidSystem;
        _dungeonSystem = dungeonSystem;
        _researchTree = researchTree;
        _developmentSystem = developmentSystem;
        _mapData = mapData;
    }

    public void AdvanceTurn()
    {
        ResolveBuildingsTurnStart();
        _eventSystem.ResolveTurnStartEvents(_state);
        _resourceManager.ResolveTurnResources(_state);
        _guildManager.ResolveActions(_state);
        int assignedDeveloperMembers = _guildManager != null ? _guildManager.CountAssignedDevelopmentWorkers(_state) : 0;
        _developmentSystem.ResolveTurnDevelopment(_state, _mapData, assignedDeveloperMembers);
        _happinessSystem.Recalculate(_state);
        _raidSystem.ResolveRaidCheck(_state);
        _dungeonSystem.StartAssignedDungeonRuns(_state);
        _dungeonSystem.ResolveExplorationProgress(_state);
        _researchTree.StartAssignedResearch(_state);
        _researchTree.AdvanceResearch(_state);
        ResolveBuildingsTurnEnd();
        _state.AdvanceTurn();
    }

    private void ResolveBuildingsTurnStart()
    {
        for (int i = 0; i < _state.Buildings.Count; i++)
        {
            BuildingBase building = _state.Buildings[i];
            if (building != null && building.IsActive)
            {
                building.OnTurnStart(_state);
            }
        }
    }

    private void ResolveBuildingsTurnEnd()
    {
        for (int i = 0; i < _state.Buildings.Count; i++)
        {
            BuildingBase building = _state.Buildings[i];
            if (building != null && building.IsActive)
            {
                building.OnTurnEnd(_state);
            }
        }
    }
}
