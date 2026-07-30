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
    private int index = 0;

    private void AddThresholds()
    {
        foreach (var threshold in eventManager.EventThresholds)
            if (threshold != null) 
                eventThreshHolds.Add(threshold.moneyAmount);
    }

    private int GetNextThreshold()
    {
        foreach(int threshold in eventThreshHolds)
        {
            if(threshold > blackjackGame.PlayerMoney)
            {
                return threshold;
            }
        }

        return -1;
    }

    public void DisplayNextMilestone()
    {
        if(eventManager.TriggeredThresholdsCount > index)
        {
            index = eventManager.TriggeredThresholdsCount;

            foreach(var keepsake in KeepsakeManager.instance.equippedKeepsakes)
            {
                if(keepsake is SecondDealing secondDealing)
                {
                    secondDealing.Recharge();
                }
            }
        }

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

        index = eventManager.TriggeredThresholdsCount;

        DisplayNextMilestone();
    }
}