public class GuildManager
{
    public int CountAssignedResearchWorkers(GameState state)
    {
        return CountAssignedWorkers(state, GuildAction.Research);
    }

    public int CountAssignedDevelopmentWorkers(GameState state)
    {
        return CountAssignedWorkers(state, GuildAction.Develop);
    }

    public int CountAssignedWorkers(GameState state, GuildAction action)
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
                GuildMember member = guild.Members[j];
                if (member != null && member.CurrentAction == action)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public void ResolveActions(GameState state)
    {
        if (state == null)
        {
            return;
        }

        for (int i = 0; i < state.Guilds.Count; i++)
        {
            GuildBase guild = state.Guilds[i];
            if (guild != null)
            {
                guild.ResolveActions(state);
            }
        }
    }
}
