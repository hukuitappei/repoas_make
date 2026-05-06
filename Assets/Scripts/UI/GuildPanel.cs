using System.Text;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

#pragma warning disable 0649
public class GuildPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text guildSummaryText;
    [SerializeField] private TMP_Text memberListText;
    [SerializeField] private TMP_Text selectedMemberText;
    [SerializeField] private TMP_Text actionResultText;
    [SerializeField] private Button previousMemberButton;
    [SerializeField] private Button nextMemberButton;
    [SerializeField] private Button idleButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button exploreButton;
    [SerializeField] private Button researchButton;
    [SerializeField] private Button constructButton;

    private GameManager _gameManager;
    private GameState _state;
    private int _selectedMemberIndex;

    private void Awake()
    {
        AddListener(previousMemberButton, OnPreviousMemberClicked);
        AddListener(nextMemberButton, OnNextMemberClicked);
        AddListener(idleButton, OnIdleClicked);
        AddListener(defendButton, OnDefendClicked);
        AddListener(exploreButton, OnExploreClicked);
        AddListener(researchButton, OnResearchClicked);
        AddListener(constructButton, OnConstructClicked);
    }

    private void OnDestroy()
    {
        RemoveListener(previousMemberButton, OnPreviousMemberClicked);
        RemoveListener(nextMemberButton, OnNextMemberClicked);
        RemoveListener(idleButton, OnIdleClicked);
        RemoveListener(defendButton, OnDefendClicked);
        RemoveListener(exploreButton, OnExploreClicked);
        RemoveListener(researchButton, OnResearchClicked);
        RemoveListener(constructButton, OnConstructClicked);
    }

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;
        _state = _gameManager != null ? _gameManager.State : null;
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
            SetText(memberListText, string.Empty);
            SetText(selectedMemberText, "選択中: なし");
            return;
        }

        ClampSelection();
        SetText(guildSummaryText, BuildGuildSummaryText(_state));
        SetText(memberListText, BuildMemberListText(_state, _selectedMemberIndex));
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

    public void OnPreviousMemberClicked()
    {
        int count = CountMembers(_state);
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
        int count = CountMembers(_state);
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

    private void AssignSelectedAction(GuildAction action)
    {
        if (!TryGetSelectedMember(out GuildBase guild, out GuildMember member))
        {
            SetText(actionResultText, "メンバーがいません。");
            Refresh();
            return;
        }

        bool assigned = _gameManager != null
            ? _gameManager.TryAssignGuildAction(guild, member, action)
            : TryAssignWithoutGameManager(guild, member, action);

        SetText(actionResultText, assigned
            ? member.Name + " → " + action
            : member.Name + " に " + action + " を割り当てられません。");
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

    private string BuildSelectedMemberText()
    {
        if (!TryGetSelectedMember(out GuildBase guild, out GuildMember member))
        {
            return "選択中: なし";
        }

        return "選択中: " + guild.GuildName + " / " + member.Name + " (" + member.CurrentAction + ")";
    }

    private bool TryGetSelectedMember(out GuildBase guild, out GuildMember member)
    {
        guild = null;
        member = null;

        if (_state == null)
        {
            return false;
        }

        int currentIndex = 0;
        for (int i = 0; i < _state.Guilds.Count; i++)
        {
            GuildBase currentGuild = _state.Guilds[i];
            if (currentGuild == null)
            {
                continue;
            }

            for (int j = 0; j < currentGuild.Members.Count; j++)
            {
                GuildMember currentMember = currentGuild.Members[j];
                if (currentMember == null)
                {
                    continue;
                }

                if (currentIndex == _selectedMemberIndex)
                {
                    guild = currentGuild;
                    member = currentMember;
                    return true;
                }

                currentIndex++;
            }
        }

        return false;
    }

    private void ClampSelection()
    {
        int count = CountMembers(_state);
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

    private static string BuildGuildSummaryText(GameState state)
    {
        if (state.Guilds.Count == 0)
        {
            return "ギルド: なし";
        }

        StringBuilder builder = new StringBuilder("ギルド一覧\n");
        for (int i = 0; i < state.Guilds.Count; i++)
        {
            GuildBase guild = state.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            string unlocked = guild.IsUnlocked ? "開放済" : "未開放";
            builder.AppendLine($"{guild.GuildName} {guild.Members.Count}/{guild.MaxMembers} [{unlocked}] 戦力 {guild.CalculateCombatPower()}");
        }

        return builder.ToString();
    }

    private static string BuildMemberListText(GameState state, int selectedMemberIndex)
    {
        StringBuilder builder = new StringBuilder("メンバー一覧\n");
        int currentIndex = 0;

        for (int i = 0; i < state.Guilds.Count; i++)
        {
            GuildBase guild = state.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            for (int memberIndex = 0; memberIndex < guild.Members.Count; memberIndex++)
            {
                GuildMember member = guild.Members[memberIndex];
                if (member == null)
                {
                    continue;
                }

                string prefix = currentIndex == selectedMemberIndex ? "> " : "  ";
                builder.AppendLine($"{prefix}{guild.GuildName} / {member.Name} Lv.{member.Level} 経験値{member.Experience}/{member.RequiredExperience} 戦闘力{member.CurrentCombatPower} 技術力{member.CurrentSkillPower} 行動{member.CurrentAction}");
                currentIndex++;
            }
        }

        return builder.ToString();
    }

    private static int CountMembers(GameState state)
    {
        if (state == null)
        {
            return 0;
        }

        int count = 0;
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
