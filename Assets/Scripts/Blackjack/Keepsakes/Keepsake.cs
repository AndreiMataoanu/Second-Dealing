using UnityEngine;
using System.Collections.Generic;

public enum ChallengeType
{
    None,
    LoseByOne,
    TriggerNegativeEvent,
    TriggerRemoveEvent,
    TriggerAddEvent,
    ItemAfterStand,
    AlterDealerHand,
    WinRedSuits,
    WinBlackSuits,
    UseItems,
    Split,
    DoubleDown,
    DoubleDownAndSplit,
    ThreeOfAKind,
    CompleteRound
}

public abstract class Keepsake : ScriptableObject
{
    public string keepsakeName;
    [TextArea] public string description;
    [TextArea] public string unlockDescription;
    public ChallengeType requiredChallenge = ChallengeType.None;
    public int requiredTarget = 0;
    public GameObject tablePrefab;
    public bool isActive = false;

    public virtual void OnRoundStart()
    {
    }

    public virtual void SetMembers(BlackjackGame blackjackGame)
    {
    }

    public virtual bool ActivateTableEffect(BlackjackGame game)
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

    public virtual bool AddTarotCards()
    {
        return false;
    }

    public virtual bool AllowPostStandItem(BlackjackGame game)
    {
        return false;
    }
}