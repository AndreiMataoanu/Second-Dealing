using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CashOutSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button stayButton;
    [SerializeField] private Animator fadeInAnimator;
    [SerializeField] private GameObject champagneFlask;
    private static readonly int FadeInTrig = Animator.StringToHash("fadeInTrig");

    private DialogueSystem dialogueSystem;
    private CursorDetection cursorDetection;
    private BlackjackGame game;

    private bool stayed = false;

    public void SetBlackjackGame(BlackjackGame blackjackGame)
    {
        game = blackjackGame;
        dialogueSystem = game.DialogueSystem;
        cursorDetection = game.CursorDetection;
    }

    public void CheckCashOut()
    {
        if(game.PlayerMoney >= 100000 && stayed == false)
            StartCoroutine(LeaveOrStayCoroutine());
    }
    
    private IEnumerator LeaveOrStayCoroutine()
    {
        dialogueSystem.PlayCashOutText();
        
        yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        
        cursorDetection.SetAllInactive();
        leaveButton.gameObject.SetActive(true);
        stayButton.gameObject.SetActive(true);
    }
    
    public void Leave()
    {
        PlayerPrefs.SetInt("PreviousRunMoney", game.PlayerMaxMoney);
        PlayerPrefs.SetInt("PreviousRunWins", game.TimesWon);
        PlayerPrefs.SetInt("PreviousRunLoss", game.TimesLost);
        PlayerPrefs.Save();
        
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.CashOut);
        KeepsakeUnlockProgression.instance.EndRun();
        
        fadeInAnimator.SetTrigger(FadeInTrig);
        
        if(game.PlayerMoney >= 1000000)
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.Millionaire);

        CardEffects.ClearColorSwappedCards();
        SceneManager.LoadSceneAsync(2);
    }
    
    public void Stay()
    {
        leaveButton.gameObject.SetActive(false);
        stayButton.gameObject.SetActive(false);
        cursorDetection.OnRoundInactive();
        stayed = true;
        champagneFlask.SetActive(true);

    }
}
