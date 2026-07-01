using UnityEngine;
using System.Collections.Generic;

public abstract class Keepsake : ScriptableObject
{
    public string keepsakeName;
    [TextArea] public string description;

    public virtual void OnRoundStart()
    {
    }

    public virtual int ModifyPayout(int originalPayout, List<List<CardInstance>> allHands)
    {
        return originalPayout;
    }

    public virtual bool IsConditionMet(List<List<CardInstance>> allHands)
    {
        return false;
    }

    public virtual bool AllowAnySplit()
    {
        return false;
    }

    public virtual int GetDealerBustModifier()
    {
        return 0;
    }

    public virtual bool AllowEndlessDoubleDown()
    {
        return false;
    }
}