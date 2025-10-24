using UnityEngine;

[CreateAssetMenu(fileName = "RankMultiplier", menuName = "Events/Set Rank Multiplier")]
public class RankMultiplierEvent : BlackjackEvent
{
    public Card.Rank rankToModify;

    [Tooltip("e.g., 2.0 for double, 0.5 for half")]
    public float multiplier;

    public override void Apply(BlackjackGame game)
    {
        game.SetRankMultiplier(rankToModify, multiplier);
    }
}