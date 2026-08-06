using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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

        [Header("Systems")] 
        [SerializeField] private DialogueSystem dialogueSystem;
        [SerializeField] private CashOutSystem cashOutSystem;
        [SerializeField] private SelectMilestoneKeepsake selectKeepsake;
        
        private IEnumerator milestoneTriggerCoroutine;
        
        private Milestone nextMilestone;
        private int currentMaxTurns;
        private int currentTurns;

        private BlackjackGame blackjackGame;

        private int TurnsLeft => currentMaxTurns - currentTurns;
        
        #region Monobehaviour
        
        private void Start()
        {
            InitMilestones();

            nextMilestone = milestones.First();
            currentMaxTurns = nextMilestone.maxTurns;
            currentTurns = 0;

            UpdateProgressDisplay();
        }
        
        #endregion
        
        #region Setup

        public void SetBlackjackGame(BlackjackGame game)
        {
            blackjackGame = game;
            cashOutSystem.SetBlackjackGame(game);
            selectKeepsake.SetBlackjackGame(game);
        }

        private void InitMilestones()
        {
            randomEvents.Shuffle();
            
            foreach (var milestone in milestones)
            {
                switch (milestone.milestoneType)
                {
                    case MilestoneType.RandomEvent when randomEvents.Count == 0:
                        Debug.Log("Not enough random events");
                        return;
                    case MilestoneType.RandomEvent:
                        milestone.gameEvent = randomEvents.First();
                        randomEvents.RemoveAt(0);
                        break;
                    case MilestoneType.FinalGoal:
                        milestone.gameEvent = null;
                        break;
                }
            }
        }

        #endregion

        #region Update Display
        
        private void UpdateTurnsLeft()
        {
            if (!useTurnLimit) return;
            
            currentTurns++;
            UpdateProgressDisplay();
        }
        
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

        public IEnumerator UpdateMilestoneProgress()
        {
            yield return StartCoroutine(UpdateProgressCoroutine());
        }
        
        private IEnumerator UpdateProgressCoroutine()
        {
            UpdateTurnsLeft();
            
            var passedMilestone = GoToNextMilestone();
            if (passedMilestone == null) yield break;
            if (passedMilestone.milestoneType == MilestoneType.FinalGoal) useTurnLimit = false;
            else
            {
                blackjackGame.CursorDetection.SetAllInactive();
                blackjackGame.ShopManager.SetInventoryActive(false);
            }

            if(passedMilestone.milestoneType == MilestoneType.CanonEvent)
            {
                foreach(var keepsake in KeepsakeManager.instance.equippedKeepsakes)
                {
                    if(keepsake is TrustFund trustFund)
                    {
                        trustFund.ScaleIncome();
                    }
                }
            }

            UpdateEventKeepsakes();
            
            UpdateProgressDisplay();

            yield return eventManager.TriggerEvent(passedMilestone.gameEvent);

            yield return PresentKeepsakeChoice(passedMilestone);
            
            gameCamera.ChangeToCamera(CameraType.Sitting);
            
            blackjackGame.CursorDetection.OnRoundInactive();
            blackjackGame.ShopManager.SetInventoryActive(true);
        }

        public IEnumerator OnEndProgressUpdate()
        {
            yield return ShowTurnLimitDialogue();

            cashOutSystem.CheckCashOut();
        }

        private IEnumerator ShowTurnLimitDialogue()
        {
            if (!useTurnLimit || currentTurns < currentMaxTurns) yield break;
            
            dialogueSystem.ShowTurnLimitTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            
            SceneManager.LoadSceneAsync(2);
        }
        
        #endregion

        #region Next Milestone
        
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
        
        private void UpdateEventKeepsakes()
        {
            KeepsakeManager.instance.RechargeSecondDealing();
        }

        private IEnumerator DisplayNextMilestone(Milestone milestone)
        {
            if (milestone == null) yield break;
            
            StartCoroutine(milestone.gameEvent.StartDisplay(gameCamera, statusText));
        }

        #endregion

        #region Keepsakes

        private IEnumerator PresentKeepsakeChoice(Milestone milestone)
        {
            if (milestone?.keepsakes == null || milestone.keepsakes.Count == 0) yield break;

            yield return new WaitForSeconds(0.5f);
            
            gameCamera.ChangeToCamera(CameraType.Playing);
            
            yield return selectKeepsake.PresentKeepsakeChoice(milestone.keepsakes);

        }

        #endregion
    }
}