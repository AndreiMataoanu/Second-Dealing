using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BloodRose", menuName = "Keepsakes/Blood Rose")]
public class BloodRose : Keepsake
{
    [Tooltip("The multiplier applied to your original payout.")]
    public int payoutMultiplier = 2;

    public override int ModifyHandPayout(int originalPayout, List<CardInstance> currentHand)
    {
        if(currentHand == null || currentHand.Count == 0) return originalPayout;

        bool isPureRed = true;

        foreach(var cardInstance in currentHand)
        {
            if(!cardInstance.cardData.IsRedSuit())
            {
                isPureRed = false;

                break;
            }
        }

        if(isPureRed) return originalPayout * payoutMultiplier;

        return originalPayout;
    }

    public override bool IsConditionMet(List<List<CardInstance>> allHands)
    {
        if(allHands == null || allHands.Count == 0) return false;

        foreach(var hand in allHands)
        {
            if(hand.Count == 0) continue;

            bool isPureRed = true;

            foreach(var cardInstance in hand)
            {
                if(!cardInstance.cardData.IsRedSuit())
                {
                    isPureRed = false;

                    break;
                }
            }

            if(isPureRed) return true;
        }

        return false;
    }
}