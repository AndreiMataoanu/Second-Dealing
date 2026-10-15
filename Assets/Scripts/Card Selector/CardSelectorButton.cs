using UnityEngine;
using UnityEngine.Events;

public class CardSelectorButton : Clickable
{
    [SerializeField] private UnityEvent onClickAction;
    [SerializeField] private bool isDummyButton = false;

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        base.OnClick(mouseButton);

        if(isDummyButton)
        {
            AudioManager.instance.Play("ItemDeny"); //DELETE LATER
            AudioManagerFMOD.Play(AudioEvent.sfxCantUseItem);
            return;
        }

        AudioManager.instance.Play("BetUp"); //DELETE LATER
        AudioManagerFMOD.Play(AudioEvent.sfxIncreasingBet);

        onClickAction.Invoke();
    }
}