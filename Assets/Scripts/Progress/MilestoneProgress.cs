using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utils;

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
        [SerializeField] public UnityEvent ChangeProgressText;

        [Header("Dialogue")] 
        [SerializeField] private DialogueSystem dialogueSystem;
        
        private IEnumerator eventTriggerCoroutine;
        
        private int targetMoneyBalance;
        private int triggeredEventsCount = 0; // is it needed?
        private int currentMaxTurns;
        private int currentTurns;

        private bool milestoneTriggered = false;
        
        private BlackjackGame blackjackGame;

        private void Awake()
        {
            InitRandomMilestones();
            InitCardsEventActions();
            
            currentMaxTurns = milestones.First().maxTurns;
            currentTurns = 0;
        }
        
        public void UpdateTurnsLeft()
        {
            if (!useTurnLimit) return;
            
            currentTurns++;
            ChangeProgressText.Invoke();
        }

        public IEnumerator CheckTurnLimit()
        {
            if (!useTurnLimit || currentTurns < currentMaxTurns) yield break;
            
            blackjackGame.DialogueSystem.ShowTurnLimitTaunt();

            yield return new WaitWhile(() => blackjackGame.DialogueSystem.IsPlaying);
            
            SceneManager.LoadSceneAsync(2);
        }

        public IEnumerator CheckForEventTrigger()
        {
            eventTriggerCoroutine = CheckForEventTriggerCoroutine();
            
            yield return StartCoroutine(eventTriggerCoroutine);
        }
        
        private Milestone GoToNextMilestone()
        {
            if (milestones == null || milestones.Count == 0) return null;
                
            var milestone = milestones.First();
            if (blackjackGame.TargetMoneyBalance >= milestone.moneyAmount)
            {
                milestoneTriggered = false;
                milestones.RemoveAt(0);
                return milestone;
            }

            return null;
        }
        
        private IEnumerator CheckForEventTriggerCoroutine()
        {
            Milestone nextMilestone;
            while(true) //TODO test without while
            {
                nextMilestone = GoToNextMilestone();
                if(nextMilestone == null) break;

                yield return DisplayNextMilestone(nextMilestone);

                yield return PresentPlayerChoice(nextMilestone);

                ExplainEventChoice(nextMilestone);
                
                ApplyEvent(nextMilestone);
                
                currentMaxTurns = nextMilestone.maxTurns;
                currentTurns = 0;
                
                // ChangeProgressText?.Invoke();
                // UpdatePowerballGoal?.Invoke();
                //
                //
                //
                // if (isPowerballTriggered)
                // {
                //     blackjackGame.DialogueSystem.PlayPowerballTutorial();
                //     isPowerballTriggered = false;
                // }
            }
        }

        private IEnumerator DisplayNextMilestone(Milestone nextMilestone)
        {
            if (milestoneTriggered) yield break;
            
            StartCoroutine(nextMilestone.gameEvent.StartDisplay(gameCamera, statusText));

            milestoneTriggered = true;
        }

        private IEnumerator PresentPlayerChoice(Milestone milestone)
        {
            if (milestoneTriggered) yield break;

            var playerChoice = milestone.gameEvent.GiveChoiceToPlayer(gameCamera, milestone.cardChoiceEvent);
            if (playerChoice == null) yield break;

            StartCoroutine(playerChoice);
            StopEventFlow();
        }

        private void ExplainEventChoice(Milestone milestone)
        {
            milestone.gameEvent.ExplainChoiceDialogue(dialogueSystem);
        }

        private void ApplyEvent(Milestone milestone)
        {
            milestone.gameEvent.Apply(eventManager);
        }

        #region Event Flow
        
        private void StopEventFlow()
        {
            StopCoroutine(eventTriggerCoroutine);
            // tableCards.ClearTable();
            blackjackGame.UpdateBettingUI();
            blackjackGame.ResetTexts();
        }
        
        #endregion

        #region Setup
        
        private void InitRandomMilestones()
        {
            randomEvents.Shuffle();
            
            foreach (var milestone in milestones)
            {
                if (milestone.eventType == EventType.Random)
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

        private void InitCardsEventActions()
        {
            foreach (var milestone in milestones)
            {
                milestone.cardChoiceEvent = eventManager.GetCardChoiceEvent(milestone.gameEvent);
            }
        }

        #endregion
    }
}