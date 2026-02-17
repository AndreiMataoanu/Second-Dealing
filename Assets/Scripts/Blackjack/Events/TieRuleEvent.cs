using UnityEngine;

[CreateAssetMenu(fileName = "TieRule", menuName = "Events/Set Tie Rule")]
public class SetTieRuleEvent : BlackjackEvent
{
    public bool dealerWillWinTies = true;

    public override void Apply(BlackjackGame game)
    {
        game.SetDealerWinsTies(dealerWillWinTies);
    }
}