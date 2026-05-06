using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class RaidPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Text titleText;
    [SerializeField] private Text detailText;
    [SerializeField] private Button closeButton;

    private GameManager _gameManager;
    private RaidResult _currentResult;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }
    }

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;
        _currentResult = _gameManager != null && _gameManager.RaidSystem != null
            ? _gameManager.RaidSystem.LastRaidResult
            : null;
        Refresh();
    }

    public void Show(RaidResult result)
    {
        _currentResult = result;
        if (root != null)
        {
            root.SetActive(true);
        }

        Refresh();
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Refresh()
    {
        if (_gameManager != null && _gameManager.RaidSystem != null)
        {
            _currentResult = _gameManager.RaidSystem.LastRaidResult;
        }

        if (_currentResult == null || _currentResult.Outcome == RaidOutcome.None)
        {
            SetText(titleText, "襲撃なし");
            SetText(detailText, string.Empty);
            return;
        }

        SetText(titleText, $"襲撃結果: {FormatOutcome(_currentResult.Outcome)}");
        SetText(detailText, $"敵戦力: {_currentResult.EnemyPower}\n都市防衛力: {_currentResult.DefensePower}");
    }

    private static string FormatOutcome(RaidOutcome outcome)
    {
        if (outcome == RaidOutcome.PerfectWin)
        {
            return "完全勝利";
        }

        if (outcome == RaidOutcome.CloseWin)
        {
            return "辛勝";
        }

        if (outcome == RaidOutcome.Loss)
        {
            return "敗北";
        }

        if (outcome == RaidOutcome.Collapse)
        {
            return "壊滅";
        }

        return "なし";
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
