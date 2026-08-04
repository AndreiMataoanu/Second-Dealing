using System.Collections;
using UnityEngine;
using Utils;

public abstract class BlackjackEvent : ScriptableObject
{
    public string eventName;

    public IEnumerator StartDisplay(GameCamera gameCamera, TMPro.TextMeshProUGUI statusText)
    {
        gameCamera.ChangeToCamera(CameraType.Event);
        AudioManager.instance.Play("Laugh");
        statusText.text = "Let's make it more interesting";
        yield return GameUtils.WaitDelayOrInput(5.0f);

        statusText.text = $"New Event: {eventName}";
        yield return GameUtils.WaitDelayOrInput(5.0f);
    }

    public virtual IEnumerator GiveChoiceToPlayer(GameCamera gameCamera, CardChoiceEvent cardChoiceEvent)
    {
        yield return null;
    }

    public virtual void ExplainChoiceDialogue(DialogueSystem dialogue) {}

    public IEnumerator EndDisplay(GameCamera gameCamera)
    {
        yield return GameUtils.WaitDelayOrInput(1.0f);
        
        gameCamera.ChangeToCamera(CameraType.Sitting);
    }

    public abstract void Apply(EventManager events);

    public virtual IEnumerator ExplainEventDialogue(DialogueSystem dialogue)
    {
        yield return null;
    }
}
