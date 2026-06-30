using System.Collections.Generic;
using UnityEngine;

public class KeepsakeInteractable : Clickable
{
    [SerializeField] private Keepsake keepsake;
    [SerializeField] private BlackjackGame blackjackGame;
    [SerializeField] private GameObject glowVisuals;

    private void Start()
    {
        glowVisuals.SetActive(false);
    }

    private void Update()
    {
        Glow();
    }

    //temp, its a glowing object that shows if the keepsake condition is met
    private void Glow()
    {
        if(glowVisuals == null || blackjackGame == null || keepsake == null) return;

        bool isEquipped = KeepsakeManager.instance.equippedKeepsake == keepsake;

        if(!isEquipped)
        {
            glowVisuals.SetActive(false);

            return;
        }

        List<List<CardInstance>> hands = blackjackGame.GetPlayerHands();

        bool conditionMet = keepsake.IsConditionMet(hands);

        glowVisuals.SetActive(conditionMet);
    }

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        base.OnClick(mouseButton);

        if(keepsake != null && KeepsakeManager.instance != null)
        {
            KeepsakeManager.instance.equippedKeepsake = keepsake;
            AudioManager.instance.Play("ItemBuy");
        }
    }

    protected override string GetTooltipHeader()
    {
        return keepsake.name;
    }

    protected override string GetTooltipContent()
    {
        return keepsake.description;
    }
}