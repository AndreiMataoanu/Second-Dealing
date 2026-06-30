using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "777", menuName = "Keepsakes/777")]
public class SevenSevenSeven : Keepsake
{
    [Tooltip("Number of same value cards needed to trigger the multiplier.")]
    public int requiredMatches = 3;

    [Tooltip("The multiplier applied to your original payout.")]
    public int payoutMultiplier = 3;

    private int timesTriggered = 0;

    private void OnEnable()
    {
        timesTriggered = 0;
    }

    public override void OnRoundStart()
    {
        timesTriggered = 0;
    }

    public override int ModifyPayout(int originalPayout, List<List<CardInstance>> allHands)
    {
        if(allHands == null || allHands.Count == 0) return originalPayout;

        var allCards = allHands.SelectMany(hand => hand).ToList();

        if(allCards.Count < requiredMatches) return originalPayout;

        int totalMatches = 0;
        var groups = allCards.GroupBy(c => c.cardData.GetValue());

        foreach(var group in groups)
        {
            totalMatches += group.Count() / requiredMatches;
        }

        int unspentMatches = totalMatches - timesTriggered;


        if(unspentMatches > 0)
        {
            timesTriggered += unspentMatches;

            int finalMultiplier = payoutMultiplier * unspentMatches;

            return originalPayout * finalMultiplier;
        }

        return originalPayout;
    }

    public override bool IsConditionMet(List<List<CardInstance>> allHands)
    {
        if(allHands == null || allHands.Count == 0) return false;

        var allCards = allHands.SelectMany(hand => hand).ToList();

        if(allCards.Count < requiredMatches) return false;

        var groups = allCards.GroupBy(c => c.cardData.GetValue());

        foreach(var group in groups)
        {
            if(group.Count() >= requiredMatches)
            {
                return true;
            }
        }

        return false;
    }
}