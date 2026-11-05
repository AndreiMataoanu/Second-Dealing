using UnityEngine;

[CreateAssetMenu(fileName = "OldNegativeSuit", menuName = "Events/Old Negative Suit")]
public class OldNegativeSuitEvent : BlackjackEvent
{
    [SerializeField] Card.Suit suitToTarget;

    public override void Apply(EventManager events)
    {
        events.SetNegativeSuit(suitToTarget);
        
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerNegativeEvent);
    }
}