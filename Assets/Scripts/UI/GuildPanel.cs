using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable 0649
public class GuildPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text guildSummaryText;
    [SerializeField] private TMP_Text selectedGuildText;
    [SerializeField] private TMP_Text memberListText;
    [SerializeField] private TMP_Text selectedMemberText;
    [SerializeField] private TMP_Text actionResultText;
    [SerializeField] private Button previousGuildButton;
    [SerializeField] private Button nextGuildButton;
    [SerializeField] private Button hireButton;
    [SerializeField] private Button previousMemberButton;
    [SerializeField] private Button nextMemberButton;
    [SerializeField] private Button idleButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button exploreButton;
    [SerializeField] private Button researchButton;
    [SerializeField] private Button constructButton;
    [SerializeField] private Button developButton;

    private GameManager _gameManager;
    private GameState _state;
    private int _selectedGuildIndex;
    private int _selectedMemberIndex;

    private void Awake()
    {
        AddListener(previousGuildButton, OnPreviousGuildClicked);
        AddListener(nextGuildButton, OnNextGuildClicked);
        AddListener(hireButton, OnHireClicked);
        AddListener(previousMemberButton, OnPreviousMemberClicked);
        AddListener(nextMemberButton, OnNextMemberClicked);
        AddListener(idleButton, OnIdleClicked);
        AddListener(defendButton, OnDefendClicked);
        AddListener(exploreButton, OnExploreClicked);
        AddListener(researchButton, OnResearchClicked);
        AddListener(constructButton, OnConstructClicked);
        AddListener(developButton, OnDevelopClicked);
    }

    private void OnDestroy()
    {
        RemoveListener(previousGuildButton, OnPreviousGuildClicked);
        RemoveListener(nextGuildButton, OnNextGuildClicked);
        RemoveListener(hireButton, OnHireClicked);
        RemoveListener(previousMemberButton, OnPreviousMemberClicked);
        RemoveListener(nextMemberButton, OnNextMemberClicked);
        RemoveListener(idleButton, OnIdleClicked);
        RemoveListener(defendButton, OnDefendClicked);
        RemoveListener(exploreButton, OnExploreClicked);
        RemoveListener(researchButton, OnResearchClicked);
        RemoveListener(constructButton, OnConstructClicked);
        RemoveListener(developButton, OnDevelopClicked);
    }

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;
        _state = _gameManager != null ? _gameManager.State : null;
        _selectedGuildIndex = 0;
        _selectedMemberIndex = 0;
        SetText(actionResultText, string.Empty);
        Refresh();
    }

    public void Bind(GameState state)
    {
        _gameManager = null;
        _state = state;
        Refresh();
    }

    public void Refresh(GameState state)
    {
        _state = state;
        Refresh();
    }

    public void Refresh()
    {
        if (_gameManager != null)
        {
            _state = _gameManager.State;
        }

        if (_state == null)
        {
            SetText(guildSummaryText, "ギルド: 未接続");
            SetText(selectedGuildText, "選択中ギルド: なし");
            SetText(memberListText, string.Empty);
            SetText(selectedMemberText, "選択中メンバー: なし");
            return;
        }

        ClampGuildSelection();
        ClampMemberSelection();
        SetText(guildSummaryText, BuildGuildSummaryText(_state, _selectedGuildIndex));
        SetText(selectedGuildText, BuildSelectedGuildText());
        SetText(memberListText, BuildMemberListText());
        SetText(selectedMemberText, BuildSelectedMemberText());
    }

    public void AssignAction(GuildBase guild, GuildMember member, GuildAction action)
    {
        if (guild == null || member == null)
        {
            return;
        }

        guild.AssignAction(member, action);
        Refresh();
    }

    public void OnPreviousGuildClicked()
    {
        int count = CountGuilds(_state);
        if (count <= 0)
        {
            return;
        }

        _selectedGuildIndex = (_selectedGuildIndex - 1 + count) % count;
        _selectedMemberIndex = 0;
        SetText(actionResultText, string.Empty);
        Refresh();
    }

    public void OnNextGuildClicked()
    {
        int count = CountGuilds(_state);
        if (count <= 0)
        {
            return;
        }

        _selectedGuildIndex = (_selectedGuildIndex + 1) % count;
        _selectedMemberIndex = 0;
        SetText(actionResultText, string.Empty);
        Refresh();
    }

    public void OnHireClicked()
    {
        if (_gameManager == null)
        {
            SetText(actionResultText, "加入処理を開始できません。");
            return;
        }

        GuildBase guild = GetSelectedGuild();
        if (guild == null)
        {
            SetText(actionResultText, "選択中のギルドがありません。");
            return;
        }

        _gameManager.TryHireGuildMember(guild, out string message);
        SetText(actionResultText, message);
        Refresh();
    }

    public void OnPreviousMemberClicked()
    {
        GuildBase guild = GetSelectedGuild();
        int count = guild != null ? CountMembers(guild) : 0;
        if (count <= 0)
        {
            return;
        }

        _selectedMemberIndex = (_selectedMemberIndex - 1 + count) % count;
        SetText(actionResultText, string.Empty);
        Refresh();
    }

    public void OnNextMemberClicked()
    {
        GuildBase guild = GetSelectedGuild();
        int count = guild != null ? CountMembers(guild) : 0;
        if (count <= 0)
        {
            return;
        }

        _selectedMemberIndex = (_selectedMemberIndex + 1) % count;
        SetText(actionResultText, string.Empty);
        Refresh();
    }

    public void OnIdleClicked()
    {
        AssignSelectedAction(GuildAction.Idle);
    }

    public void OnDefendClicked()
    {
        AssignSelectedAction(GuildAction.Defend);
    }

    public void OnExploreClicked()
    {
        AssignSelectedAction(GuildAction.Explore);
    }

    public void OnResearchClicked()
    {
        AssignSelectedAction(GuildAction.Research);
    }

    public void OnConstructClicked()
    {
        AssignSelectedAction(GuildAction.Construct);
    }

    public void OnDevelopClicked()
    {
        AssignSelectedAction(GuildAction.Develop);
    }

    private void AssignSelectedAction(GuildAction action)
    {
        GuildBase guild = GetSelectedGuild();
        GuildMember member = GetSelectedMember(guild);
        if (guild == null || member == null)
        {
            SetText(actionResultText, "操作対象のメンバーがいません。");
            Refresh();
            return;
        }

        bool assigned = _gameManager != null
            ? _gameManager.TryAssignGuildAction(guild, member, action)
            : TryAssignWithoutGameManager(guild, member, action);

        SetText(actionResultText, assigned
            ? $"{member.Name} を {action} に設定しました。"
            : $"{member.Name} を {action} に設定できませんでした。");
        Refresh();
    }

    private bool TryAssignWithoutGameManager(GuildBase guild, GuildMember member, GuildAction action)
    {
        if (guild == null || member == null)
        {
            return false;
        }

        guild.AssignAction(member, action);
        return member.CurrentAction == action;
    }

    private string BuildSelectedGuildText()
    {
        GuildBase guild = GetSelectedGuild();
        if (guild == null)
        {
            return "選択中ギルド: なし";
        }

        int hireCost = _gameManager != null ? _gameManager.GetGuildHireCost(guild) : (guild.Data != null ? guild.Data.hireCostFunds : 0);
        return $"選択中ギルド: {guild.GuildName} / {guild.Members.Count}/{guild.MaxMembers} / 加入費 {hireCost} / {(guild.IsUnlocked ? "解放済み" : "未解放")}";
    }

    private string BuildSelectedMemberText()
    {
        GuildBase guild = GetSelectedGuild();
        GuildMember member = GetSelectedMember(guild);
        if (guild == null || member == null)
        {
            return "選択中メンバー: なし";
        }

        return $"選択中メンバー: {guild.GuildName} / {member.Name} / Lv.{member.Level} / 行動 {member.CurrentAction}";
    }

    private GuildBase GetSelectedGuild()
    {
        if (_state == null)
        {
            return null;
        }

        int currentIndex = 0;
        for (int i = 0; i < _state.Guilds.Count; i++)
        {
            GuildBase guild = _state.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            if (currentIndex == _selectedGuildIndex)
            {
                return guild;
            }

            currentIndex++;
        }

        return null;
    }

    private GuildMember GetSelectedMember(GuildBase guild)
    {
        if (guild == null)
        {
            return null;
        }

        int currentIndex = 0;
        for (int i = 0; i < guild.Members.Count; i++)
        {
            GuildMember member = guild.Members[i];
            if (member == null)
            {
                continue;
            }

            if (currentIndex == _selectedMemberIndex)
            {
                return member;
            }

            currentIndex++;
        }

        return null;
    }

    private void ClampGuildSelection()
    {
        int count = CountGuilds(_state);
        if (count <= 0)
        {
            _selectedGuildIndex = 0;
            return;
        }

        if (_selectedGuildIndex < 0)
        {
            _selectedGuildIndex = 0;
        }
        else if (_selectedGuildIndex >= count)
        {
            _selectedGuildIndex = count - 1;
        }
    }

    private void ClampMemberSelection()
    {
        GuildBase guild = GetSelectedGuild();
        int count = guild != null ? CountMembers(guild) : 0;
        if (count <= 0)
        {
            _selectedMemberIndex = 0;
            return;
        }

        if (_selectedMemberIndex < 0)
        {
            _selectedMemberIndex = 0;
        }
        else if (_selectedMemberIndex >= count)
        {
            _selectedMemberIndex = count - 1;
        }
    }

    private static string BuildGuildSummaryText(GameState state, int selectedGuildIndex)
    {
        if (state.Guilds.Count == 0)
        {
            return "ギルド: なし";
        }

        StringBuilder builder = new StringBuilder("ギルド一覧\n");
        int visibleIndex = 0;
        for (int i = 0; i < state.Guilds.Count; i++)
        {
            GuildBase guild = state.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            string prefix = visibleIndex == selectedGuildIndex ? "> " : "  ";
            string unlocked = guild.IsUnlocked ? "解放済み" : "未解放";
            builder.Append(prefix)
                .Append(guild.GuildName)
                .Append(" ")
                .Append(guild.Members.Count)
                .Append("/")
                .Append(guild.MaxMembers)
                .Append(" [")
                .Append(unlocked)
                .Append("] 戦力 ")
                .Append(guild.CalculateCombatPower())
                .AppendLine();
            visibleIndex++;
        }

        return builder.ToString();
    }

    private string BuildMemberListText()
    {
        GuildBase guild = GetSelectedGuild();
        if (guild == null)
        {
            return "メンバー一覧\nなし";
        }

        if (CountMembers(guild) == 0)
        {
            return "メンバー一覧\n加入済みメンバーなし";
        }

        StringBuilder builder = new StringBuilder("メンバー一覧\n");
        int visibleIndex = 0;
        for (int i = 0; i < guild.Members.Count; i++)
        {
            GuildMember member = guild.Members[i];
            if (member == null)
            {
                continue;
            }

            string prefix = visibleIndex == _selectedMemberIndex ? "> " : "  ";
            builder.Append(prefix)
                .Append(member.Name)
                .Append(" Lv.")
                .Append(member.Level)
                .Append(" 経験値 ")
                .Append(member.Experience)
                .Append("/")
                .Append(member.RequiredExperience)
                .Append(" 戦闘 ")
                .Append(member.CurrentCombatPower)
                .Append(" 技能 ")
                .Append(member.CurrentSkillPower)
                .Append(" 行動 ")
                .Append(member.CurrentAction)
                .AppendLine();
            visibleIndex++;
        }

        return builder.ToString();
    }

    private static int CountGuilds(GameState state)
    {
        if (state == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < state.Guilds.Count; i++)
        {
            if (state.Guilds[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountMembers(GuildBase guild)
    {
        if (guild == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < guild.Members.Count; i++)
        {
            if (guild.Members[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    private static void AddListener(Button button, UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveListener(Button button, UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
#pragma warning restore 0649
