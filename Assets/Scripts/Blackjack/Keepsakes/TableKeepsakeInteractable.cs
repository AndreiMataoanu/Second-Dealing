using UnityEngine;

public class TableKeepsakeInteractable : Clickable
{
    private BlackjackGame blackjackGame;
    private Keepsake keepsake;

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

        base.OnClick(mouseButton);
        bool activated = keepsake.ActivateTableEffect(blackjackGame);

        if(!activated)
        {
            AudioManager.instance.Play("ItemDeny");
        }
    }
}