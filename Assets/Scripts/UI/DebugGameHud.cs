using System.Collections.Generic;
using UnityEngine;

public class DebugGameHud : MonoBehaviour
{
    [SerializeField] private GameBootstrap bootstrap;

    private Vector2 _scroll;
    private int _selectedMemberIndex;
    private string _latestMessage;
    private int _selectedResearchNodeIndex;
    private int _selectedConstructTargetIndex;

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
            GUI.Box(new Rect(20f, 20f, 320f, 60f), "GameBootstrap / GameManager が見つかりません。");
            return;
        }

        GameState state = gameManager.State;
        GUILayout.BeginArea(new Rect(16f, 16f, 700f, Screen.height - 32f), GUI.skin.box);
        _scroll = GUILayout.BeginScrollView(_scroll);

        GUILayout.Label($"ターン: {state.CurrentTurn} / {GameConstants.MAX_TURNS}");
        GUILayout.Label(state.IsGameOver ? $"状態: {(state.IsVictory ? "勝利" : "敗北")} / {state.GameEndReason}" : "状態: 進行中");
        GUILayout.Space(8f);

        GUILayout.Label($"食料 {state.Food} / 資金 {state.Funds} / 人口 {state.Population}/{state.PopulationCapacity} / 幸福度 {state.Happiness}");
        GUILayout.Label($"襲撃元探索: {state.InitialRaidOriginExplorationProgress}% / ダンジョン: {(state.IsDungeonExplorationUnlocked ? "解放済み" : "未解放")} / 発見: {(state.IsInitialRaidOriginExplored ? "済" : "未")}");
        GUILayout.Label($"進行中ダンジョン: {(gameManager.DungeonSystem != null ? gameManager.DungeonSystem.ActiveRuns.Count : 0)}");
        GUILayout.Space(4f);

        DrawWarningBox(gameManager.BuildIdleWarningMessage());
        DrawLatestMessageBox();
        GUILayout.Space(12f);

        DrawMapSection();
        GUILayout.Space(12f);

        DrawGuildSection(gameManager);
        GUILayout.Space(12f);
        DrawExplorationSection(gameManager);
        GUILayout.Space(12f);

        if (GUILayout.Button("ターン終了", GUILayout.Height(32f)))
        {
            gameManager.AdvanceTurn();
            _latestMessage = string.IsNullOrEmpty(gameManager.LastTurnWarningMessage)
                ? "ターンを進行しました。"
                : gameManager.LastTurnWarningMessage;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
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
            GUILayout.Label("所属メンバーがいません。");
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
        }
        GUILayout.EndHorizontal();
        GUILayout.Label($"候補: {selectedBuilding.Name} Lv.{selectedBuilding.Level}/{selectedBuilding.MaxLevel}");
    }

    private void DrawMapSection()
    {
        MapData mapData = bootstrap != null ? bootstrap.CurrentMapData : null;
        if (mapData == null)
        {
            GUILayout.Label("マップ: 未生成");
            return;
        }

        GUILayout.Label("マップ");
        GUILayout.TextArea(BuildMapText(mapData), GUILayout.Height(220f));
        GUILayout.Label("凡例: . 平地 / # 岩場 / ~ 川 / D ダンジョン入口 / R 襲撃元");
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

        GUILayout.Label($"襲撃元発見: {(state.IsInitialRaidOriginExplored ? "済" : "未")} / 進行 {state.InitialRaidOriginExplorationProgress}%");
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
        }
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

    private static string BuildMapText(MapData mapData)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int y = mapData.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                builder.Append(ToMapChar(mapData.GetTile(x, y)));
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static char ToMapChar(MapTileType tileType)
    {
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
