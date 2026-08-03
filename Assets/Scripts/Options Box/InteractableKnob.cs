using UnityEngine;
using UnityEngine.Events;

public class InteractableKnob : Clickable
{
    [SerializeField] private UnityEvent onLeftClick;
    [SerializeField] private UnityEvent onRightClick;

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        if(mouseButton == 0)
        {
            onLeftClick?.Invoke();
        }
        else if(mouseButton == 1)
        {
            onRightClick?.Invoke();
        }

        OnRemoveOutline();
    }
}