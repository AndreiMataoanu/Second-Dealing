using UnityEngine;
using System.Collections.Generic;

public class KeepsakeManager : MonoBehaviour
{
    public static KeepsakeManager instance;

    public Keepsake equippedKeepsake;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int ApplyPayoutModifiers(int payout, List<List<CardInstance>> allHands)
    {
        if(equippedKeepsake == null) return payout;

        return equippedKeepsake.ModifyPayout(payout, allHands);
    }

    public void ResetKeepsake()
    {
        if(equippedKeepsake != null)
        {
            equippedKeepsake.OnRoundStart();
        }
    }

    public bool AllowsAnySplit()
    {
        if(equippedKeepsake == null) return false;

        return equippedKeepsake.AllowAnySplit();
    }
}