using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("Set-up")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image panel;
    [SerializeField] private float delayBetweenLines = 1f;
    [SerializeField] private float typingSpeed = 0.02f;

    [Header("Tutorials")]
    [Tooltip("Tutorial shown at the start of the game.")]
    [SerializeField] private string[] tutorialLines;
    [Tooltip("Tutorial shown when the player can split for the first time.")]
    [SerializeField] private string[] splitTutorialLines;
    [Tooltip("Tutorial shown when the player can double down for the first time.")]
    [SerializeField] private string[] doubleDownTutorialLines;

    [Header("Taunt Quotes")]
    [Tooltip("Taunts shown when the player is low on money.")]
    [SerializeField] private List<string> lowMoneyTaunts;
    [Tooltip("Taunts shown when the player loses a lot of money in a bet.")]
    [SerializeField] private List<string> betLostTaunts;
    [Tooltip("Taunts shown when the dealer gets a natural blackjack.")]
    [SerializeField] private List<string> dealerBlackjackTaunts;
    [Tooltip("Taunts shown when the dealer wins by 1 point.")]
    [SerializeField] private List<string> dealerWinsByOneTaunts;
    [Tooltip("Taunts shown when the player gets a natural blackjack.")]
    [SerializeField] private List<string> playerBlackjackTaunts;
    [Tooltip("Taunts shown when the player and dealer tie.")]
    [SerializeField] private List<string> tieTaunts;
    [Tooltip("Taunts shown when player is out of turns.")]
    [SerializeField] private List<string> turnLimitTaunts;
    [Tooltip("Taunts shown when player chooses which card to copy")] 
    [SerializeField] private List<string> copyOptionTaunts;

    private Coroutine sequenceCoroutine;
    private Coroutine typingCoroutine;
    private bool isPlaying = false;
    private bool allowSkip = false;

    public bool IsPlaying => isPlaying;

    private void Update()
    {
        if(isPlaying && allowSkip && Input.anyKeyDown)
        {
            SkipDialogue();
        }
    }

    public void PlayTutorial()
    {
        StopCurrentDialogue();

        sequenceCoroutine = StartCoroutine(SequenceCoroutine(tutorialLines));
    }

    public void PlaySplitTutorial()
    {
        StopCurrentDialogue();

        sequenceCoroutine = StartCoroutine(SequenceCoroutine(splitTutorialLines));
    }

    public void PlayDoubleDownTutorial()
    {
        StopCurrentDialogue();

        sequenceCoroutine = StartCoroutine(SequenceCoroutine(doubleDownTutorialLines));
    }

    public void ShowAddCardsText(int copyNumber)
    {
        StopCurrentDialogue();
        string text = "Choose which card to copy " + copyNumber + " ";
        text += copyNumber == 1 ? "time." : "times."; 
        sequenceCoroutine = StartCoroutine(SingleMessageCoroutine(text, 4f));
    }

    private IEnumerator SequenceCoroutine(string[] lines)
    {
        isPlaying = true;
        allowSkip = false;

        if(panel != null) panel.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        allowSkip = true;

        foreach(string line in lines)
        {
            typingCoroutine = StartCoroutine(TypeText(line));

            yield return typingCoroutine;
            yield return new WaitForSeconds(delayBetweenLines);
        }

        EndDialogue();
    }

    public void ShowMessage(string message)
    {
        StopCurrentDialogue();

        sequenceCoroutine = StartCoroutine(SingleMessageCoroutine(message));
    }

    private IEnumerator SingleMessageCoroutine(string message, float delay=2f)
    {
        isPlaying = true;
        allowSkip = false;

        if(panel != null) panel.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        allowSkip = true;

        typingCoroutine = StartCoroutine(TypeText(message));

        yield return typingCoroutine;
        yield return new WaitForSeconds(delay);

        EndDialogue();
    }

    private IEnumerator TypeText(string line)
    {
        dialogueText.text = "";

        foreach(char letter in line.ToCharArray())
        {
            dialogueText.text += letter;

            if(AudioManager.instance != null) AudioManager.instance.Play("Typing");

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void StopCurrentDialogue()
    {
        if(sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);

        if(typingCoroutine != null) StopCoroutine(typingCoroutine);
    }

    private void EndDialogue()
    {
        isPlaying = false;
        allowSkip = false;

        if(panel != null) panel.gameObject.SetActive(false);

        dialogueText.text = "";
    }

    private void SkipDialogue()
    {
        StopCurrentDialogue();
        EndDialogue();
    }

    public void ShowLowMoneyTaunt() => ShowMessage(GetRandomTaunt(lowMoneyTaunts));
    public void ShowBetLostTaunt() => ShowMessage(GetRandomTaunt(betLostTaunts));
    public void ShowDealerBlackjackTaunt() => ShowMessage(GetRandomTaunt(dealerBlackjackTaunts));
    public void ShowDealerWinsByOneTaunt() => ShowMessage(GetRandomTaunt(dealerWinsByOneTaunts));
    public void ShowPlayerBlackjackTaunt() => ShowMessage(GetRandomTaunt(playerBlackjackTaunts));
    public void ShowTieTaunt() => ShowMessage(GetRandomTaunt(tieTaunts));
    public void ShowTurnLimitTaunt() => ShowMessage(GetRandomTaunt(turnLimitTaunts));
    public void ShowCopyChoiceTaunt() => ShowMessage(GetRandomTaunt(copyOptionTaunts));

    private string GetRandomTaunt(List<string> taunts)
    {
        if(taunts == null || taunts.Count == 0) return "";

        int index = Random.Range(0, taunts.Count);

        return taunts[index];
    }
}