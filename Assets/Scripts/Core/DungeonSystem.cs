using System.Collections.Generic;

public class DungeonSystem
{
    private const int FAILURE_COMBAT_PENALTY_PERCENT = 30;
    private const int FAILURE_COMBAT_PENALTY_TURNS = 2;

    private readonly List<DungeonRun> _activeRuns;
    private readonly System.Random _random;

    public IReadOnlyList<DungeonRun> ActiveRuns => _activeRuns;

    public DungeonSystem()
    {
        _activeRuns = new List<DungeonRun>();
        _random = new System.Random();
    }

    public bool HasActiveRun(GuildMember member)
    {
        if (member == null)
        {
            return false;
        }

        for (int i = 0; i < _activeRuns.Count; i++)
        {
            DungeonRun run = _activeRuns[i];
            if (run != null && run.Member == member)
            {
                return true;
            }
        }

        return false;
    }

    public bool StartExploration(GuildMember member)
    {
        if (member == null
            || member.CurrentAction != GuildAction.Explore
            || member.CurrentActionTargetId != GameConstants.EXPLORATION_TARGET_DUNGEON
            || HasActiveRun(member))
        {
            return false;
        }

        _activeRuns.Add(new DungeonRun(member));
        member.SetInDungeonRun(true);
        return true;
    }

    public void StartAssignedDungeonRuns(GameState state)
    {
        if (state == null || !state.IsDungeonExplorationUnlocked || !state.IsInitialRaidOriginExplored)
        {
            return;
        }

        for (int i = 0; i < state.Guilds.Count; i++)
        {
            GuildBase guild = state.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            for (int j = 0; j < guild.Members.Count; j++)
            {
                GuildMember member = guild.Members[j];
                if (member == null
                    || member.IsInDungeonRun
                    || member.CurrentAction != GuildAction.Explore
                    || member.CurrentActionTargetId != GameConstants.EXPLORATION_TARGET_DUNGEON)
                {
                    continue;
                }

                StartExploration(member);
            }
        }
    }

    public void ResolveExplorationProgress(GameState state)
    {
        if (state == null)
        {
            return;
        }

        for (int i = _activeRuns.Count - 1; i >= 0; i--)
        {
            DungeonRun run = _activeRuns[i];
            run.Advance(state.DungeonFloorSpeedBonus);
            if (!run.IsFloorCompleted())
            {
                continue;
            }

            if (IsExplorationFailed(run))
            {
                ApplyFailurePenalty(run.Member);
                run.Member.SetInDungeonRun(false);
                run.Member.ClearAction();
                _activeRuns.RemoveAt(i);
                continue;
            }

            ApplyFloorReward(state, run.CurrentFloor);
            state.AddClearedDungeonFloor();
            if (!run.MoveToNextFloor())
            {
                run.Member.SetInDungeonRun(false);
                run.Member.ClearAction();
                _activeRuns.RemoveAt(i);
            }
        }
    }

    private void ApplyFloorReward(GameState state, int floor)
    {
        int rewardBonus = state.DungeonRewardPercentBonus;
        if (floor == 1)
        {
            AddReward(state, 60, MaterialType.Stone, 20, MaterialType.Wood, 20, MaterialType.Foodstuff, 10, rewardBonus);
        }
        else if (floor == 2)
        {
            AddReward(state, 90, MaterialType.Stone, 25, MaterialType.Wood, 25, MaterialType.Metal, 15, rewardBonus);
            state.AddMaterial(MaterialType.Magic, MaterialGrade.F, ApplyPercent(5, rewardBonus));
        }
        else if (floor == 3)
        {
            state.AddFunds(ApplyPercent(130, rewardBonus));
            state.AddMaterial(MaterialType.Stone, MaterialGrade.E, ApplyPercent(20, rewardBonus));
            state.AddMaterial(MaterialType.Wood, MaterialGrade.E, ApplyPercent(20, rewardBonus));
            state.AddMaterial(MaterialType.Metal, MaterialGrade.F, ApplyPercent(25, rewardBonus));
            state.AddMaterial(MaterialType.Magic, MaterialGrade.F, ApplyPercent(10, rewardBonus));
        }
        else if (floor == 4)
        {
            state.AddFunds(ApplyPercent(180, rewardBonus));
            state.AddMaterial(MaterialType.Stone, MaterialGrade.E, ApplyPercent(25, rewardBonus));
            state.AddMaterial(MaterialType.Metal, MaterialGrade.E, ApplyPercent(25, rewardBonus));
            state.AddMaterial(MaterialType.Magic, MaterialGrade.E, ApplyPercent(15, rewardBonus));
        }
        else if (floor == 5)
        {
            state.AddFunds(ApplyPercent(250, rewardBonus));
            state.AddMaterial(MaterialType.Stone, MaterialGrade.C, ApplyPercent(20, rewardBonus));
            state.AddMaterial(MaterialType.Metal, MaterialGrade.C, ApplyPercent(20, rewardBonus));
            state.AddMaterial(MaterialType.Magic, MaterialGrade.C, ApplyPercent(20, rewardBonus));
        }

        RollSpecialItem(state, floor);
    }

    private void AddReward(GameState state, int funds, MaterialType firstType, int firstAmount, MaterialType secondType, int secondAmount, MaterialType thirdType, int thirdAmount, int rewardBonus)
    {
        state.AddFunds(ApplyPercent(funds, rewardBonus));
        state.AddMaterial(firstType, MaterialGrade.F, ApplyPercent(firstAmount, rewardBonus));
        state.AddMaterial(secondType, MaterialGrade.F, ApplyPercent(secondAmount, rewardBonus));
        state.AddMaterial(thirdType, MaterialGrade.F, ApplyPercent(thirdAmount, rewardBonus));
    }

    private int ApplyPercent(int value, int percent)
    {
        return value + value * percent / 100;
    }

    private bool IsExplorationFailed(DungeonRun run)
    {
        if (run == null || run.Member == null)
        {
            return true;
        }

        int requiredPower = GetRequiredPower(run.CurrentFloor);
        int memberPower = run.Member.CurrentCombatPower + run.Member.CurrentSkillPower;
        if (memberPower >= requiredPower)
        {
            return false;
        }

        int successRange = memberPower * 100 / requiredPower;
        return _random.Next(0, 100) >= successRange;
    }

    private static int GetRequiredPower(int floor)
    {
        if (floor <= 1)
        {
            return 18;
        }

        if (floor == 2)
        {
            return 26;
        }

        if (floor == 3)
        {
            return 34;
        }

        if (floor == 4)
        {
            return 44;
        }

        return 56;
    }

    private static void ApplyFailurePenalty(GuildMember member)
    {
        if (member == null)
        {
            return;
        }

        member.ApplyTemporaryCombatPenaltyPercent(FAILURE_COMBAT_PENALTY_PERCENT, FAILURE_COMBAT_PENALTY_TURNS);
    }

    private void RollSpecialItem(GameState state, int floor)
    {
        int roll = _random.Next(0, 100);
        if (floor == 1)
        {
            AddSpecialItemByRoll(state, roll, 40, MaterialGrade.F, 50, MaterialGrade.E);
        }
        else if (floor == 2)
        {
            AddSpecialItemByRoll(state, roll, 35, MaterialGrade.F, 50, MaterialGrade.E, 55, MaterialGrade.D);
        }
        else if (floor == 3)
        {
            AddSpecialItemByRoll(state, roll, 25, MaterialGrade.E, 40, MaterialGrade.D, 45, MaterialGrade.C);
        }
        else if (floor == 4)
        {
            AddSpecialItemByRoll(state, roll, 25, MaterialGrade.D, 40, MaterialGrade.C, 45, MaterialGrade.B);
        }
        else if (floor == 5)
        {
            AddSpecialItemByRoll(state, roll, 25, MaterialGrade.C, 40, MaterialGrade.B, 45, MaterialGrade.A, 46, MaterialGrade.S);
        }
    }

    private void AddSpecialItemByRoll(GameState state, int roll, int firstThreshold, MaterialGrade firstGrade, int secondThreshold, MaterialGrade secondGrade)
    {
        if (roll < firstThreshold)
        {
            state.AddSpecialItem(firstGrade, 1);
        }
        else if (roll < secondThreshold)
        {
            state.AddSpecialItem(secondGrade, 1);
        }
    }

    private void AddSpecialItemByRoll(GameState state, int roll, int firstThreshold, MaterialGrade firstGrade, int secondThreshold, MaterialGrade secondGrade, int thirdThreshold, MaterialGrade thirdGrade)
    {
        if (roll < firstThreshold)
        {
            state.AddSpecialItem(firstGrade, 1);
        }
        else if (roll < secondThreshold)
        {
            state.AddSpecialItem(secondGrade, 1);
        }
        else if (roll < thirdThreshold)
        {
            state.AddSpecialItem(thirdGrade, 1);
        }
    }

    private void AddSpecialItemByRoll(GameState state, int roll, int firstThreshold, MaterialGrade firstGrade, int secondThreshold, MaterialGrade secondGrade, int thirdThreshold, MaterialGrade thirdGrade, int fourthThreshold, MaterialGrade fourthGrade)
    {
        if (roll < firstThreshold)
        {
            state.AddSpecialItem(firstGrade, 1);
        }
        else if (roll < secondThreshold)
        {
            state.AddSpecialItem(secondGrade, 1);
        }
        else if (roll < thirdThreshold)
        {
            state.AddSpecialItem(thirdGrade, 1);
        }
        else if (roll < fourthThreshold)
        {
            state.AddSpecialItem(fourthGrade, 1);
        }
    }
}
