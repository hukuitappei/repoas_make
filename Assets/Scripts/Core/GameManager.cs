public class GameManager
{
    public GameState State { get; private set; }
    public TurnManager TurnManager { get; private set; }
    public ResourceManager ResourceManager { get; private set; }
    public ResearchTree ResearchTree { get; private set; }
    public EventSystem EventSystem { get; private set; }
    public RaidSystem RaidSystem { get; private set; }
    public DungeonSystem DungeonSystem { get; private set; }
    public GuildManager GuildManager { get; private set; }
    public HappinessSystem HappinessSystem { get; private set; }
    public MetaProgressionSystem MetaProgressionSystem { get; private set; }
    public ScoreCalculator ScoreCalculator { get; private set; }
    public MapGenerator MapGenerator { get; private set; }

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
        MetaProgressionSystem = new MetaProgressionSystem();
        ScoreCalculator = new ScoreCalculator();
        MapGenerator = new MapGenerator();
        TurnManager = new TurnManager(State, EventSystem, ResourceManager, GuildManager, HappinessSystem, RaidSystem, DungeonSystem, ResearchTree);
    }

    public void AdvanceTurn()
    {
        if (State.IsGameOver)
        {
            return;
        }

        TurnManager.AdvanceTurn();
        EvaluateGameEnd();
    }

    public void EvaluateGameEnd()
    {
        if (State.IsGameOver)
        {
            return;
        }

        if (State.Population <= 0)
        {
            State.EndGame(false, "Population reached zero.");
            return;
        }

        if (State.FundsDeficitTurnCount >= GameConstants.FUNDS_DEFICIT_DEFEAT_TURNS)
        {
            State.EndGame(false, "Funds stayed negative for too long.");
            return;
        }

        if (State.HappinessCrisisTurnCount >= GameConstants.HAPPINESS_CRISIS_DEFEAT_TURNS)
        {
            State.EndGame(false, "Happiness crisis continued for too long.");
            return;
        }

        if (State.CurrentTurn > GameConstants.MAX_TURNS)
        {
            State.EndGame(true, "Completed 60 turns.");
        }
    }
}
