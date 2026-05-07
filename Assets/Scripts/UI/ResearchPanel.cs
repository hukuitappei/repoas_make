using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

#pragma warning disable 0649
public class ResearchPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text activeResearchText;
    [SerializeField] private TMP_Text completedResearchText;
    [SerializeField] private TMP_Text availableResearchText;
    [SerializeField] private ResearchNodeData[] availableNodes;

    private GameManager _gameManager;

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;
        RegisterAvailableNodes();
        Refresh();
    }

    public void Bind(GameManager gameManager, ResearchNodeData[] nodes)
    {
        _gameManager = gameManager;
        availableNodes = nodes;
        RegisterAvailableNodes();
        Refresh();
    }

    public void Refresh()
    {
        GameState state = _gameManager != null ? _gameManager.State : null;
        ResearchTree researchTree = _gameManager != null ? _gameManager.ResearchTree : null;

        SetText(activeResearchText, BuildActiveResearchText(researchTree));
        SetText(completedResearchText, BuildCompletedResearchText(state));
        SetText(availableResearchText, BuildAvailableResearchText(state, researchTree));
    }

    public void OnStartResearchClicked(string nodeId)
    {
        if (_gameManager == null || _gameManager.State == null || _gameManager.ResearchTree == null)
        {
            return;
        }

        int workers = _gameManager.GuildManager != null
            ? _gameManager.GuildManager.CountAssignedResearchWorkers(_gameManager.State)
            : 0;
        _gameManager.ResearchTree.StartResearch(_gameManager.State, nodeId, workers);
        Refresh();
    }

    private void RegisterAvailableNodes()
    {
        if (_gameManager == null || _gameManager.ResearchTree == null || availableNodes == null)
        {
            return;
        }

        for (int i = 0; i < availableNodes.Length; i++)
        {
            _gameManager.ResearchTree.RegisterNode(availableNodes[i]);
        }
    }

    private static string BuildActiveResearchText(ResearchTree researchTree)
    {
        if (researchTree == null)
        {
            return "進行中研究: 未接続";
        }

        if (researchTree.ActiveResearch.Count == 0)
        {
            return "進行中研究: なし";
        }

        StringBuilder builder = new StringBuilder("進行中研究\n");
        for (int i = 0; i < researchTree.ActiveResearch.Count; i++)
        {
            ResearchProgress progress = researchTree.ActiveResearch[i];
            if (progress == null || progress.NodeData == null)
            {
                continue;
            }

            int totalTurns = progress.NodeData.researchDurationTurns > 0 ? progress.NodeData.researchDurationTurns : 1;
            int completedTurns = totalTurns - progress.RemainingTurns;
            float ratio = Mathf.Clamp01(completedTurns / (float)totalTurns);
            builder.Append(progress.NodeData.displayName)
                .Append(" / 残り")
                .Append(progress.RemainingTurns)
                .Append("ターン ")
                .Append(BuildProgressBar(ratio))
                .AppendLine();
        }

        return builder.ToString();
    }

    private static string BuildCompletedResearchText(GameState state)
    {
        if (state == null)
        {
            return "完了研究: 未接続";
        }

        if (state.CompletedResearchNodeIds.Count == 0)
        {
            return "完了研究: なし";
        }

        return "完了研究: " + string.Join(", ", state.CompletedResearchNodeIds);
    }

    private string BuildAvailableResearchText(GameState state, ResearchTree researchTree)
    {
        List<ResearchNodeData> nodes = CollectDisplayNodes(researchTree);
        if (nodes.Count == 0)
        {
            return "未完了ノード一覧: 研究ノード未登録";
        }

        StringBuilder builder = new StringBuilder("未完了ノード一覧\n");
        bool hasAnyIncomplete = false;
        for (int i = 0; i < nodes.Count; i++)
        {
            ResearchNodeData node = nodes[i];
            if (node == null)
            {
                continue;
            }

            bool completed = state != null && state.IsResearchNodeCompleted(node.nodeId);
            if (completed)
            {
                continue;
            }

            hasAnyIncomplete = true;
            bool isActive = IsActiveResearch(researchTree, node.nodeId);
            bool canStart = state != null
                && researchTree != null
                && researchTree.CanStartResearch(state, node.nodeId, GetAssignedResearchWorkers(state));

            builder.Append(node.displayName)
                .Append(" [")
                .Append(isActive ? "進行中" : (canStart ? "着手可" : "未着手"))
                .AppendLine("]");
            builder.Append("  コスト: 資金").Append(node.researchCostFunds)
                .Append(" / 食料").Append(node.requiredFood)
                .Append(" / 人員").Append(node.requiredWorkers)
                .Append(" / ").Append(node.researchDurationTurns).AppendLine("ターン");
            builder.Append("  前提: ").Append(BuildPrerequisiteText(state, node)).AppendLine();
            builder.Append("  素材: ").Append(BuildMaterialRequirementsText(node.materialRequirements)).AppendLine();
        }

        return hasAnyIncomplete ? builder.ToString() : "未完了ノード一覧: なし";
    }

    private static int GetAssignedResearchWorkers(GameState state)
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
                if (member != null && member.CurrentAction == GuildAction.Research)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private List<ResearchNodeData> CollectDisplayNodes(ResearchTree researchTree)
    {
        List<ResearchNodeData> nodes = new List<ResearchNodeData>();
        if (researchTree != null)
        {
            foreach (ResearchNodeData node in researchTree.RegisteredNodes)
            {
                if (node != null)
                {
                    nodes.Add(node);
                }
            }
        }

        if (nodes.Count == 0 && availableNodes != null)
        {
            for (int i = 0; i < availableNodes.Length; i++)
            {
                if (availableNodes[i] != null)
                {
                    nodes.Add(availableNodes[i]);
                }
            }
        }

        return nodes;
    }

    private static bool IsActiveResearch(ResearchTree researchTree, string nodeId)
    {
        if (researchTree == null || string.IsNullOrEmpty(nodeId))
        {
            return false;
        }

        for (int i = 0; i < researchTree.ActiveResearch.Count; i++)
        {
            ResearchProgress progress = researchTree.ActiveResearch[i];
            if (progress != null && progress.NodeData != null && progress.NodeData.nodeId == nodeId)
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildPrerequisiteText(GameState state, ResearchNodeData node)
    {
        if (node == null || node.prerequisiteNodeIds == null || node.prerequisiteNodeIds.Length == 0)
        {
            return "なし";
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < node.prerequisiteNodeIds.Length; i++)
        {
            string prerequisite = node.prerequisiteNodeIds[i];
            bool completed = state != null && state.IsResearchNodeCompleted(prerequisite);
            builder.Append(prerequisite).Append(completed ? "(完了)" : "(未)");
            if (i < node.prerequisiteNodeIds.Length - 1)
            {
                builder.Append(", ");
            }
        }

        return builder.ToString();
    }

    private static string BuildMaterialRequirementsText(MaterialRequirement[] requirements)
    {
        if (requirements == null || requirements.Length == 0)
        {
            return "なし";
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < requirements.Length; i++)
        {
            MaterialRequirement requirement = requirements[i];
            builder.Append(requirement.Type)
                .Append(" ")
                .Append(requirement.MinimumGrade)
                .Append("以上 x")
                .Append(requirement.Amount);
            if (i < requirements.Length - 1)
            {
                builder.Append(", ");
            }
        }

        return builder.ToString();
    }

    private static string BuildProgressBar(float ratio)
    {
        const int width = 10;
        int filled = Mathf.RoundToInt(ratio * width);
        filled = Mathf.Clamp(filled, 0, width);
        return "[" + new string('■', filled) + new string('□', width - filled) + "]";
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
