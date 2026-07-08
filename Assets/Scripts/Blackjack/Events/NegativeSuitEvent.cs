using UnityEngine;

[CreateAssetMenu(fileName = "NegativeSuit", menuName = "Events/Negative Suit")]
public class NegativeSuitEvent : BlackjackEvent
{
    [SerializeField] Card.Suit suitToTarget;

    public override void Apply(EventManager events)
    {
        events.SetNegativeSuit(suitToTarget);
        
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerNegativeEvent);
    }
}