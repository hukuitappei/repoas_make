using UnityEngine;

[CreateAssetMenu(fileName = "EventData", menuName = "repoas/EventData")]
public class EventData : ScriptableObject
{
    public string eventId;
    public string displayName;
    public string description;
    public float baseProbability;
    public int priority;
    public EventEffect[] effects;
    public string[] prerequisiteConditions;
    public EventChoice[] choices;
}
