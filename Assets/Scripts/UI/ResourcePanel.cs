using System.Text;
using TMPro;
using UnityEngine;

#pragma warning disable 0649
public class ResourcePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private TMP_Text materialsText;
    [SerializeField] private TMP_Text warningText;

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
            SetText(summaryText, "リソース: 未接続");
            SetText(materialsText, string.Empty);
            SetText(warningText, string.Empty);
            return;
        }

        SetText(summaryText, $"食料: {_state.Food} / 資金: {_state.Funds} / 人口: {_state.Population}/{_state.PopulationCapacity}");
        SetText(materialsText, BuildMaterialsText(_state));
        SetText(warningText, BuildWarningText(_state));
    }

    private static string BuildMaterialsText(GameState state)
    {
        StringBuilder builder = new StringBuilder();
        AppendMaterialLine(builder, state, MaterialType.Stone, "石材");
        AppendMaterialLine(builder, state, MaterialType.Wood, "木材");
        AppendMaterialLine(builder, state, MaterialType.Metal, "金属");
        AppendMaterialLine(builder, state, MaterialType.Foodstuff, "食材");
        AppendMaterialLine(builder, state, MaterialType.Magic, "魔力素材");
        return builder.ToString();
    }

    private static void AppendMaterialLine(StringBuilder builder, GameState state, MaterialType type, string label)
    {
        builder.Append(label).Append(": ");
        for (int i = (int)MaterialGrade.F; i <= (int)MaterialGrade.S; i++)
        {
            MaterialGrade grade = (MaterialGrade)i;
            builder.Append(grade).Append("=").Append(state.GetMaterialAmount(type, grade));
            if (i < (int)MaterialGrade.S)
            {
                builder.Append(" ");
            }
        }

        builder.AppendLine();
    }

    private static string BuildWarningText(GameState state)
    {
        StringBuilder builder = new StringBuilder();
        if (state.Food < state.Population)
        {
            builder.AppendLine("警告: 次ターンの食料不足の可能性があります。");
        }

        if (state.Funds < 0)
        {
            builder.AppendLine("警告: 資金がマイナスです。");
        }

        if (state.Happiness < 20)
        {
            builder.AppendLine("警告: 幸福度が危険水準です。");
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
