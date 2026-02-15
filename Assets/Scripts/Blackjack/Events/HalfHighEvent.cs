using UnityEngine;

[CreateAssetMenu(fileName = "HalfHigh", menuName = "Events/Half High")]
public class HalfHighEvent : BlackjackEvent
{
    public override void Apply(BlackjackGame game)
    {
        game.SetHalfHighActive(true);
    }
}