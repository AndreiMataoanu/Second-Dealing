using UnityEngine;

[CreateAssetMenu(fileName = "Old Remove ", menuName = "Events/Old Remove Suit")]
public class OldRemoveSuitEvent : BlackjackEvent
{
    public Card.Suit suitToRemove;

    public override void Apply(EventManager events)
    {
        events.RemoveSuitFromDeck(suitToRemove);
        
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerRemoveEvent);
    }
}