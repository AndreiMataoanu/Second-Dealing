using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "AddCards", menuName = "Events/Add Cards")]
public class AddCardsEvent : BlackjackEvent
{
    [Min(1)] [SerializeField] private int copyRangeMin;
    [Min(1)] [SerializeField] private int copyRangeMax;

    private int copyCount;

    public int CopyCount => copyCount;

    private void OnEnable()
    {
        if (copyRangeMax < copyRangeMin)
            (copyRangeMax, copyRangeMin) = (copyRangeMin, copyRangeMax);
    }

    public override IEnumerator GiveChoiceToPlayer(GameCamera gameCamera, CardChoiceEvent cardChoiceEvent)
    {
        copyCount = Random.Range(copyRangeMin, copyRangeMax + 1);
        gameCamera.ChangeToCamera(CameraType.Playing);
        
        yield return new WaitForSeconds(1.5f);

        var addCardsEvent = cardChoiceEvent as AddCardChoiceEvent;
        addCardsEvent?.DealOptions();
        addCardsEvent?.SetAddCardsEvent(this);
    }

    public override void ExplainChoiceDialogue(DialogueSystem dialogue)
    {
        dialogue.ShowAddCardsText(copyCount);
    }

    public override void Apply(EventManager events)
    {
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerAddEvent);
    }
}
