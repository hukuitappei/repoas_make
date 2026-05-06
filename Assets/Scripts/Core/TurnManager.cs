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

    public TurnManager(
        GameState state,
        EventSystem eventSystem,
        ResourceManager resourceManager,
        GuildManager guildManager,
        HappinessSystem happinessSystem,
        RaidSystem raidSystem,
        DungeonSystem dungeonSystem,
        ResearchTree researchTree)
    {
        _state = state;
        _eventSystem = eventSystem;
        _resourceManager = resourceManager;
        _guildManager = guildManager;
        _happinessSystem = happinessSystem;
        _raidSystem = raidSystem;
        _dungeonSystem = dungeonSystem;
        _researchTree = researchTree;
    }

    public void AdvanceTurn()
    {
        _eventSystem.ResolveTurnStartEvents(_state);
        _resourceManager.ResolveTurnResources(_state);
        _guildManager.ResolveActions(_state);
        _happinessSystem.Recalculate(_state);
        _raidSystem.ResolveRaidCheck(_state);
        _dungeonSystem.ResolveExplorationProgress(_state);
        _researchTree.AdvanceResearch(_state);
        _state.AdvanceTurn();
    }
}
