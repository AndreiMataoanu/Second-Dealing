using UnityEngine;

[CreateAssetMenu(fileName = "AddCards", menuName = "Events/Add Cards")]
public class AddCardsEvent : BlackjackEvent
{
    [Min(0)] [SerializeField] private int copyRangeMin;
    [Min(0)] [SerializeField] private int copyRangeMax;

    public override void Apply(BlackjackGame game)
    {
        if (copyRangeMax < copyRangeMin)
            game.DisplayCardOptions(copyRangeMax, copyRangeMin);
        else
            game.DisplayCardOptions(copyRangeMin, copyRangeMax);
    }
}
