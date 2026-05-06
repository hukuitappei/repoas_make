using System.Text;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class GuildPanel : MonoBehaviour
{
    [SerializeField] private Text guildSummaryText;
    [SerializeField] private Text memberListText;

    private GameState _state;

    public void Bind(GameState state)
    {
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
        if (_state == null)
        {
            SetText(guildSummaryText, "ギルド: 未接続");
            SetText(memberListText, string.Empty);
            return;
        }

        SetText(guildSummaryText, BuildGuildSummaryText(_state));
        SetText(memberListText, BuildMemberListText(_state));
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

    private static string BuildGuildSummaryText(GameState state)
    {
        if (state.Guilds.Count == 0)
        {
            return "ギルド: なし";
        }

        StringBuilder builder = new StringBuilder("ギルド:\n");
        for (int i = 0; i < state.Guilds.Count; i++)
        {
            GuildBase guild = state.Guilds[i];
            if (guild == null)
            {
                continue;
            }

            string unlocked = guild.IsUnlocked ? "解放済み" : "未解放";
            builder.AppendLine($"{guild.GuildName} {guild.Members.Count}/{guild.MaxMembers} [{unlocked}] 戦闘力{guild.CalculateCombatPower()}");
        }

        return builder.ToString();
    }

    private static string BuildMemberListText(GameState state)
    {
        StringBuilder builder = new StringBuilder("ギルド員:\n");
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

                builder.AppendLine($"{member.Name} Lv.{member.Level} Exp.{member.Experience}/{member.RequiredExperience} 戦力{member.CombatPower} 技能{member.SkillPower} 行動:{member.CurrentAction}");
            }
        }

        return builder.ToString();
    }

    private static void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
#pragma warning restore 0649
