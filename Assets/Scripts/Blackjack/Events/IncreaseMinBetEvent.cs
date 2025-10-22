using UnityEngine;

// This makes it so you can create this event from the Assets menu
[CreateAssetMenu(fileName = "MinBet ", menuName = "Events/Increase Min Bet")]
public class IncreaseMinBetEvent : BlackjackEvent
{
    public int increaseAmount = 50;

    public override void Apply(BlackjackGame game)
    {
        game.IncreaseMinimumBet(increaseAmount);
    }
}