using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Powerball", menuName = "Events/Powerball")]
public class PowerballEvent : BlackjackEvent
{
    public override void Apply(BlackjackGame game)
    {
        List<int> numbers = GenerateNumbers();
        game.SetPowerballEventActive(numbers);
    }

    public static List<int> GenerateNumbers()
    {
        List<int> numbers = new List<int>();

        // for (int i = 0; i < 3; i++)
        //     numbers.Add(Random.Range(2, 34));

        numbers.Add(20);
        numbers.Add(20);
        numbers.Add(10);
        
        return numbers;
    }
}