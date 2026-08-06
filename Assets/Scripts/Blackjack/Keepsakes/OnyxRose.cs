using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OnyxRose", menuName = "Keepsakes/Onyx Rose")]
public class OnyxRose : Keepsake
{
    [Tooltip("The multiplier applied to your original payout.")]
    public int payoutMultiplier = 2;
    private int timesTriggered = 0;

    public override void OnRoundStart()
    {
        timesTriggered = 0;
    }

    public override int ModifyPayout(int originalPayout, List<List<CardInstance>> allHands)
    {
        if(allHands == null || allHands.Count == 0) return originalPayout;

        if(CountBlackHands(allHands) - timesTriggered > 0)
        {
            timesTriggered++;

            return originalPayout * payoutMultiplier;
        }

        return originalPayout;
    }

    public override bool IsConditionMet(List<List<CardInstance>> allHands)
    {
        if(allHands == null || allHands.Count == 0) return false;

        return CountBlackHands(allHands) > 0;
    }

    private int CountBlackHands(List<List<CardInstance>> allHands)
    {
        int count = 0;

        foreach(var hand in allHands)
        {
            if(hand.Count == 0) continue;

            bool isPureBlack = true;

            foreach(var cardInstance in hand)
            {
                if(!cardInstance.cardData.IsBlackSuit())
                {
                    isPureBlack = false;

                    break;
                }
            }

            if(isPureBlack)
            {
                count++;
            }
        }

        return count;
    }
}