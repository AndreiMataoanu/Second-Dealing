using UnityEngine;

[CreateAssetMenu(fileName = "DoubleLow", menuName = "Events/Double Low")]
public class DoubleLowEvent : BlackjackEvent
{
    public override void Apply(EventManager events)
    {
        events.SetDoubleLowActive(true);
    }
}