using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class HappinessPanel : MonoBehaviour
{
    [SerializeField] private Text happinessText;
    [SerializeField] private Text detailText;

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
            SetText(happinessText, "幸福度: 未接続");
            SetText(detailText, string.Empty);
            return;
        }

        SetText(happinessText, $"幸福度: {_state.Happiness}/100");
        SetText(detailText, BuildDetailText(_state));
    }

    private static string BuildDetailText(GameState state)
    {
        string crisis = state.Happiness < 20 ? "危機" : "通常";
        float foodMonths = state.Population > 0 ? (float)state.Food / state.Population : 0f;
        float crowding = state.PopulationCapacity > 0 ? (float)state.Population / state.PopulationCapacity : 0f;
        return $"状態: {crisis}\n食料備蓄: {foodMonths:0.0}か月分\n人口密度: {crowding * 100f:0}%\n危機継続: {state.HappinessCrisisTurnCount}ターン";
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
