using System.Collections;
using UnityEngine;
using Utils;

public abstract class BlackjackEvent : ScriptableObject
{
    public enum EventSeverity { Low, Medium, High }
    public string eventName;
    public EventSeverity severity;

    [HideInInspector] public bool wasTriggered;

    public IEnumerator StartDisplay(GameCamera gameCamera, TMPro.TextMeshProUGUI statusText)
    {
        gameCamera.ChangeToCamera(CameraType.Event);
        AudioManager.instance.Play("Laugh");
        statusText.text = "Let's make it more interesting.";
        yield return GameUtils.WaitDelayOrInput(5.0f);

        statusText.text = $"New Event: {eventName}";
        yield return GameUtils.WaitDelayOrInput(5.0f);
    }

    public virtual IEnumerator GiveChoiceToPlayer(GameCamera gameCamera, CardChoiceEvent cardChoiceEvent)
    {
        yield return null;
    }

    public virtual void ExplainChoiceDialogue(DialogueSystem dialogue) {}

    public void EndDisplay(GameCamera gameCamera) => gameCamera.ChangeToCamera(CameraType.Sitting);
    
    public abstract void Apply(EventManager events);
    
    public virtual void ExplainEventDialogue(DialogueSystem dialogue) {}
}
