using System.Text;
using UnityEngine;
using TMPro;

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
        SetText(availableResearchText, BuildAvailableResearchText(state));
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
            return "研究中: 未接続";
        }

        if (researchTree.ActiveResearch.Count == 0)
        {
            return "研究中: なし";
        }

        StringBuilder builder = new StringBuilder("研究中:\n");
        for (int i = 0; i < researchTree.ActiveResearch.Count; i++)
        {
            ResearchProgress progress = researchTree.ActiveResearch[i];
            string name = progress.NodeData != null ? progress.NodeData.displayName : "不明な研究";
            builder.AppendLine($"{name} / 残り{progress.RemainingTurns}ターン");
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

    private string BuildAvailableResearchText(GameState state)
    {
        if (availableNodes == null || availableNodes.Length == 0)
        {
            return "研究候補: 未設定";
        }

        StringBuilder builder = new StringBuilder("研究候補:\n");
        for (int i = 0; i < availableNodes.Length; i++)
        {
            ResearchNodeData node = availableNodes[i];
            if (node == null)
            {
                continue;
            }

            bool completed = state != null && state.IsResearchNodeCompleted(node.nodeId);
            builder.Append(node.displayName).Append(" [").Append(completed ? "完了" : "未完了").Append("]");
            builder.Append(" 資金").Append(node.researchCostFunds);
            builder.Append(" 食料").Append(node.requiredFood);
            builder.Append(" 人員").Append(node.requiredWorkers);
            builder.Append(" 期間").Append(node.researchDurationTurns).AppendLine("T");
        }

        return builder.ToString();
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
