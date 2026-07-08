using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProgressDisplay : MonoBehaviour
{
    [SerializeField] private BlackjackGame blackjackGame;

    private EventManager eventManager;
    private List<int> eventThreshHolds = new();
    private TMP_Text textComponent;
    private string defaultText;
    
    private void AddThresholds()
    {
        foreach (var threshold in eventManager.EventThresholds)
            if (threshold != null) 
                eventThreshHolds.Add(threshold.moneyAmount);
    }

    private int GetNextThreshold()
    {
        int i = eventManager.TriggeredThresholdsCount;
        return i < eventThreshHolds.Count ? eventThreshHolds[i] : -1;
    }

    public void DisplayNextMilestone()
    {
        var next = GetNextThreshold();
        if (next == -1)
        {
            textComponent.text = "There's nothing left.";
            return;
        }

        if (!eventManager.UseTurnLimit)
        {
            textComponent.text = "Next milestone:\n$" + next;
            defaultText = textComponent.text;
            return;
        }

        textComponent.text = "Next milestone:\n$" + next + "\n\nTurns left: " + eventManager.TurnsLeft;
        defaultText = textComponent.text;
    }

    public void UpdatePowerballGoal()
    {
        textComponent.text = defaultText;
        var goal = eventManager.PowerballGoal;
        if (goal.Count == 0) return;

        textComponent.text += "\n\nPowerball:\n";
        foreach (var number in goal)
            textComponent.text += number + " ";
    }
    
    private void Start()
    {
        eventManager = blackjackGame.EventManager;
        textComponent = GetComponent<TMP_Text>();
        AddThresholds();
        DisplayNextMilestone();
    }
}
