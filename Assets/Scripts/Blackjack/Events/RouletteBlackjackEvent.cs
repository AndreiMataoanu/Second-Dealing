using UnityEngine;

[CreateAssetMenu(fileName = "RouletteBlackjack", menuName = "Events/Roulette Blackjack")]
public class RouletteBlackjackEvent : BlackjackEvent
{
    public override void Apply(EventManager events)
    {
        events.SetRouletteBlackjackActive(true);
    }
}
