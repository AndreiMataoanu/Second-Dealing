using UnityEngine;

[CreateAssetMenu(fileName = "DealerRage", menuName = "Events/DealerRage")]
public class DealerRage : BlackjackEvent
{
    public override void Apply(EventManager events)
    {
        events.SetDealerRageActive(true);
    }
    
}
