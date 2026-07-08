using UnityEngine;

[CreateAssetMenu(fileName = "HalfHigh", menuName = "Events/Half High")]
public class HalfHighEvent : BlackjackEvent
{
    public override void Apply(EventManager events)
    {
        events.SetHalfHighActive(true);
    }
}