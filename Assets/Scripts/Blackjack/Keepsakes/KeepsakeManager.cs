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

    public int GetDealerBustModifier()
    {
        if(equippedKeepsake == null) return 0;

        return equippedKeepsake.GetDealerBustModifier();
    }

    public bool AllowsEndlessDoubleDown()
    {
        if(equippedKeepsake == null) return false;

        return equippedKeepsake.AllowEndlessDoubleDown();
    }

    public bool AllowsOverdraft()
    {
        if(equippedKeepsake == null) return false;

        return equippedKeepsake.AllowOverdraft();
    }

    public int GetPassiveIncome()
    {
        if(equippedKeepsake == null) return 0;

        return equippedKeepsake.GetPassiveIncome();
    }

    public bool TryConsumeKeepsake()
    {
        if(equippedKeepsake == null) return false;

        bool isConsumed = equippedKeepsake.Consume();

        if(isConsumed)
        {
            equippedKeepsake = null;

            return true;
        }

        return false;
    }
}