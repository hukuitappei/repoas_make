using System;

public class DevelopmentSystem
{
    private readonly Random _random;
    private int _selectedTargetX;
    private int _selectedTargetY;
    private int _trackedTargetX;
    private int _trackedTargetY;

    public string LastResultMessage { get; private set; }
    public bool HasSelectedTarget => _selectedTargetX >= 0 && _selectedTargetY >= 0;
    public int SelectedTargetX => _selectedTargetX;
    public int SelectedTargetY => _selectedTargetY;
    public float AccumulatedSuccessRate { get; private set; }
    public int ConsecutiveInactiveTurns { get; private set; }
    public int LastAssignedDeveloperCount { get; private set; }

    public DevelopmentSystem()
    {
        _random = new Random();
        _selectedTargetX = -1;
        _selectedTargetY = -1;
        _trackedTargetX = -1;
        _trackedTargetY = -1;
        LastResultMessage = string.Empty;
    }

    public bool TrySelectTarget(MapData mapData, int x, int y, out string message)
    {
        if (mapData == null)
        {
            message = "マップが未生成です。";
            return false;
        }

        if (!mapData.IsValidDevelopmentTarget(x, y))
        {
            message = $"({x},{y}) は開拓対象にできません。隣接する未開拓地を選んでください。";
            return false;
        }

        _selectedTargetX = x;
        _selectedTargetY = y;
        ResetAccumulationIfTargetChanged(x, y);
        int distance = mapData.GetDistanceFromHome(x, y);
        message = $"開拓対象を ({x},{y}) に設定しました。距離 {distance}";
        return true;
    }

    public void ClearSelectedTarget()
    {
        _selectedTargetX = -1;
        _selectedTargetY = -1;
    }

    public float CalculateTurnGain(GameState state, MapData mapData, int x, int y)
    {
        if (state == null || mapData == null)
        {
            return 0f;
        }

        int distanceFromHome = mapData.GetDistanceFromHome(x, y);
        float gain = state.AssignedDevelopmentWorkers * GameConstants.DEVELOPMENT_SUCCESS_RATE_PER_WORKER;
        gain -= distanceFromHome * GameConstants.DEVELOPMENT_DISTANCE_PENALTY_PER_TILE;
        return Clamp(gain, 0f, GameConstants.DEVELOPMENT_MAX_SUCCESS_RATE);
    }

    public void ResolveTurnDevelopment(GameState state, MapData mapData, int assignedDeveloperMembers)
    {
        LastResultMessage = string.Empty;
        LastAssignedDeveloperCount = assignedDeveloperMembers;

        if (state == null || mapData == null)
        {
            return;
        }

        if (state.AssignedDevelopmentWorkers <= 0)
        {
            ApplyDecay();
            LastResultMessage = $"開拓担当人口がいないため、開拓値が減衰しました。現在 {(int)(AccumulatedSuccessRate * 100f)}%";
            return;
        }

        if (assignedDeveloperMembers <= 0)
        {
            ApplyDecay();
            LastResultMessage = $"開拓担当ギルド員がいないため、開拓値が減衰しました。現在 {(int)(AccumulatedSuccessRate * 100f)}%";
            return;
        }

        if (!TryResolveTarget(mapData, out int x, out int y))
        {
            ApplyDecay();
            LastResultMessage = $"開拓候補がないため、開拓値が減衰しました。現在 {(int)(AccumulatedSuccessRate * 100f)}%";
            return;
        }

        ResetAccumulationIfTargetChanged(x, y);

        ConsecutiveInactiveTurns = 0;
        float gainedRate = CalculateTurnGain(state, mapData, x, y);
        AccumulatedSuccessRate = Clamp(AccumulatedSuccessRate + gainedRate, 0f, GameConstants.DEVELOPMENT_MAX_SUCCESS_RATE);

        int distanceFromHome = mapData.GetDistanceFromHome(x, y);
        bool isSuccess = _random.NextDouble() < AccumulatedSuccessRate;
        if (!isSuccess)
        {
            LastResultMessage = $"開拓継続: ({x},{y}) 距離 {distanceFromHome} / 今回加算 {(int)(gainedRate * 100f)}% / 累積 {(int)(AccumulatedSuccessRate * 100f)}%";
            return;
        }

        mapData.MarkDeveloped(x, y);
        LastResultMessage = $"開拓成功: ({x},{y}) を開拓しました。距離 {distanceFromHome} / 累積 {(int)(AccumulatedSuccessRate * 100f)}%";
        AccumulatedSuccessRate = 0f;
        ConsecutiveInactiveTurns = 0;
        ClearSelectedTarget();
        _trackedTargetX = -1;
        _trackedTargetY = -1;
    }

    private void ResetAccumulationIfTargetChanged(int x, int y)
    {
        if (_trackedTargetX == x && _trackedTargetY == y)
        {
            return;
        }

        _trackedTargetX = x;
        _trackedTargetY = y;
        AccumulatedSuccessRate = 0f;
        ConsecutiveInactiveTurns = 0;
    }

    private void ApplyDecay()
    {
        if (AccumulatedSuccessRate <= 0f)
        {
            ConsecutiveInactiveTurns++;
            return;
        }

        ConsecutiveInactiveTurns++;
        float decayAmount = ConsecutiveInactiveTurns * GameConstants.DEVELOPMENT_INACTIVITY_DECAY_BASE;
        AccumulatedSuccessRate -= decayAmount;
        if (AccumulatedSuccessRate < 0f)
        {
            AccumulatedSuccessRate = 0f;
        }
    }

    private bool TryResolveTarget(MapData mapData, out int x, out int y)
    {
        if (HasSelectedTarget && mapData.IsValidDevelopmentTarget(_selectedTargetX, _selectedTargetY))
        {
            x = _selectedTargetX;
            y = _selectedTargetY;
            return true;
        }

        if (mapData.TryGetNextDevelopmentCandidate(out x, out y, out _))
        {
            return true;
        }

        x = -1;
        y = -1;
        return false;
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
