using System.Collections.Generic;

public class ResearchTree
{
    private readonly Dictionary<string, ResearchNodeData> _nodes;
    private readonly List<ResearchProgress> _activeResearch;

    public IReadOnlyList<ResearchProgress> ActiveResearch => _activeResearch;
    public IEnumerable<ResearchNodeData> RegisteredNodes => _nodes.Values;

    public ResearchTree()
    {
        _nodes = new Dictionary<string, ResearchNodeData>();
        _activeResearch = new List<ResearchProgress>();
    }

    public void RegisterNode(ResearchNodeData nodeData)
    {
        if (nodeData == null || string.IsNullOrEmpty(nodeData.nodeId))
        {
            return;
        }

        _nodes[nodeData.nodeId] = nodeData;
    }

    public bool CanStartResearch(GameState state, string nodeId, int assignedResearchWorkers)
    {
        if (state == null || !_nodes.TryGetValue(nodeId, out ResearchNodeData nodeData))
        {
            return false;
        }

        if (state.IsResearchNodeCompleted(nodeId) || IsResearchActive(nodeId))
        {
            return false;
        }

        if (_activeResearch.Count >= assignedResearchWorkers)
        {
            return false;
        }

        if (assignedResearchWorkers < nodeData.requiredWorkers)
        {
            return false;
        }

        if (state.Funds < nodeData.researchCostFunds || state.Food < nodeData.requiredFood)
        {
            return false;
        }

        if (!state.HasMaterials(nodeData.materialRequirements))
        {
            return false;
        }

        return ArePrerequisitesCompleted(state, nodeData);
    }

    public bool StartResearch(GameState state, string nodeId, int assignedResearchWorkers)
    {
        if (!CanStartResearch(state, nodeId, assignedResearchWorkers))
        {
            return false;
        }

        ResearchNodeData nodeData = _nodes[nodeId];
        state.TrySpendFunds(nodeData.researchCostFunds);
        state.TrySpendFood(nodeData.requiredFood);
        state.TryConsumeMaterials(nodeData.materialRequirements);
        _activeResearch.Add(new ResearchProgress(nodeData));
        return true;
    }

    public void AdvanceResearch(GameState state)
    {
        if (state == null)
        {
            return;
        }

        int turnResearchBonusPercent = state.ConsumeTurnResearchSpeedBonusPercent();
        for (int i = _activeResearch.Count - 1; i >= 0; i--)
        {
            ResearchProgress progress = _activeResearch[i];
            progress.Advance(state.ResearchSpeedPercentBonus + turnResearchBonusPercent);
            if (!progress.IsCompleted())
            {
                continue;
            }

            CompleteResearch(state, progress.NodeData);
            _activeResearch.RemoveAt(i);
        }
    }

    public void StartAssignedResearch(GameState state)
    {
        if (state == null)
        {
            return;
        }

        Dictionary<string, int> assignedWorkersByNodeId = new Dictionary<string, int>();
        int totalAssignedResearchWorkers = 0;
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
                if (member == null || member.CurrentAction != GuildAction.Research || string.IsNullOrEmpty(member.CurrentActionTargetId))
                {
                    continue;
                }

                totalAssignedResearchWorkers++;
                if (!assignedWorkersByNodeId.ContainsKey(member.CurrentActionTargetId))
                {
                    assignedWorkersByNodeId[member.CurrentActionTargetId] = 0;
                }

                assignedWorkersByNodeId[member.CurrentActionTargetId]++;
            }
        }

        foreach (KeyValuePair<string, int> pair in assignedWorkersByNodeId)
        {
            if (!_nodes.TryGetValue(pair.Key, out ResearchNodeData nodeData))
            {
                continue;
            }

            if (pair.Value < nodeData.requiredWorkers)
            {
                continue;
            }

            StartResearch(state, pair.Key, totalAssignedResearchWorkers);
        }
    }

    private bool ArePrerequisitesCompleted(GameState state, ResearchNodeData nodeData)
    {
        if (nodeData.prerequisiteNodeIds == null)
        {
            return true;
        }

        for (int i = 0; i < nodeData.prerequisiteNodeIds.Length; i++)
        {
            if (!state.IsResearchNodeCompleted(nodeData.prerequisiteNodeIds[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsResearchActive(string nodeId)
    {
        for (int i = 0; i < _activeResearch.Count; i++)
        {
            ResearchProgress progress = _activeResearch[i];
            if (progress.NodeData != null && progress.NodeData.nodeId == nodeId)
            {
                return true;
            }
        }

        return false;
    }

    private void CompleteResearch(GameState state, ResearchNodeData nodeData)
    {
        if (nodeData == null)
        {
            return;
        }

        state.CompleteResearchNode(nodeData.nodeId);
        if (!string.IsNullOrEmpty(nodeData.importantResearchGroupId) && IsImportantGroupCompleted(state, nodeData))
        {
            state.CompleteImportantResearchGroup(nodeData.importantResearchGroupId, nodeData.importantGroupTier);
        }

        ApplyEffects(state, nodeData.effects);
    }

    private bool IsImportantGroupCompleted(GameState state, ResearchNodeData completedNode)
    {
        string groupId = completedNode.importantResearchGroupId;
        foreach (ResearchNodeData node in _nodes.Values)
        {
            if (node.importantResearchGroupId == groupId && !state.IsResearchNodeCompleted(node.nodeId))
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyEffects(GameState state, ResearchEffect[] effects)
    {
        if (effects == null)
        {
            return;
        }

        for (int i = 0; i < effects.Length; i++)
        {
            ResearchEffect effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            if (effect.effectType == ResearchEffectType.AddBaseFoodProduction)
            {
                state.AddBaseFoodProduction(effect.intValue);
            }
            else if (effect.effectType == ResearchEffectType.AddFoodProductionPercent)
            {
                state.AddFoodProductionPercentBonus(effect.intValue);
            }
            else if (effect.effectType == ResearchEffectType.AddResearchSpeedPercent)
            {
                state.AddResearchSpeedPercentBonus(effect.intValue);
            }
            else if (effect.effectType == ResearchEffectType.AddDefensePercent)
            {
                state.AddDefensePowerBonus(effect.intValue);
            }
            else if (effect.effectType == ResearchEffectType.AddMaterialProductionPercent)
            {
                state.AddMaterialProductionPercentBonus(effect.intValue);
            }
            else if (effect.effectType == ResearchEffectType.UnlockExploration)
            {
                state.UnlockDungeonExploration();
            }
            else if (effect.effectType == ResearchEffectType.UnlockGuild)
            {
                for (int j = 0; j < state.Guilds.Count; j++)
                {
                    GuildBase guild = state.Guilds[j];
                    if (guild != null && guild.Data != null && guild.Data.guildType.ToString() == effect.targetId)
                    {
                        guild.Unlock();
                        break;
                    }
                }
            }
            else if (effect.effectType == ResearchEffectType.AddBuildingMaxLevel)
            {
                for (int j = 0; j < state.Buildings.Count; j++)
                {
                    BuildingBase building = state.Buildings[j];
                    if (building != null && (string.IsNullOrEmpty(effect.targetId) || building.Name == effect.targetId))
                    {
                        building.IncreaseMaxLevel(effect.intValue);
                    }
                }
            }
            else if (effect.effectType == ResearchEffectType.AddLordStat)
            {
                if (state.Lord != null)
                {
                    state.Lord.AddStat(effect.targetId, effect.intValue);
                }
            }
            else if (effect.effectType == ResearchEffectType.AddGuildMaxMembers)
            {
                for (int j = 0; j < state.Guilds.Count; j++)
                {
                    GuildBase guild = state.Guilds[j];
                    if (guild != null && guild.Data != null && guild.Data.guildType.ToString() == effect.targetId)
                    {
                        guild.AddMaxMembers(effect.intValue);
                        break;
                    }
                }
            }
        }
    }
}
