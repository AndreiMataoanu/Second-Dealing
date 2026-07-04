using UnityEngine;

public class TableKeepsakeInteractable : Clickable
{
    private BlackjackGame blackjackGame;
    private Keepsake keepsake;
    private bool usedThisRound = false;

    private void Start()
    {
        blackjackGame = FindFirstObjectByType<BlackjackGame>();

        var cursorDetection = FindFirstObjectByType<CursorDetection>();

        if(cursorDetection != null)
        {
            cursorDetection.AddRoundActiveClickable(this);
        }

        SetActive(false);
    }

    public void SetKeepsake(Keepsake k)
    {
        keepsake = k;
        tooltipHeader = k.keepsakeName;
    }

    public void ResetUse()
    {
        usedThisRound = false;
    }

    private void OnDestroy()
    {
        var cursorDetection = FindFirstObjectByType<CursorDetection>();

        if(cursorDetection != null)
        {
            cursorDetection.RemoveRoundActiveClickable(this);
        }
    }

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        if(!keepsake.isActive || usedThisRound) return;

        base.OnClick(mouseButton);
        bool activated = keepsake.ActivateTableEffect(blackjackGame);

        if(activated)
        {
            usedThisRound = true;

            OnRemoveOutline();
        }
        else
        {
            AudioManager.instance.Play("ItemDeny");
        }
    }

    public override void ApplyOutline()
    {
        if(!keepsake.isActive || usedThisRound) return;

        base.ApplyOutline();
    }
}