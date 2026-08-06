using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BloodRose", menuName = "Keepsakes/Blood Rose")]
public class BloodRose : Keepsake
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

        if(CountRedHands(allHands) - timesTriggered > 0)
        {
            timesTriggered++;

            return originalPayout * payoutMultiplier;
        }

        return originalPayout;
    }

    public override bool IsConditionMet(List<List<CardInstance>> allHands)
    {
        if(allHands == null || allHands.Count == 0) return false;

        return CountRedHands(allHands) > 0;
    }

    private int CountRedHands(List<List<CardInstance>> allHands)
    {
        int count = 0;

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

            if(isPureRed)
            {
                count++;
            }
        }

        return count;
    }
}