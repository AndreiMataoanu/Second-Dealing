using UnityEngine;
using System.Collections.Generic;

public abstract class Keepsake : ScriptableObject
{
    public string keepsakeName;
    [TextArea] public string description;
    public GameObject tablePrefab;

    public virtual void OnRoundStart()
    {
    }

    public virtual bool TryActivateTableEffect(BlackjackGame game)
    {
        return false;
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

    public virtual bool AllowOverdraft()
    {
        return false;
    }

    public virtual int GetPassiveIncome()
    {
        return 0;
    }

    public virtual bool Consume()
    {
        return false;
    }

    public virtual bool AddsTarotCards()
    {
        return false;
    }
}