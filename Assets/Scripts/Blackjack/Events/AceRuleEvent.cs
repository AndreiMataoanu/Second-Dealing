using UnityEngine;

[CreateAssetMenu(fileName = "AceRule", menuName = "Events/Set Ace Rule")]
public class AceRuleEvent : BlackjackEvent
{
    public BlackjackGame.AceValueRule ruleToSet;

    public override void Apply(BlackjackGame game)
    {
        game.SetAceRule(ruleToSet);
    }
}