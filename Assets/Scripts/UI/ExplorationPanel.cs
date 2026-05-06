using UnityEngine;
using TMPro;
using UnityEngine.UI;

#pragma warning disable 0649
public class ExplorationPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button exploreButton;
    [SerializeField] private Button startDungeonButton;

    private GameManager _gameManager;
    private string _lastResult;

    private void Awake()
    {
        if (exploreButton != null)
        {
            exploreButton.onClick.AddListener(OnExploreButtonClicked);
        }

        if (startDungeonButton != null)
        {
            startDungeonButton.onClick.AddListener(OnStartDungeonButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (exploreButton != null)
        {
            exploreButton.onClick.RemoveListener(OnExploreButtonClicked);
        }

        if (startDungeonButton != null)
        {
            startDungeonButton.onClick.RemoveListener(OnStartDungeonButtonClicked);
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

        string explored = state.IsInitialRaidOriginExplored ? "踏破済" : "未踏破";
        string dungeon = state.IsDungeonExplorationUnlocked ? "解放済" : "未解放";
        int activeRuns = _gameManager != null && _gameManager.DungeonSystem != null
            ? _gameManager.DungeonSystem.ActiveRuns.Count
            : 0;
        SetText(statusText, $"襲撃起点: {explored} / 進捗{state.InitialRaidOriginExplorationProgress}% / ダンジョン {dungeon} / 探索中 {activeRuns}件");
        SetText(resultText, _lastResult);
    }

    public void OnExploreButtonClicked()
    {
        if (_gameManager == null || _gameManager.RaidSystem == null || _gameManager.State == null)
        {
            return;
        }

        bool success = _gameManager.RaidSystem.ExploreInitialRaidOrigin(_gameManager.State);
        _lastResult = success ? "偵察成功。進捗 +50%。" : "偵察失敗。進捗 +34%。";
        if (_gameManager.State.IsInitialRaidOriginExplored)
        {
            _lastResult += " ダンジョン入口の踏破が完了しました。";
        }

        Refresh();
    }

    public void OnStartDungeonButtonClicked()
    {
        if (_gameManager == null)
        {
            return;
        }

        GuildMember member = _gameManager.FindFirstMemberByAction(GuildAction.Explore);
        bool started = _gameManager.TryStartDungeonExploration(member, out string reason);
        _lastResult = started ? reason : "ダンジョン探索を開始できません: " + reason;
        Refresh();
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
