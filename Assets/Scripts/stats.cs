using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class stats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyEarnedText;
    [SerializeField] private TextMeshProUGUI timesWon;
    [SerializeField] private TextMeshProUGUI timesLost;
    [SerializeField] private Animator fadeInAnimator;
    [SerializeField] private float delay = 2f;

    void Start()
    {
        moneyEarnedText.text = "Money earned: "+ PlayerPrefs.GetInt("PreviousRunMoney").ToString();
        timesWon.text = "Rounds won: " + PlayerPrefs.GetInt("PreviousRunWins").ToString();
        timesLost.text = "Rounds lost: " + PlayerPrefs.GetInt("PreviousRunLoss").ToString();

    }


    public void GoBack()
    {
        StartCoroutine(fadeRoutine());
    }
    public IEnumerator fadeRoutine()
    {
        fadeInAnimator.SetTrigger("fadeInTrig");
        yield return StartCoroutine(GameUtils.WaitDelayOrInput(delay));
        SceneManager.LoadSceneAsync(1);
        yield return null;
        
    }
}
