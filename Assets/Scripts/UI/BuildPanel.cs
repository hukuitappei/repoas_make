using System.Text;
using UnityEngine;
using TMPro;

#pragma warning disable 0649
public class BuildPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text ownedBuildingsText;
    [SerializeField] private TMP_Text availableBuildingsText;
    [SerializeField] private BuildingData[] availableBuildings;

    private GameState _state;

    public void Bind(GameState state)
    {
        _state = state;
        Refresh();
    }

    public void Bind(GameState state, BuildingData[] buildings)
    {
        _state = state;
        availableBuildings = buildings;
        Refresh();
    }

    public void Refresh(GameState state)
    {
        _state = state;
        Refresh();
    }

    public void Refresh()
    {
        SetText(ownedBuildingsText, BuildOwnedBuildingsText(_state));
        SetText(availableBuildingsText, BuildAvailableBuildingsText());
    }

    private static string BuildOwnedBuildingsText(GameState state)
    {
        if (state == null)
        {
            return "施設: 未接続";
        }

        if (state.Buildings.Count == 0)
        {
            return "建設済み施設: なし";
        }

        StringBuilder builder = new StringBuilder("建設済み施設:\n");
        for (int i = 0; i < state.Buildings.Count; i++)
        {
            BuildingBase building = state.Buildings[i];
            if (building == null)
            {
                continue;
            }

            string active = building.IsActive ? "稼働" : "停止";
            builder.AppendLine($"{building.Name} Lv.{building.Level}/{building.MaxLevel} [{active}]");
        }

        return builder.ToString();
    }

    private string BuildAvailableBuildingsText()
    {
        if (availableBuildings == null || availableBuildings.Length == 0)
        {
            return "建設候補: 未設定";
        }

        StringBuilder builder = new StringBuilder("建設候補:\n");
        for (int i = 0; i < availableBuildings.Length; i++)
        {
            BuildingData data = availableBuildings[i];
            if (data == null)
            {
                continue;
            }

            builder.Append(data.buildingName);
            builder.Append(" / 最大Lv.").Append(data.maxLevel);
            builder.Append(" / Lv1資金").Append(data.GetBuildCostFunds(1));
            builder.Append(" / 維持").Append(data.GetMaintenanceCostFunds(1));
            builder.Append(" / 効果").Append(data.GetEffectValue(1));
            builder.AppendLine();
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
