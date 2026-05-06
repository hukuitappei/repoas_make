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

    public bool TryAssignGuildAction(GuildBase guild, GuildMember member, GuildAction action)
    {
        if (State == null || guild == null || member == null)
        {
            return false;
        }

        guild.AssignAction(member, action);
        return member.CurrentAction == action;
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

    public bool TryStartDungeonExploration(GuildMember member, out string reason)
    {
        if (State == null || DungeonSystem == null)
        {
            reason = "Game state is not ready.";
            return false;
        }

        if (!State.IsDungeonExplorationUnlocked)
        {
            reason = "Dungeon exploration is locked.";
            return false;
        }

        if (member == null)
        {
            reason = "No explorer is selected.";
            return false;
        }

        if (member.CurrentAction != GuildAction.Explore)
        {
            reason = member.Name + " is not assigned to Explore.";
            return false;
        }

        if (DungeonSystem.HasActiveRun(member))
        {
            reason = member.Name + " is already in a dungeon run.";
            return false;
        }

        bool started = DungeonSystem.StartExploration(member);
        reason = started ? member.Name + " entered the dungeon." : "Failed to start the dungeon run.";
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
