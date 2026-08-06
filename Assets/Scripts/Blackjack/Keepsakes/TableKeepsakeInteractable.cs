using UnityEngine;

public class TableKeepsakeInteractable : Clickable
{
    private BlackjackGame blackjackGame;
    public Keepsake keepsake { get; private set; }
    private bool usedThisRound = false;

    private void Awake()
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
        k.SetMembers(blackjackGame);
    }

    public void ResetUse()
    {
        usedThisRound = false;
    }

    private void OnDestroy()
    {
        blackjackGame.CursorDetection.RemoveRoundActiveClickable(this);
    }

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        if(!keepsake.isActive || usedThisRound) return;

        base.OnClick(mouseButton);
        bool activated = keepsake.ActivateTableEffect();

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

    private void CancelKeepsake()
    {
        if (keepsake.OnCancel())
        {
            keepsake.isActive = true;
            usedThisRound = false;
            IsActive = true;
        }
    }
    
    private void Update()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        CancelKeepsake();
    }
}