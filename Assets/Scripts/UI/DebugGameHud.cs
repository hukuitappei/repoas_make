using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DebugGameHud : MonoBehaviour
{
    [SerializeField] private GameBootstrap bootstrap;

    private Vector2 _scroll;
    private int _selectedMemberIndex;
    private int _selectedResearchNodeIndex;
    private int _selectedConstructTargetIndex;
    private int _selectedDevelopmentCandidateIndex;
    private string _latestMessage;

    private void Awake()
    {
        if (bootstrap == null)
        {
            bootstrap = FindFirstObjectByType<GameBootstrap>();
        }
    }

    private void OnGUI()
    {
        GameManager gameManager = bootstrap != null ? bootstrap.CurrentGameManager : null;
        if (gameManager == null || gameManager.State == null)
        {
            GUI.Box(new Rect(20f, 20f, 360f, 60f), "GameBootstrap / GameManager が見つかりません。");
            return;
        }

        GameState state = gameManager.State;
        GUILayout.BeginArea(new Rect(16f, 16f, 780f, Screen.height - 32f), GUI.skin.box);
        _scroll = GUILayout.BeginScrollView(_scroll);

        GUILayout.Label($"ターン: {state.CurrentTurn} / {GameConstants.MAX_TURNS}");
        GUILayout.Label(state.IsGameOver
            ? $"状態: {(state.IsVictory ? "勝利" : "敗北")} / {state.GameEndReason}"
            : "状態: 進行中");
        GUILayout.Space(8f);

        GUILayout.Label($"食料 {state.Food} / 資金 {state.Funds} / 人口 {state.Population}/{state.PopulationCapacity} / 幸福度 {state.Happiness}");
        GUILayout.Label($"襲撃元探索: {state.InitialRaidOriginExplorationProgress}% / ダンジョン: {(state.IsDungeonExplorationUnlocked ? "解放済み" : "未解放")} / 襲撃元: {(state.IsInitialRaidOriginExplored ? "発見済み" : "未発見")}");
        GUILayout.Label($"進行中ダンジョン: {(gameManager.DungeonSystem != null ? gameManager.DungeonSystem.ActiveRuns.Count : 0)}");
        GUILayout.Space(4f);

        DrawWarningBox(gameManager.BuildIdleWarningMessage());
        DrawLatestMessageBox();
        GUILayout.Space(12f);

        DrawPopulationAssignmentSection(gameManager);
        GUILayout.Space(12f);

        DrawMapSection(gameManager);
        GUILayout.Space(12f);

        DrawGuildSection(gameManager);
        GUILayout.Space(12f);

        DrawExplorationSection(gameManager);
        GUILayout.Space(12f);

        if (GUILayout.Button("ターン終了", GUILayout.Height(32f)))
        {
            gameManager.AdvanceTurn();
            _latestMessage = BuildTurnResultMessage(gameManager);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawPopulationAssignmentSection(GameManager gameManager)
    {
        GameState state = gameManager.State;
        GUILayout.Label("人口配分");
        GUILayout.Label($"食料担当 {state.AssignedFoodWorkers}/{GameConstants.MAX_ASSIGNED_FOOD_WORKERS} / 資金担当 {state.AssignedFundsWorkers}/{GameConstants.MAX_ASSIGNED_FUNDS_WORKERS} / 開拓担当 {state.AssignedDevelopmentWorkers} / 自由人口 {state.FreePopulation}");
        GUILayout.Label($"想定生産: 食料 +{state.AssignedFoodWorkers * GameConstants.FOOD_PRODUCTION_PER_ASSIGNED_WORKER} / 資金 +{state.AssignedFundsWorkers * GameConstants.FUNDS_PRODUCTION_PER_ASSIGNED_WORKER}");

        List<Vector2Int> candidates = CollectDevelopmentCandidates(gameManager.CurrentMapData);
        if (candidates.Count > 0)
        {
            ClampDevelopmentCandidateIndex(candidates.Count);
            Vector2Int candidate = candidates[_selectedDevelopmentCandidateIndex];
            int distance = gameManager.CurrentMapData.GetDistanceFromHome(candidate.x, candidate.y);
            float successRate = gameManager.DevelopmentSystem != null
                ? gameManager.DevelopmentSystem.CalculateTurnGain(state, gameManager.CurrentMapData, candidate.x, candidate.y)
                : 0f;

            GUILayout.Label($"開拓候補: ({candidate.x},{candidate.y}) / 距離 {distance} / 今回加算 {(int)(successRate * 100f)}%");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("候補前", GUILayout.Width(80f)))
            {
                _selectedDevelopmentCandidateIndex = (_selectedDevelopmentCandidateIndex - 1 + candidates.Count) % candidates.Count;
            }
            if (GUILayout.Button("候補次", GUILayout.Width(80f)))
            {
                _selectedDevelopmentCandidateIndex = (_selectedDevelopmentCandidateIndex + 1) % candidates.Count;
            }
            if (GUILayout.Button("このマスを開拓", GUILayout.Width(140f)))
            {
                string message = "開拓システムが初期化されていません。";
                if (gameManager.DevelopmentSystem != null
                    && gameManager.DevelopmentSystem.TrySelectTarget(gameManager.CurrentMapData, candidate.x, candidate.y, out message))
                {
                    _latestMessage = message;
                }
                else
                {
                    _latestMessage = message;
                }
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("開拓候補: なし");
        }

        if (gameManager.DevelopmentSystem != null && gameManager.DevelopmentSystem.HasSelectedTarget)
        {
            GUILayout.Label($"選択中の開拓対象: ({gameManager.DevelopmentSystem.SelectedTargetX},{gameManager.DevelopmentSystem.SelectedTargetY})");
        }
        else
        {
            GUILayout.Label("選択中の開拓対象: 未設定");
        }
        if (gameManager.DevelopmentSystem != null)
        {
            GUILayout.Label($"累積開拓値: {(int)(gameManager.DevelopmentSystem.AccumulatedSuccessRate * 100f)}% / 連続非開拓ターン {gameManager.DevelopmentSystem.ConsecutiveInactiveTurns} / 開拓担当ギルド員 {gameManager.DevelopmentSystem.LastAssignedDeveloperCount}");
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("食料-10", GUILayout.Width(80f)))
        {
            TryUpdateWorkers(() => gameManager.TrySetAssignedFoodWorkers(state.AssignedFoodWorkers - 10), "食料担当を減らしました。");
        }
        if (GUILayout.Button("食料+10", GUILayout.Width(80f)))
        {
            TryUpdateWorkers(() => gameManager.TrySetAssignedFoodWorkers(state.AssignedFoodWorkers + 10), "食料担当を増やしました。");
        }
        if (GUILayout.Button("資金-10", GUILayout.Width(80f)))
        {
            TryUpdateWorkers(() => gameManager.TrySetAssignedFundsWorkers(state.AssignedFundsWorkers - 10), "資金担当を減らしました。");
        }
        if (GUILayout.Button("資金+10", GUILayout.Width(80f)))
        {
            TryUpdateWorkers(() => gameManager.TrySetAssignedFundsWorkers(state.AssignedFundsWorkers + 10), "資金担当を増やしました。");
        }
        if (GUILayout.Button("開拓-10", GUILayout.Width(80f)))
        {
            TryUpdateWorkers(() => gameManager.TrySetAssignedDevelopmentWorkers(state.AssignedDevelopmentWorkers - 10), "開拓担当を減らしました。");
        }
        if (GUILayout.Button("開拓+10", GUILayout.Width(80f)))
        {
            TryUpdateWorkers(() => gameManager.TrySetAssignedDevelopmentWorkers(state.AssignedDevelopmentWorkers + 10), "開拓担当を増やしました。");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button($"100資金で食料交換 (+{gameManager.GetFoodExchangeGainFor100Funds()})", GUILayout.Width(260f)))
        {
            bool success = gameManager.TryExchangeFundsForFood(out string message);
            _latestMessage = message;
            if (success)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }
    }

    private void DrawWarningBox(string warningMessage)
    {
        if (!string.IsNullOrEmpty(warningMessage))
        {
            GUI.color = new Color(1f, 0.93f, 0.55f);
            GUILayout.Box("警告\n" + warningMessage, GUILayout.MinHeight(56f));
            GUI.color = Color.white;
            return;
        }

        GUI.color = new Color(0.78f, 0.95f, 0.78f);
        GUILayout.Box("警告\n待機中メンバーはいません。", GUILayout.MinHeight(56f));
        GUI.color = Color.white;
    }

    private void DrawLatestMessageBox()
    {
        if (string.IsNullOrEmpty(_latestMessage))
        {
            return;
        }

        GUI.color = new Color(0.8f, 0.9f, 1f);
        GUILayout.Box("最新メッセージ\n" + _latestMessage, GUILayout.MinHeight(56f));
        GUI.color = Color.white;
    }

    private void DrawGuildSection(GameManager gameManager)
    {
        GUILayout.Label("ギルド");
        int memberCount = CountMembers(gameManager.State);
        if (memberCount <= 0)
        {
            GUILayout.Label("利用可能なメンバーがいません。");
            return;
        }

        ClampSelectedMemberIndex(memberCount);

        if (TryGetSelectedMember(gameManager.State, _selectedMemberIndex, out GuildBase guild, out GuildMember member))
        {
            GUILayout.Label($"選択中: {guild.GuildName} / {member.Name} / Lv.{member.Level} / 行動 {member.CurrentAction}");
            GUILayout.Label($"戦闘 {member.CurrentCombatPower} / 技能 {member.CurrentSkillPower} / 経験値 {member.Experience}/{member.RequiredExperience}");
            GUILayout.Label($"対象: {FormatActionTarget(member)}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("前", GUILayout.Width(60f)))
            {
                _selectedMemberIndex = (_selectedMemberIndex - 1 + memberCount) % memberCount;
            }
            if (GUILayout.Button("次", GUILayout.Width(60f)))
            {
                _selectedMemberIndex = (_selectedMemberIndex + 1) % memberCount;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawActionButton(gameManager, guild, member, GuildAction.Idle, null, "待機");
            DrawActionButton(gameManager, guild, member, GuildAction.Defend, null, "防衛");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawActionButton(gameManager, guild, member, GuildAction.Explore, GameConstants.EXPLORATION_TARGET_RAID_ORIGIN, "襲撃元探索");
            DrawActionButton(gameManager, guild, member, GuildAction.Explore, GameConstants.EXPLORATION_TARGET_DUNGEON, "ダンジョン探索");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawActionButton(gameManager, guild, member, GuildAction.Develop, null, "開拓");
            GUILayout.EndHorizontal();

            DrawResearchTargetControls(gameManager, guild, member);
            DrawConstructTargetControls(gameManager, guild, member);
        }

        GUILayout.Space(6f);
        for (int i = 0; i < gameManager.State.Guilds.Count; i++)
        {
            GuildBase currentGuild = gameManager.State.Guilds[i];
            if (currentGuild == null)
            {
                continue;
            }

            GUILayout.Label($"{currentGuild.GuildName} {currentGuild.Members.Count}/{currentGuild.MaxMembers} {(currentGuild.IsUnlocked ? "解放済み" : "未解放")} / 戦力 {currentGuild.CalculateCombatPower()}");
            for (int j = 0; j < currentGuild.Members.Count; j++)
            {
                GuildMember currentMember = currentGuild.Members[j];
                if (currentMember == null)
                {
                    continue;
                }

                string suffix = currentMember.IsInDungeonRun ? " / ダンジョン中" : string.Empty;
                GUILayout.Label($"  - {currentMember.Name} / 行動 {currentMember.CurrentAction} / 対象 {FormatActionTarget(currentMember)}{suffix}");
            }
        }
    }

    private void DrawResearchTargetControls(GameManager gameManager, GuildBase guild, GuildMember member)
    {
        List<ResearchNodeData> nodes = new List<ResearchNodeData>(gameManager.ResearchTree.RegisteredNodes);
        if (nodes.Count == 0)
        {
            GUILayout.Label("研究対象: 研究ノード未登録");
            return;
        }

        if (_selectedResearchNodeIndex >= nodes.Count)
        {
            _selectedResearchNodeIndex = 0;
        }

        ResearchNodeData selectedNode = nodes[_selectedResearchNodeIndex];
        GUILayout.Label("研究対象");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("研究前", GUILayout.Width(60f)))
        {
            _selectedResearchNodeIndex = (_selectedResearchNodeIndex - 1 + nodes.Count) % nodes.Count;
        }
        if (GUILayout.Button("研究次", GUILayout.Width(60f)))
        {
            _selectedResearchNodeIndex = (_selectedResearchNodeIndex + 1) % nodes.Count;
        }
        if (GUILayout.Button("研究に設定", GUILayout.Width(120f)))
        {
            if (gameManager.TryAssignGuildAction(guild, member, GuildAction.Research, selectedNode.nodeId))
            {
                _latestMessage = $"{member.Name} の研究対象を {selectedNode.displayName} に設定しました。";
            }
            else
            {
                _latestMessage = $"{member.Name} の研究対象を設定できませんでした。";
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Label($"候補: {selectedNode.displayName} ({selectedNode.nodeId})");
    }

    private void DrawConstructTargetControls(GameManager gameManager, GuildBase guild, GuildMember member)
    {
        List<BuildingBase> buildings = new List<BuildingBase>();
        for (int i = 0; i < gameManager.State.Buildings.Count; i++)
        {
            BuildingBase building = gameManager.State.Buildings[i];
            if (building != null)
            {
                buildings.Add(building);
            }
        }

        if (buildings.Count == 0)
        {
            GUILayout.Label("建設対象: 建物なし");
            return;
        }

        if (_selectedConstructTargetIndex >= buildings.Count)
        {
            _selectedConstructTargetIndex = 0;
        }

        BuildingBase selectedBuilding = buildings[_selectedConstructTargetIndex];
        GUILayout.Label("建設対象");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("建設前", GUILayout.Width(60f)))
        {
            _selectedConstructTargetIndex = (_selectedConstructTargetIndex - 1 + buildings.Count) % buildings.Count;
        }
        if (GUILayout.Button("建設次", GUILayout.Width(60f)))
        {
            _selectedConstructTargetIndex = (_selectedConstructTargetIndex + 1) % buildings.Count;
        }
        if (GUILayout.Button("建設に設定", GUILayout.Width(120f)))
        {
            if (gameManager.TryAssignGuildAction(guild, member, GuildAction.Construct, selectedBuilding.Name))
            {
                _latestMessage = $"{member.Name} の建設対象を {selectedBuilding.Name} に設定しました。";
            }
            else
            {
                _latestMessage = $"{member.Name} の建設対象を設定できませんでした。";
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Label($"候補: {selectedBuilding.Name} Lv.{selectedBuilding.Level}/{selectedBuilding.MaxLevel}");
    }

    private void DrawMapSection(GameManager gameManager)
    {
        MapData mapData = gameManager.CurrentMapData;
        if (mapData == null)
        {
            GUILayout.Label("マップ: 未生成");
            return;
        }

        GUILayout.Label("マップ");
        GUILayout.TextArea(BuildMapText(mapData, gameManager.DevelopmentSystem), GUILayout.Height(220f));
        GUILayout.Label($"凡例: H 中心地 / * 選択中開拓 / o 開拓地 / . 平地 / # 岩場 / ~ 川 / D ダンジョン入口 / R 襲撃元 / 開拓済み {mapData.DevelopedTileCount}マス");
    }

    private void DrawExplorationSection(GameManager gameManager)
    {
        GameState state = gameManager.State;
        GUILayout.Label("探索状況");

        if (GUILayout.Button("ダンジョン突入を試行", GUILayout.Width(180f)))
        {
            GuildMember explorer = gameManager.FindFirstMemberByAction(GuildAction.Explore);
            bool success = gameManager.TryStartDungeonExploration(explorer, out string reason);
            _latestMessage = success ? "ダンジョン開始: " + reason : "ダンジョン開始失敗: " + reason;
            Debug.Log(_latestMessage);
        }

        GUILayout.Label($"襲撃元: {(state.IsInitialRaidOriginExplored ? "発見済み" : "未発見")} / 進行度 {state.InitialRaidOriginExplorationProgress}%");
        if (gameManager.DungeonSystem != null)
        {
            for (int i = 0; i < gameManager.DungeonSystem.ActiveRuns.Count; i++)
            {
                DungeonRun run = gameManager.DungeonSystem.ActiveRuns[i];
                if (run == null || run.Member == null)
                {
                    continue;
                }

                GUILayout.Label($"探索中: {run.Member.Name} / フロア {run.CurrentFloor} / 残り {run.RemainingTurns}/{GameConstants.DUNGEON_TURNS_PER_FLOOR}");
            }
        }
    }

    private void DrawActionButton(GameManager gameManager, GuildBase guild, GuildMember member, GuildAction action, string targetId, string label)
    {
        if (GUILayout.Button(label, GUILayout.Width(110f)))
        {
            if (gameManager.TryAssignGuildAction(guild, member, action, targetId))
            {
                _latestMessage = $"{member.Name} の行動を {label} に変更しました。";
            }
            else
            {
                _latestMessage = $"{member.Name} の行動を {label} に変更できませんでした。";
            }
        }
    }

    private void TryUpdateWorkers(System.Func<bool> updateFunc, string successMessage)
    {
        _latestMessage = updateFunc() ? successMessage : "人口配分を変更できませんでした。";
    }

    private string BuildTurnResultMessage(GameManager gameManager)
    {
        string warning = gameManager.LastTurnWarningMessage;
        string development = gameManager.DevelopmentSystem != null ? gameManager.DevelopmentSystem.LastResultMessage : string.Empty;

        if (!string.IsNullOrEmpty(warning) && !string.IsNullOrEmpty(development))
        {
            return warning + "\n" + development;
        }

        if (!string.IsNullOrEmpty(warning))
        {
            return warning;
        }

        if (!string.IsNullOrEmpty(development))
        {
            return development;
        }

        return "ターンを進めました。";
    }

    private void ClampSelectedMemberIndex(int memberCount)
    {
        if (_selectedMemberIndex >= memberCount)
        {
            _selectedMemberIndex = memberCount - 1;
        }

        if (_selectedMemberIndex < 0)
        {
            _selectedMemberIndex = 0;
        }
    }

    private void ClampDevelopmentCandidateIndex(int candidateCount)
    {
        if (_selectedDevelopmentCandidateIndex >= candidateCount)
        {
            _selectedDevelopmentCandidateIndex = candidateCount - 1;
        }

        if (_selectedDevelopmentCandidateIndex < 0)
        {
            _selectedDevelopmentCandidateIndex = 0;
        }
    }

    private static string FormatActionTarget(GuildMember member)
    {
        if (member == null || string.IsNullOrEmpty(member.CurrentActionTargetId))
        {
            return "未設定";
        }

        if (member.CurrentActionTargetId == GameConstants.EXPLORATION_TARGET_RAID_ORIGIN)
        {
            return "襲撃元";
        }

        if (member.CurrentActionTargetId == GameConstants.EXPLORATION_TARGET_DUNGEON)
        {
            return "ダンジョン";
        }

        return member.CurrentActionTargetId;
    }

    private static int CountMembers(GameState state)
    {
        int count = 0;
        if (state == null)
        {
            return count;
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
                if (guild.Members[j] != null)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool TryGetSelectedMember(GameState state, int selectedIndex, out GuildBase selectedGuild, out GuildMember selectedMember)
    {
        selectedGuild = null;
        selectedMember = null;

        if (state == null)
        {
            return false;
        }

        int currentIndex = 0;
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
                if (member == null)
                {
                    continue;
                }

                if (currentIndex == selectedIndex)
                {
                    selectedGuild = guild;
                    selectedMember = member;
                    return true;
                }

                currentIndex++;
            }
        }

        return false;
    }

    private static List<Vector2Int> CollectDevelopmentCandidates(MapData mapData)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        if (mapData == null)
        {
            return candidates;
        }

        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                if (mapData.IsValidDevelopmentTarget(x, y))
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        candidates.Sort((a, b) =>
        {
            int distanceCompare = mapData.GetDistanceFromHome(a.x, a.y).CompareTo(mapData.GetDistanceFromHome(b.x, b.y));
            if (distanceCompare != 0)
            {
                return distanceCompare;
            }

            int yCompare = a.y.CompareTo(b.y);
            return yCompare != 0 ? yCompare : a.x.CompareTo(b.x);
        });

        return candidates;
    }

    private static string BuildMapText(MapData mapData, DevelopmentSystem developmentSystem)
    {
        StringBuilder builder = new StringBuilder();
        for (int y = mapData.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                builder.Append(ToMapChar(mapData, developmentSystem, x, y));
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static char ToMapChar(MapData mapData, DevelopmentSystem developmentSystem, int x, int y)
    {
        if (developmentSystem != null
            && developmentSystem.HasSelectedTarget
            && developmentSystem.SelectedTargetX == x
            && developmentSystem.SelectedTargetY == y)
        {
            return '*';
        }

        if (x == mapData.HomeX && y == mapData.HomeY)
        {
            return 'H';
        }

        if (mapData.IsDeveloped(x, y))
        {
            return 'o';
        }

        MapTileType tileType = mapData.GetTile(x, y);
        if (tileType == MapTileType.Rock)
        {
            return '#';
        }

        if (tileType == MapTileType.River)
        {
            return '~';
        }

        if (tileType == MapTileType.DungeonEntrance)
        {
            return 'D';
        }

        if (tileType == MapTileType.InitialRaidOrigin)
        {
            return 'R';
        }

        return '.';
    }
}
