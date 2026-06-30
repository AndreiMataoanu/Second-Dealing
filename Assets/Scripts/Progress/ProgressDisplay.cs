using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProgressDisplay : MonoBehaviour
{
    [SerializeField] private BlackjackGame blackjackGame;

    private List<int> eventThreshHolds = new();
    private TMP_Text textComponent;
    
    private void AddThresholds()
    {
        foreach (var threshold in blackjackGame.EventThresholds)
            if (threshold != null) 
                eventThreshHolds.Add(threshold.moneyAmount);
    }

    private int GetNextThreshold()
    {
        int i = blackjackGame.TriggeredThresholdsCount;
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

        textComponent.text = "Next milestone:\n" + next + "$";
    }
    
    private void Start()
    {
        AddThresholds();
        textComponent = GetComponent<TMP_Text>();
        DisplayNextMilestone();
    }
}
