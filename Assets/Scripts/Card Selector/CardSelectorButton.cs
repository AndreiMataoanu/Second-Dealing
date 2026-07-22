using UnityEngine;
using UnityEngine.Events;

public class CardSelectorButton : Clickable
{
    [SerializeField] private UnityEvent onClickAction;
    [SerializeField] private bool isDummyButton = false;

    private void Awake()
    {
        CursorDetection cursorDetection = FindFirstObjectByType<CursorDetection>();

        if(cursorDetection != null)
        {
            cursorDetection.AddRoundActiveClickable(this);
        }
    }

    private void OnDestroy()
    {
        CursorDetection cursorDetection = FindFirstObjectByType<CursorDetection>();

        if(cursorDetection != null)
        {
            cursorDetection.RemoveRoundActiveClickable(this);
        }
    }

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        base.OnClick(mouseButton);

        if(isDummyButton)
        {
            AudioManager.instance.Play("ItemDeny");

            return;
        }

        onClickAction.Invoke();
    }
}