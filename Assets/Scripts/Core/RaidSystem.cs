using System;

public class RaidSystem
{
    private readonly Random _random;

    public RaidResult LastRaidResult { get; private set; }

    public RaidSystem()
    {
        _random = new Random();
        LastRaidResult = new RaidResult(RaidOutcome.None, 0, 0);
    }

    public bool ExploreInitialRaidOrigin(GameState state)
    {
        if (state == null || state.IsInitialRaidOriginExplored)
        {
            return false;
        }

        bool isSuccess = _random.NextDouble() < GameConstants.EXPLORATION_SUCCESS_RATE;
        int progress = isSuccess ? GameConstants.EXPLORATION_SUCCESS_PROGRESS : GameConstants.EXPLORATION_FAILURE_PROGRESS;
        state.AddInitialRaidOriginExplorationProgress(progress);
        return isSuccess;
    }

    public RaidResult ResolveRaidCheck(GameState state)
    {
        if (state == null)
        {
            return LastRaidResult;
        }

        if (!state.HasFirstRaidOccurred)
        {
            return ResolveFirstRaidCheck(state);
        }

        return ResolveSubsequentRaidCheck(state);
    }

    private RaidResult ResolveFirstRaidCheck(GameState state)
    {
        if (state.CurrentTurn < GameConstants.FIRST_RAID_START_TURN)
        {
            return LastRaidResult;
        }

        int elapsedTurns = state.CurrentTurn - GameConstants.FIRST_RAID_START_TURN;
        bool isForced = elapsedTurns >= GameConstants.FIRST_RAID_FORCED_AFTER_TURNS;
        bool shouldOccur = isForced || _random.NextDouble() < CalculateFirstRaidProbability(elapsedTurns);
        if (!shouldOccur)
        {
            return LastRaidResult;
        }

        int enemyPower = state.IsInitialRaidOriginExplored
            ? GameConstants.FIRST_RAID_POWER_EXPLORED
            : GameConstants.FIRST_RAID_POWER_UNEXPLORED;
        int defensePower = CalculateDefensePower(state);
        LastRaidResult = ResolveRaid(state, enemyPower, defensePower, isFirstRaid: true);
        state.MarkFirstRaidOccurred();
        if (LastRaidResult.Outcome == RaidOutcome.PerfectWin || LastRaidResult.Outcome == RaidOutcome.CloseWin)
        {
            state.MarkFirstRaidCleared();
        }

        return LastRaidResult;
    }

    private RaidResult ResolveSubsequentRaidCheck(GameState state)
    {
        if (_random.NextDouble() >= GameConstants.SUBSEQUENT_RAID_BASE_PROBABILITY)
        {
            return LastRaidResult;
        }

        int enemyPower = GameConstants.SUBSEQUENT_RAID_POWER_BASE
            + state.CurrentTurn * GameConstants.SUBSEQUENT_RAID_POWER_PER_TURN;
        int defensePower = CalculateDefensePower(state);
        LastRaidResult = ResolveRaid(state, enemyPower, defensePower, isFirstRaid: false);
        return LastRaidResult;
    }

    public int CalculateDefensePower(GameState state)
    {
        if (state == null)
        {
            return 0;
        }

        int defensePower = GameConstants.INITIAL_CITY_DEFENSE + state.DefensePowerBonus;
        for (int i = 0; i < state.Guilds.Count; i++)
        {
            GuildBase guild = state.Guilds[i];
            if (guild != null)
            {
                defensePower += guild.CalculateCombatPower();
            }
        }

        return defensePower;
    }

    private RaidResult ResolveRaid(GameState state, int enemyPower, int defensePower, bool isFirstRaid)
    {
        RaidOutcome outcome;
        if (defensePower >= enemyPower * 2)
        {
            outcome = RaidOutcome.PerfectWin;
            state.RecordRaidResult(true, true, isFirstRaid);
        }
        else if (defensePower >= enemyPower)
        {
            outcome = RaidOutcome.CloseWin;
            state.RecordRaidResult(true, false, isFirstRaid);
            state.AddFunds(-20);
        }
        else if (defensePower * 2 >= enemyPower)
        {
            outcome = RaidOutcome.Loss;
            state.RecordRaidResult(false, false, isFirstRaid);
            state.AddFunds(-80);
            state.AddFood(-100);
        }
        else
        {
            outcome = RaidOutcome.Collapse;
            state.RecordRaidResult(false, false, isFirstRaid);
            state.AddFunds(-150);
            state.AddFood(-200);
            state.AddPopulation(-(state.Population / 5));
        }

        return new RaidResult(outcome, enemyPower, defensePower);
    }

    private double CalculateFirstRaidProbability(int elapsedTurns)
    {
        double probability = 0.15d * Math.Pow(1.8d, elapsedTurns);
        return probability > 1.0d ? 1.0d : probability;
    }
}
