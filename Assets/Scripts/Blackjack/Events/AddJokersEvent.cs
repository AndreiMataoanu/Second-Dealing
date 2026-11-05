using UnityEngine;

[CreateAssetMenu(fileName = "AddJokers", menuName = "Events/Add Jokers")]
public class AddJokersEvent : BlackjackEvent
{
    public override void Apply(EventManager events)
    {
        events.AddJokers();

        AudioManagerFMOD.Play(AudioEvent.sfxJokerEvent);
        
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerAddEvent);
    }
}