using UnityEngine;

[CreateAssetMenu(fileName = "AltBlackjack", menuName = "Events/Set Alternate Blackjack")]
public class AlternateBlackjackEvent : BlackjackEvent
{
    [Tooltip("The hand value that will also count as a Blackjack.")]
    public int valueToCount = 1;

    public override void Apply(BlackjackGame game)
    {
        game.SetAlternateBlackjackValue(valueToCount);
    }
}