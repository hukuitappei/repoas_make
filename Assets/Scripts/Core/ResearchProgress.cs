public class ResearchProgress
{
    private float _accumulatedProgress;

    public ResearchNodeData NodeData { get; private set; }
    public int RemainingTurns { get; private set; }

    public ResearchProgress(ResearchNodeData nodeData)
    {
        NodeData = nodeData;
        RemainingTurns = nodeData != null ? nodeData.researchDurationTurns : 0;
    }

    public void Advance(int researchSpeedPercentBonus)
    {
        _accumulatedProgress += 1.0f + researchSpeedPercentBonus / 100.0f;
        int progress = (int)_accumulatedProgress;
        _accumulatedProgress -= progress;
        RemainingTurns -= progress;
    }

    public bool IsCompleted()
    {
        return RemainingTurns <= 0;
    }
}
