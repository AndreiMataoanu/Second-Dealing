using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NegativeSuit", menuName = "Events/Negative Suit")]
public class NegativeSuitEvent : BlackjackEvent
{
    public override IEnumerator GiveChoiceToPlayer(GameCamera gameCamera, CardChoiceEvent cardChoiceEvent)
    {
        var negativeSuitEvent = cardChoiceEvent as NegativeSuitChoiceEvent;
        negativeSuitEvent?.SetDartsActive(true);
        
        gameCamera.ChangeToCamera(CameraType.Playing);
        
        yield return new WaitForSeconds(1.5f);

        negativeSuitEvent?.DealOptions();
    }
    
    public override void ExplainChoiceDialogue(DialogueSystem dialogue)
    {
        dialogue.ShowNegativeSuitText();
    }

    public override void Apply(EventManager events)
    {
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerNegativeEvent);
    }
}
