using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class PopulationAssignmentPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text assignmentSummaryText;
    [SerializeField] private Slider foodWorkersSlider;
    [SerializeField] private Slider fundsWorkersSlider;
    [SerializeField] private Slider developmentWorkersSlider;
    [SerializeField] private TMP_Text foodWorkersValueText;
    [SerializeField] private TMP_Text fundsWorkersValueText;
    [SerializeField] private TMP_Text developmentWorkersValueText;

    private GameManager _gameManager;
    private GameState _state;

    private void Awake()
    {
        AddSliderListener(foodWorkersSlider, OnFoodWorkersChanged);
        AddSliderListener(fundsWorkersSlider, OnFundsWorkersChanged);
        AddSliderListener(developmentWorkersSlider, OnDevelopmentWorkersChanged);
    }

    private void OnDestroy()
    {
        RemoveSliderListener(foodWorkersSlider, OnFoodWorkersChanged);
        RemoveSliderListener(fundsWorkersSlider, OnFundsWorkersChanged);
        RemoveSliderListener(developmentWorkersSlider, OnDevelopmentWorkersChanged);
    }

    public void Bind(GameManager gameManager)
    {
        _gameManager = gameManager;
        _state = _gameManager != null ? _gameManager.State : null;
        Refresh();
    }

    public void Refresh()
    {
        if (_gameManager != null)
        {
            _state = _gameManager.State;
        }

        if (_state == null)
        {
            SetText(assignmentSummaryText, "人口配分: 未接続");
            SetText(foodWorkersValueText, string.Empty);
            SetText(fundsWorkersValueText, string.Empty);
            SetText(developmentWorkersValueText, string.Empty);
            return;
        }

        SetText(assignmentSummaryText, $"人口配分: 食料 {_state.AssignedFoodWorkers} / 資金 {_state.AssignedFundsWorkers} / 開拓 {_state.AssignedDevelopmentWorkers} / 自由 {_state.FreePopulation}");
        SetText(foodWorkersValueText, $"食料担当 {_state.AssignedFoodWorkers}");
        SetText(fundsWorkersValueText, $"資金担当 {_state.AssignedFundsWorkers}");
        SetText(developmentWorkersValueText, $"開拓担当 {_state.AssignedDevelopmentWorkers}");

        SetSliderValue(foodWorkersSlider, _state.AssignedFoodWorkers, 0, GameConstants.MAX_ASSIGNED_FOOD_WORKERS);
        SetSliderValue(fundsWorkersSlider, _state.AssignedFundsWorkers, 0, GameConstants.MAX_ASSIGNED_FUNDS_WORKERS);
        SetSliderValue(developmentWorkersSlider, _state.AssignedDevelopmentWorkers, 0, _state.Population);
    }

    private void OnFoodWorkersChanged(float value)
    {
        if (_gameManager == null)
        {
            return;
        }

        if (!_gameManager.TrySetAssignedFoodWorkers(Mathf.RoundToInt(value)))
        {
            Refresh();
            return;
        }

        Refresh();
    }

    private void OnFundsWorkersChanged(float value)
    {
        if (_gameManager == null)
        {
            return;
        }

        if (!_gameManager.TrySetAssignedFundsWorkers(Mathf.RoundToInt(value)))
        {
            Refresh();
            return;
        }

        Refresh();
    }

    private void OnDevelopmentWorkersChanged(float value)
    {
        if (_gameManager == null)
        {
            return;
        }

        if (!_gameManager.TrySetAssignedDevelopmentWorkers(Mathf.RoundToInt(value)))
        {
            Refresh();
            return;
        }

        Refresh();
    }

    private static void SetSliderValue(Slider slider, int value, int min, int max)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
        slider.SetValueWithoutNotify(Mathf.Clamp(value, min, max));
    }

    private static void AddSliderListener(Slider slider, UnityEngine.Events.UnityAction<float> listener)
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(listener);
        }
    }

    private static void RemoveSliderListener(Slider slider, UnityEngine.Events.UnityAction<float> listener)
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(listener);
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
