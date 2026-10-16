using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RemoveSuit", menuName = "Events/Remove Suit")]
public class RemoveSuitEvent : BlackjackEvent
{
    public override IEnumerator GiveChoiceToPlayer(GameCamera gameCamera, CardChoiceEvent cardChoiceEvent)
    {
        var negativeSuitEvent = cardChoiceEvent as RemoveSuitChoiceEvent;
        negativeSuitEvent?.SetDartsActive(true);
        
        gameCamera.ChangeToCamera(CameraType.Playing);
        
        yield return new WaitForSeconds(1.5f);

        negativeSuitEvent?.DealOptions();
    }
    
    public override void ExplainChoiceDialogue(DialogueSystem dialogue)
    {
        dialogue.ShowRemoveSuitText();
    }

    public override void Apply(EventManager events)
    {
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerRemoveEvent);
    }
}
