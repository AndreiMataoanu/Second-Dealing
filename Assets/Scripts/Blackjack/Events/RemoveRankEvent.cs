using UnityEngine;

[CreateAssetMenu(fileName = "Remove ", menuName = "Events/Remove Rank")]
public class RemoveRankEvent : BlackjackEvent
{
    public Card.Rank rankToRemove;

    public override void Apply(BlackjackGame game)
    {
        game.RemoveRankFromDeck(rankToRemove);
    }
}