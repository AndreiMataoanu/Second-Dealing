using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProgressDisplay : MonoBehaviour
{
    private TMP_Text textComponent;
    private string defaultText;

    public void DisplayNextMilestone(int nextMilestone, int turnsLeft)
    {
        textComponent.text = "Next milestone:\n$" + nextMilestone + "\n\nTurns left: " + turnsLeft;
        defaultText = textComponent.text;
    }
    
    public void DisplayNextMilestone(int nextMilestone)
    {
        textComponent.text = "Next milestone:\n$" + nextMilestone;
        defaultText = textComponent.text;
    }

    public void UpdatePowerballGoal(List<int> goal)
    {
        textComponent.text = defaultText;
        if (goal == null || goal.Count == 0) return;

        textComponent.text += "\n\nPowerball:\n";
        foreach (var number in goal)
            textComponent.text += number + " ";
    }

    public void UpdateLastEvent()
    {
        textComponent.text = "Goal reached"; //test
    }
    
    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }
}