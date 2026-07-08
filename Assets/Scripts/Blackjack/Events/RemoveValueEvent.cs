using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Remove ", menuName = "Events/Remove Value")]
public class RemoveValueEvent : BlackjackEvent
{
    [SerializeField] Card.Rank valueToRemove;

    public override void Apply(EventManager events)
    {
        int targetValue = GetRankValue(valueToRemove);

        foreach(Card.Rank r in Enum.GetValues(typeof(Card.Rank)))
        {
            if(r == Card.Rank.None || r == Card.Rank.Joker) continue;

            if(GetRankValue(r) == targetValue)
            {
                events.RemoveValueFromDeck(r);
            }
        }

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.TriggerRemoveEvent);
    }

    private int GetRankValue(Card.Rank rank)
    {
        if(rank >= Card.Rank.Ten && rank <= Card.Rank.King)
        {
            return 10;
        }

        return (int)rank;
    }
}