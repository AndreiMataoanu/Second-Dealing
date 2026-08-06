using UnityEngine;

public class TableKeepsakeInteractable : Clickable
{
    [SerializeField] private Material outlineUse;
    [SerializeField] private Material outlineCantUse;
    private BlackjackGame blackjackGame;
    public Keepsake keepsake { get; private set; }
    private bool usedThisRound = false;

    protected override void Awake()
    {
        base.Awake();

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

        bool isUsable = true;

        if(!keepsake.isActive || usedThisRound)
        {
            isUsable = false;
        }
        else if(blackjackGame.isActionLocked && !blackjackGame.UseAfterStand)
        {
            isUsable = false;
        }

        if(!isUsable)
        {
            AudioManager.instance.Play("ItemDeny");

            return;
        }

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

    protected override Material GetOutlineMaterial()
    {
        if(!keepsake.isActive || usedThisRound)
        {
            return outlineCantUse;
        }

        if(blackjackGame.isActionLocked)
        {
            if(blackjackGame.UseAfterStand)
            {
                return outlineUse;
            }

            return outlineCantUse;
        }

        return outlineUse;
    }

    protected override string GetTooltipContent()
    {
        return $"\n{keepsake.GetDescription()}";
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