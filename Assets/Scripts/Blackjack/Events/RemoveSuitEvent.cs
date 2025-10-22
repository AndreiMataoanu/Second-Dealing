using UnityEngine;

[CreateAssetMenu(fileName = "Remove ", menuName = "Events/Remove Suit")]
public class RemoveSuitEvent : BlackjackEvent
{
    public Card.Suit suitToRemove;

    public override void Apply(BlackjackGame game)
    {
        game.RemoveSuitFromDeck(suitToRemove);
    }
}