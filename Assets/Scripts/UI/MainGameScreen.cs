using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class MainGameScreen : MonoBehaviour
{
    [SerializeField] private Text turnText;
    [SerializeField] private Text gameStateText;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private ResourcePanel resourcePanel;
    [SerializeField] private ResearchPanel researchPanel;
    [SerializeField] private ExplorationPanel explorationPanel;
    [SerializeField] private BuildPanel buildPanel;
    [SerializeField] private GuildPanel guildPanel;
    [SerializeField] private HappinessPanel happinessPanel;
    [SerializeField] private RaidPopup raidPopup;
    [SerializeField] private MetaScreen metaScreen;

    private GameManager _gameManager;

    private void Awake()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(OnEndTurnButtonClicked);
        }
    }

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;

        if (_gameManager != null)
        {
            resourcePanel?.Bind(_gameManager.State);
            researchPanel?.Bind(_gameManager);
            explorationPanel?.Bind(_gameManager);
            buildPanel?.Bind(_gameManager.State);
            guildPanel?.Bind(_gameManager.State);
            happinessPanel?.Bind(_gameManager.State);
            raidPopup?.Bind(_gameManager);
            metaScreen?.Bind(_gameManager);
        }

        Refresh();
    }

    public void Refresh()
    {
        GameState state = _gameManager != null ? _gameManager.State : null;
        SetText(turnText, state != null ? FormatTurn(state.CurrentTurn) : "ターン: -");
        SetText(gameStateText, FormatGameState(state));

        resourcePanel?.Refresh(state);
        researchPanel?.Refresh();
        explorationPanel?.Refresh();
        buildPanel?.Refresh(state);
        guildPanel?.Refresh(state);
        happinessPanel?.Refresh(state);
        raidPopup?.Refresh();
        metaScreen?.Refresh();
    }

    public void OnEndTurnButtonClicked()
    {
        if (_gameManager == null)
        {
            return;
        }

        _gameManager.AdvanceTurn();
        Refresh();
    }

    private static string FormatTurn(int turn)
    {
        int zeroBasedTurn = turn - 1;
        int year = zeroBasedTurn / GameConstants.TURNS_PER_YEAR + 1;
        int month = zeroBasedTurn % GameConstants.TURNS_PER_YEAR + 1;
        return $"ターン {turn} / {GameConstants.MAX_TURNS}  {year}年目 {month}月";
    }

    private static string FormatGameState(GameState state)
    {
        if (state == null)
        {
            return "状態: 未接続";
        }

        if (!state.IsGameOver)
        {
            return "状態: 進行中";
        }

        string result = state.IsVictory ? "勝利" : "敗北";
        return $"状態: {result} / {state.GameEndReason}";
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
