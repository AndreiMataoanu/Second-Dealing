using UnityEngine;

[CreateAssetMenu(fileName = "AceRule", menuName = "Events/Set Ace Rule")]
public class AceRuleEvent : BlackjackEvent
{
    public AceValueRule ruleToSet;

    public override void Apply(EventManager events)
    {
        events.SetAceRule(ruleToSet);
    }
}