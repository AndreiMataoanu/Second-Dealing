using UnityEngine;

[CreateAssetMenu(fileName = "AddJokers", menuName = "Events/Add Jokers")]
public class AddJokersEvent : BlackjackEvent
{
    public override void Apply(EventManager events)
    {
        events.AddJokers();
        
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerAddEvent);
    }
}