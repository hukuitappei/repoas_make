using UnityEngine;

public class DebugGameHud : MonoBehaviour
{
    [SerializeField] private GameBootstrap bootstrap;

    private Vector2 _scroll;
    private int _selectedMemberIndex;

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
        GUILayout.BeginArea(new Rect(16f, 16f, 580f, Screen.height - 32f), GUI.skin.box);
        _scroll = GUILayout.BeginScrollView(_scroll);

        GUILayout.Label($"ターン: {state.CurrentTurn} / {GameConstants.MAX_TURNS}");
        GUILayout.Label(state.IsGameOver ? $"状態: {(state.IsVictory ? "勝利" : "敗北")} / {state.GameEndReason}" : "状態: 進行中");
        GUILayout.Space(8f);

        GUILayout.Label($"食料 {state.Food} / 資金 {state.Funds} / 人口 {state.Population}/{state.PopulationCapacity} / 幸福度 {state.Happiness}");
        GUILayout.Label($"襲撃元探索: {state.InitialRaidOriginExplorationProgress}% / ダンジョン: {(state.IsDungeonExplorationUnlocked ? "解放済み" : "未解放")}");
        GUILayout.Label($"進行中ダンジョン: {(gameManager.DungeonSystem != null ? gameManager.DungeonSystem.ActiveRuns.Count : 0)}");
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
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
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

        if (_selectedMemberIndex >= memberCount)
        {
            _selectedMemberIndex = memberCount - 1;
        }

        if (_selectedMemberIndex < 0)
        {
            _selectedMemberIndex = 0;
        }

        if (TryGetSelectedMember(gameManager.State, _selectedMemberIndex, out GuildBase guild, out GuildMember member))
        {
            GUILayout.Label($"選択中: {guild.GuildName} / {member.Name} / Lv.{member.Level} / 行動 {member.CurrentAction}");
            GUILayout.Label($"戦闘 {member.CurrentCombatPower} / 技能 {member.CurrentSkillPower} / 経験値 {member.Experience}/{member.RequiredExperience}");
        }

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
        DrawActionButton(gameManager, guild, member, GuildAction.Idle, "待機");
        DrawActionButton(gameManager, guild, member, GuildAction.Defend, "防衛");
        DrawActionButton(gameManager, guild, member, GuildAction.Explore, "探索");
        DrawActionButton(gameManager, guild, member, GuildAction.Research, "研究");
        DrawActionButton(gameManager, guild, member, GuildAction.Construct, "建設");
        GUILayout.EndHorizontal();

        GUILayout.Space(6f);
        for (int i = 0; i < gameManager.State.Guilds.Count; i++)
        {
            GuildBase currentGuild = gameManager.State.Guilds[i];
            if (currentGuild == null)
            {
                continue;
            }

            GUILayout.Label($"{currentGuild.GuildName} {currentGuild.Members.Count}/{currentGuild.MaxMembers} {(currentGuild.IsUnlocked ? "解放済み" : "未解放")} / 戦力 {currentGuild.CalculateCombatPower()}");
        }
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
        GUILayout.Label("探索");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("襲撃元を探索", GUILayout.Width(140f)))
        {
            bool success = gameManager.RaidSystem != null && gameManager.RaidSystem.ExploreInitialRaidOrigin(state);
            Debug.Log(success ? "襲撃元探索に成功しました。" : "襲撃元探索に失敗しました。");
        }

        if (GUILayout.Button("ダンジョン開始", GUILayout.Width(140f)))
        {
            GuildMember explorer = gameManager.FindFirstMemberByAction(GuildAction.Explore);
            bool success = gameManager.TryStartDungeonExploration(explorer, out string reason);
            Debug.Log(success ? "ダンジョン開始: " + reason : "ダンジョン開始失敗: " + reason);
        }
        GUILayout.EndHorizontal();

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

    private void DrawActionButton(GameManager gameManager, GuildBase guild, GuildMember member, GuildAction action, string label)
    {
        if (GUILayout.Button(label, GUILayout.Width(72f)))
        {
            gameManager.TryAssignGuildAction(guild, member, action);
        }
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
