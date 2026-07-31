using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Progress
{
    public class MilestoneProgress : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private EventManager eventManager;
        [SerializeField] private bool useTurnLimit = false;
        [SerializeField] private List<Milestone> milestones;
        [SerializeField] private List<BlackjackEvent> randomEvents;

        [Header("Display Milestone Progress")] 
        [SerializeField] private GameCamera gameCamera;
        [SerializeField] private TMPro.TextMeshProUGUI statusText;
        [SerializeField] private ProgressDisplay progressDisplay;

        [Header("Dialogue")] 
        [SerializeField] private DialogueSystem dialogueSystem;
        
        private IEnumerator milestoneTriggerCoroutine;
        
        private Milestone nextMilestone;
        private int currentMaxTurns;
        private int currentTurns;

        private BlackjackGame blackjackGame;

        private int TurnsLeft => currentMaxTurns - currentTurns;
        
        #region Monobehaviour
        
        private void Start()
        {
            InitRandomMilestones();

            nextMilestone = milestones.First();
            currentMaxTurns = nextMilestone.maxTurns;
            currentTurns = 0;

            UpdateProgressDisplay();
        }
        
        #endregion
        
        #region Setup

        public void SetBlackjackGame(BlackjackGame game) => blackjackGame = game;
        
        private void InitRandomMilestones()
        {
            randomEvents.Shuffle();
            
            foreach (var milestone in milestones)
            {
                if (milestone.milestoneType == MilestoneType.RandomEvent)
                {
                    if (randomEvents.Count == 0)
                    {
                        Debug.Log("Not enough random events");
                        return;
                    }
                    
                    milestone.gameEvent = randomEvents.First();
                    randomEvents.RemoveAt(0);
                }
            }
        }

        #endregion

        #region Update Display
        
        private void UpdateProgressDisplay()
        {
            if (nextMilestone == null)
            {
                progressDisplay.UpdateLastEvent();
                return;
            }

            if (useTurnLimit) 
                progressDisplay.DisplayNextMilestone(nextMilestone.moneyAmount, TurnsLeft);
            else
                progressDisplay.DisplayNextMilestone(nextMilestone.moneyAmount);
            
            progressDisplay.UpdatePowerballGoal(eventManager.PowerballGoal);
        }
        
        #endregion

        #region Check Milestone Progress

        public IEnumerator ShowTurnLimitDialogue()
        {
            if (!useTurnLimit || currentTurns < currentMaxTurns) yield break;
            
            dialogueSystem.ShowTurnLimitTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            
            SceneManager.LoadSceneAsync(2);
        }

        public IEnumerator UpdateMilestoneProgress()
        {
            yield return StartCoroutine(UpdateProgressCoroutine());
        }
        
        private IEnumerator UpdateProgressCoroutine()
        {
            UpdateTurnsLeft();
            
            var passedMilestone = GoToNextMilestone();
            if(passedMilestone == null) yield break;

            UpdateKeepsakes();
            
            UpdateProgressDisplay();

            yield return eventManager.TriggerEvent(passedMilestone.gameEvent);
        }

        #endregion

        #region Helpers

        private void UpdateTurnsLeft()
        {
            if (!useTurnLimit) return;
            
            currentTurns++;
            // Debug.Log("update turns");
            UpdateProgressDisplay();
        }
        
        private Milestone GoToNextMilestone()
        {
            if (milestones == null || milestones.Count == 0) return null;
                
            var milestone = milestones.First();
            if (blackjackGame.TargetMoneyBalance >= milestone.moneyAmount)
            {
                milestones.RemoveAt(0);

                UpdateNextMilestone();
                
                return milestone;
            }

            return null;
        }
        
        private void UpdateNextMilestone()
        {
            nextMilestone = milestones.Count == 0 ? null : milestones.First();
            if (nextMilestone == null) return;

            currentMaxTurns = nextMilestone.maxTurns;
            currentTurns = 0;
        }
        
        private void UpdateKeepsakes()
        {
            KeepsakeManager.instance.RechargeSecondDealing();
        }

        private IEnumerator DisplayNextMilestone(Milestone milestone)
        {
            if (milestone == null) yield break;
            
            StartCoroutine(milestone.gameEvent.StartDisplay(gameCamera, statusText));
        }

        #endregion
    }
}