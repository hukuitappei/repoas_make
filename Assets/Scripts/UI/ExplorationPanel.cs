using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class ExplorationPanel : MonoBehaviour
{
    [SerializeField] private Text statusText;
    [SerializeField] private Text resultText;
    [SerializeField] private Button exploreButton;

    private GameManager _gameManager;
    private string _lastResult;

    private void Awake()
    {
        if (exploreButton != null)
        {
            exploreButton.onClick.AddListener(OnExploreButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (exploreButton != null)
        {
            exploreButton.onClick.RemoveListener(OnExploreButtonClicked);
        }
    }

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;
        _lastResult = string.Empty;
        Refresh();
    }

    public void Refresh()
    {
        GameState state = _gameManager != null ? _gameManager.State : null;
        if (state == null)
        {
            SetText(statusText, "探索: 未接続");
            SetText(resultText, string.Empty);
            return;
        }

        string explored = state.IsInitialRaidOriginExplored ? "探索済み" : "未探索";
        SetText(statusText, $"初期襲撃起点: {explored} / 進捗 {state.InitialRaidOriginExplorationProgress}%");
        SetText(resultText, _lastResult);
    }

    public void OnExploreButtonClicked()
    {
        if (_gameManager == null || _gameManager.RaidSystem == null || _gameManager.State == null)
        {
            return;
        }

        bool success = _gameManager.RaidSystem.ExploreInitialRaidOrigin(_gameManager.State);
        _lastResult = success ? "探索結果: 成功、進捗 +50%" : "探索結果: 失敗、進捗 +34%";
        if (_gameManager.State.IsInitialRaidOriginExplored)
        {
            _lastResult += " / 起点を特定しました。";
        }

        Refresh();
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
