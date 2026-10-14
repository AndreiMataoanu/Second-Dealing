using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Remove Cards", menuName = "Events/Remove Cards")]
public class RemoveCardsEvent : BlackjackEvent
{
    public override IEnumerator GiveChoiceToPlayer(GameCamera gameCamera, CardChoiceEvent cardChoiceEvent)
    {
        var removeCardsEvent = cardChoiceEvent as RemoveCardChoiceEvent;
        removeCardsEvent?.SetDartsActive(true);
        
        gameCamera.ChangeToCamera(CameraType.Playing);

        yield return new WaitForSeconds(1.5f);
        
        removeCardsEvent?.DealOptions();
    }
    
    public override void ExplainChoiceDialogue(DialogueSystem dialogue)
    {
        dialogue.ShowRemoveCardsText();
    }

    public override void Apply(EventManager events) {}
}
