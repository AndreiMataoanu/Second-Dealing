using UnityEngine;

[CreateAssetMenu(fileName = "AddCards", menuName = "Events/Add Cards")]
public class AddCardsEvent : BlackjackEvent
{
    [Min(1)] [SerializeField] private int copyRangeMin;
    [Min(1)] [SerializeField] private int copyRangeMax;

    public override void Apply(EventManager events)
    {
        if (copyRangeMax < copyRangeMin)
            events.DisplayCardOptions(copyRangeMax, copyRangeMin);
        else
            events.DisplayCardOptions(copyRangeMin, copyRangeMax);

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerAddEvent);
    }
}
