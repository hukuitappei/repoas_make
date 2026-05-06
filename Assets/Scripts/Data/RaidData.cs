using UnityEngine;

[CreateAssetMenu(fileName = "RaidData", menuName = "repoas/RaidData")]
public class RaidData : ScriptableObject
{
    public string raidId;
    public string displayName;
    public int earliestTurn;
    public int baseEnemyPower;
    public int exploredEnemyPower;
    public bool isForced;
    public int priority;
}
