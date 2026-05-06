[System.Serializable]
public struct MaterialRequirement
{
    public MaterialType Type;
    public MaterialGrade MinimumGrade;
    public int Amount;

    public MaterialRequirement(MaterialType type, MaterialGrade minimumGrade, int amount)
    {
        Type = type;
        MinimumGrade = minimumGrade;
        Amount = amount;
    }
}
