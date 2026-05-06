using UnityEngine;
using TMPro;

#pragma warning disable 0649
public class MetaScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text metaPointText;
    [SerializeField] private TMP_Text lordStatsText;

    private GameManager _gameManager;
    private LordCharacter _lordCharacter;
    private int _availableMetaPoints;

    public void Bind(GameManager gameManager)
    {
        Bind(gameManager, null);
    }

    public void Bind(GameManager gameManager, LordCharacter lordCharacter)
    {
        _gameManager = gameManager;
        _lordCharacter = lordCharacter;
        Refresh();
    }

    public void Refresh()
    {
        GameState state = _gameManager != null ? _gameManager.State : null;
        int score = _gameManager != null && _gameManager.ScoreCalculator != null
            ? _gameManager.ScoreCalculator.Calculate(state)
            : 0;
        _availableMetaPoints = _gameManager != null && _gameManager.MetaProgressionSystem != null
            ? _gameManager.MetaProgressionSystem.CalculateEarnedMetaPoints(state)
            : 0;

        SetText(scoreText, $"スコア: {score}");
        SetText(metaPointText, $"獲得予定メタポイント: {_availableMetaPoints}");
        SetText(lordStatsText, BuildLordStatsText());
    }

    public void OnIncreasePopularityClicked()
    {
        TrySpendOnStat(StatType.Popularity);
    }

    public void OnIncreaseNegotiationClicked()
    {
        TrySpendOnStat(StatType.Negotiation);
    }

    public void OnIncreaseLuckClicked()
    {
        TrySpendOnStat(StatType.Luck);
    }

    public void OnIncreaseSupportClicked()
    {
        TrySpendOnStat(StatType.Support);
    }

    public void OnIncreaseThinkingClicked()
    {
        TrySpendOnStat(StatType.Thinking);
    }

    private void TrySpendOnStat(StatType statType)
    {
        if (_lordCharacter == null)
        {
            return;
        }

        bool increased = false;
        int spentPoints = 0;
        if (statType == StatType.Popularity)
        {
            increased = _lordCharacter.TryIncreasePopularity(_availableMetaPoints, out spentPoints);
        }
        else if (statType == StatType.Negotiation)
        {
            increased = _lordCharacter.TryIncreaseNegotiation(_availableMetaPoints, out spentPoints);
        }
        else if (statType == StatType.Luck)
        {
            increased = _lordCharacter.TryIncreaseLuck(_availableMetaPoints, out spentPoints);
        }
        else if (statType == StatType.Support)
        {
            increased = _lordCharacter.TryIncreaseSupport(_availableMetaPoints, out spentPoints);
        }
        else if (statType == StatType.Thinking)
        {
            increased = _lordCharacter.TryIncreaseThinking(_availableMetaPoints, out spentPoints);
        }

        if (increased)
        {
            _availableMetaPoints -= spentPoints;
        }

        SetText(metaPointText, $"未割り振りメタポイント: {_availableMetaPoints}");
        SetText(lordStatsText, BuildLordStatsText());
    }

    private string BuildLordStatsText()
    {
        if (_lordCharacter == null)
        {
            return "主人公ステータス: 未接続";
        }

        return $"人望: {_lordCharacter.Popularity} / 次コスト{_lordCharacter.GetCostToIncrease(_lordCharacter.Popularity)}\n"
            + $"交渉力: {_lordCharacter.Negotiation} / 次コスト{_lordCharacter.GetCostToIncrease(_lordCharacter.Negotiation)}\n"
            + $"運: {_lordCharacter.Luck} / 次コスト{_lordCharacter.GetCostToIncrease(_lordCharacter.Luck)}\n"
            + $"支援力: {_lordCharacter.Support} / 次コスト{_lordCharacter.GetCostToIncrease(_lordCharacter.Support)}\n"
            + $"思考力: {_lordCharacter.Thinking} / 次コスト{_lordCharacter.GetCostToIncrease(_lordCharacter.Thinking)}";
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private enum StatType
    {
        Popularity,
        Negotiation,
        Luck,
        Support,
        Thinking
    }
}
#pragma warning restore 0649
