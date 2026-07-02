using UnityEngine;

public class TableKeepsakeInteractable : Clickable
{
    private BlackjackGame blackjackGame;

    private void Start()
    {
        blackjackGame = FindFirstObjectByType<BlackjackGame>();

        var cursorDetection = FindFirstObjectByType<CursorDetection>();

        if(cursorDetection != null)
        {
            cursorDetection.AddRoundActiveClickable(this);
        }

        if(KeepsakeManager.instance != null && KeepsakeManager.instance.equippedKeepsake != null)
        {
            tooltipHeader = KeepsakeManager.instance.equippedKeepsake.keepsakeName;
        }

        SetActive(false);
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

        if(KeepsakeManager.instance != null && KeepsakeManager.instance.equippedKeepsake != null)
        {
            bool activated = KeepsakeManager.instance.equippedKeepsake.TryActivateTableEffect(blackjackGame);

            if(!activated)
            {
                AudioManager.instance.Play("ItemDeny");
            }
        }
    }
}