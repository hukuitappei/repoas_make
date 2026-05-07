using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class BuildPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text ownedBuildingsText;
    [SerializeField] private TMP_Text availableBuildingsText;
    [SerializeField] private TMP_Text buildQueueText;
    [SerializeField] private TMP_Text selectedBuildingText;
    [SerializeField] private TMP_Text actionResultText;
    [SerializeField] private Button previousBuildingButton;
    [SerializeField] private Button nextBuildingButton;
    [SerializeField] private Button buildOrUpgradeButton;
    [SerializeField] private BuildingData[] availableBuildings;

    private GameManager _gameManager;
    private GameState _state;
    private int _selectedBuildingIndex;

    private void Awake()
    {
        AddListener(previousBuildingButton, OnPreviousBuildingClicked);
        AddListener(nextBuildingButton, OnNextBuildingClicked);
        AddListener(buildOrUpgradeButton, OnBuildOrUpgradeClicked);
    }

    private void OnDestroy()
    {
        RemoveListener(previousBuildingButton, OnPreviousBuildingClicked);
        RemoveListener(nextBuildingButton, OnNextBuildingClicked);
        RemoveListener(buildOrUpgradeButton, OnBuildOrUpgradeClicked);
    }

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;
        _state = _gameManager != null ? _gameManager.State : null;
        Refresh();
    }

    public void Bind(GameManager gameManager, BuildingData[] buildings)
    {
        _gameManager = gameManager;
        _state = _gameManager != null ? _gameManager.State : null;
        availableBuildings = buildings;
        Refresh();
    }

    public void Bind(GameState state)
    {
        _gameManager = null;
        _state = state;
        Refresh();
    }

    public void Bind(GameState state, BuildingData[] buildings)
    {
        _gameManager = null;
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
        if (_gameManager != null)
        {
            _state = _gameManager.State;
        }

        ClampSelection();
        SetText(ownedBuildingsText, BuildOwnedBuildingsText(_state));
        SetText(availableBuildingsText, BuildAvailableBuildingsText(_state, availableBuildings, _selectedBuildingIndex));
        SetText(buildQueueText, BuildQueueDisplayText(_state));
        SetText(selectedBuildingText, BuildSelectedBuildingText());
    }

    public void OnPreviousBuildingClicked()
    {
        if (availableBuildings == null || availableBuildings.Length == 0)
        {
            return;
        }

        _selectedBuildingIndex = (_selectedBuildingIndex - 1 + availableBuildings.Length) % availableBuildings.Length;
        SetText(actionResultText, string.Empty);
        Refresh();
    }

    public void OnNextBuildingClicked()
    {
        if (availableBuildings == null || availableBuildings.Length == 0)
        {
            return;
        }

        _selectedBuildingIndex = (_selectedBuildingIndex + 1) % availableBuildings.Length;
        SetText(actionResultText, string.Empty);
        Refresh();
    }

    public void OnBuildOrUpgradeClicked()
    {
        if (_gameManager == null)
        {
            SetText(actionResultText, "建設処理を開始できません。");
            return;
        }

        BuildingData selectedBuilding = GetSelectedBuilding();
        if (selectedBuilding == null)
        {
            SetText(actionResultText, "選択中の建設データがありません。");
            return;
        }

        _gameManager.TryBuildOrUpgradeBuilding(selectedBuilding, out string message);
        SetText(actionResultText, message);
        Refresh();
    }

    private string BuildSelectedBuildingText()
    {
        BuildingData selectedBuilding = GetSelectedBuilding();
        if (selectedBuilding == null)
        {
            return "選択中の建設: なし";
        }

        BuildingBase ownedBuilding = FindOwnedBuilding(_state, selectedBuilding.buildingName);
        int nextLevel = ownedBuilding == null ? 1 : ownedBuilding.Level + 1;
        bool isUpgrade = ownedBuilding != null;
        bool canAct = _gameManager != null
            ? (ownedBuilding == null || ownedBuilding.CanUpgrade(_state))
            : ownedBuilding == null || ownedBuilding.CanUpgrade(_state);

        StringBuilder builder = new StringBuilder();
        builder.Append("選択中の建設: ").Append(selectedBuilding.buildingName);
        builder.Append(isUpgrade ? " / 強化先" : " / 新規建設");
        builder.Append(" Lv.").Append(nextLevel).AppendLine();
        builder.Append("資金: ").Append(selectedBuilding.GetBuildCostFunds(nextLevel)).AppendLine();
        builder.Append("素材: ").Append(BuildMaterialRequirementsText(selectedBuilding.GetBuildMaterialRequirements(nextLevel))).AppendLine();
        builder.Append("維持費: ").Append(selectedBuilding.GetMaintenanceCostFunds(Mathf.Clamp(nextLevel, 1, selectedBuilding.maxLevel)));
        builder.Append(" / 効果: ").Append(selectedBuilding.GetEffectValue(Mathf.Clamp(nextLevel, 1, selectedBuilding.maxLevel))).AppendLine();
        builder.Append("状態: ").Append(canAct ? "実行可能" : "実行不可");
        return builder.ToString();
    }

    private static string BuildOwnedBuildingsText(GameState state)
    {
        if (state == null)
        {
            return "所有建設: 未接続";
        }

        if (state.Buildings.Count == 0)
        {
            return "所有建設: なし";
        }

        StringBuilder builder = new StringBuilder("所有建設\n");
        for (int i = 0; i < state.Buildings.Count; i++)
        {
            BuildingBase building = state.Buildings[i];
            if (building == null)
            {
                continue;
            }

            string active = building.IsActive ? "稼働中" : "停止中";
            builder.Append(building.Name)
                .Append(" Lv.")
                .Append(building.Level)
                .Append("/")
                .Append(building.MaxLevel)
                .Append(" [")
                .Append(active)
                .Append("] / 維持費 ")
                .Append(building.Data != null ? building.Data.GetMaintenanceCostFunds(building.Level) : 0)
                .Append(" / 効果 ")
                .Append(building.Data != null ? building.Data.GetEffectValue(building.Level) : 0)
                .AppendLine();
        }

        return builder.ToString();
    }

    private static string BuildAvailableBuildingsText(GameState state, BuildingData[] buildings, int selectedIndex)
    {
        if (buildings == null || buildings.Length == 0)
        {
            return "建設可能建物一覧: 未設定";
        }

        StringBuilder builder = new StringBuilder("建設可能建物一覧\n");
        for (int i = 0; i < buildings.Length; i++)
        {
            BuildingData data = buildings[i];
            if (data == null)
            {
                continue;
            }

            BuildingBase ownedBuilding = FindOwnedBuilding(state, data.buildingName);
            string prefix = i == selectedIndex ? "> " : "  ";
            string status = ownedBuilding == null
                ? "未建設"
                : (ownedBuilding.Level >= ownedBuilding.MaxLevel ? "最大Lv" : "強化可能");
            builder.Append(prefix).Append(data.buildingName).Append(" [").Append(status).AppendLine("]");
        }

        return builder.ToString();
    }

    private static string BuildQueueDisplayText(GameState state)
    {
        if (state == null)
        {
            return "建設キュー: 未接続";
        }

        List<string> queueEntries = new List<string>();
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
                if (member == null || member.CurrentAction != GuildAction.Construct)
                {
                    continue;
                }

                string target = string.IsNullOrEmpty(member.CurrentActionTargetId) ? "対象未設定" : member.CurrentActionTargetId;
                queueEntries.Add($"{member.Name} -> {target}");
            }
        }

        if (queueEntries.Count == 0)
        {
            return "建設キュー\nなし";
        }

        StringBuilder builder = new StringBuilder("建設キュー\n");
        for (int i = 0; i < queueEntries.Count; i++)
        {
            builder.Append(i + 1).Append(". ").AppendLine(queueEntries[i]);
        }

        return builder.ToString();
    }

    private BuildingData GetSelectedBuilding()
    {
        if (availableBuildings == null || availableBuildings.Length == 0)
        {
            return null;
        }

        ClampSelection();
        return availableBuildings[_selectedBuildingIndex];
    }

    private void ClampSelection()
    {
        if (availableBuildings == null || availableBuildings.Length == 0)
        {
            _selectedBuildingIndex = 0;
            return;
        }

        if (_selectedBuildingIndex < 0)
        {
            _selectedBuildingIndex = 0;
        }
        else if (_selectedBuildingIndex >= availableBuildings.Length)
        {
            _selectedBuildingIndex = availableBuildings.Length - 1;
        }
    }

    private static BuildingBase FindOwnedBuilding(GameState state, string buildingName)
    {
        if (state == null || string.IsNullOrEmpty(buildingName))
        {
            return null;
        }

        for (int i = 0; i < state.Buildings.Count; i++)
        {
            BuildingBase building = state.Buildings[i];
            if (building != null && building.Name == buildingName)
            {
                return building;
            }
        }

        return null;
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

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
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
