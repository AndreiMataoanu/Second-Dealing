using UnityEngine;

[CreateAssetMenu(fileName = "AddJokers", menuName = "Events/Add Jokers")]
public class AddJokersEvent : BlackjackEvent
{
    public override void Apply(BlackjackGame game)
    {
        game.AddJokers();
    }
}