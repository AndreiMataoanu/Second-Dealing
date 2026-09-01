using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "AddCards", menuName = "Events/Add Cards")]
public class AddCardsEvent : BlackjackEvent
{
    [Min(1)] [SerializeField] private int copyRangeMin;
    [Min(1)] [SerializeField] private int copyRangeMax;

    private void OnEnable()
    {
        if (copyRangeMax < copyRangeMin)
            (copyRangeMax, copyRangeMin) = (copyRangeMin, copyRangeMax);
    }

    public override IEnumerator GiveChoiceToPlayer(GameCamera gameCamera, CardChoiceEvent cardChoiceEvent)
    {
        var addCardsEvent = cardChoiceEvent as AddCardChoiceEvent;
        addCardsEvent?.SetAddCardsEvent(this);
        addCardsEvent?.SetDartsActive(true);
        
        gameCamera.ChangeToCamera(CameraType.Playing);
        
        yield return new WaitForSeconds(1.5f);

        addCardsEvent?.DealOptions();
    }

    public override void ExplainChoiceDialogue(DialogueSystem dialogue)
    {
        dialogue.ShowAddCardsText();
    }

    public override void Apply(EventManager events)
    {
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerAddEvent);
    }

    public int GenerateCopyCount() => Random.Range(copyRangeMin, copyRangeMax + 1);
}
