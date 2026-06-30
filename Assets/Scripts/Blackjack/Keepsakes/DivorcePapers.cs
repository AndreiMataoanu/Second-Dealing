using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DivorcePapers", menuName = "Keepsakes/DivorcePapers")]
public class DivorcePapers : Keepsake
{
    public override bool AllowAnySplit()
    {
        return true;
    }

    public override bool IsConditionMet(List<List<CardInstance>> allHands)
    {
        if(allHands == null || allHands.Count == 0) return false;

        foreach(var hand in allHands)
        {
            if(hand.Count == 2)
            {
                return true;
            }
        }

        return false;
    }
}
