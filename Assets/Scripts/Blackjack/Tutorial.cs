using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI tutorialText;
    [SerializeField] private Image panel;
    [SerializeField] private float delayBetweenLines = 1f;

    private Coroutine sequenceCoroutine;
    private Coroutine typingCoroutine;
    private bool isPlaying = false;
    private bool allowSkip = false;
    public bool IsPlaying => isPlaying;

    private void Update()
    {
        if(isPlaying && allowSkip && Input.anyKeyDown)
        {
            SkipTutorial();
        }
    }

    public void PlayTutorial(string[] lines)
    {
        if(sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if(typingCoroutine != null) StopCoroutine(typingCoroutine);

        sequenceCoroutine = StartCoroutine(SequenceCoroutine(lines));
    }

    private IEnumerator SequenceCoroutine(string[] lines)
    {
        isPlaying = true;
        allowSkip = false;
        panel.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        allowSkip = true;

        foreach(string line in lines)
        {
            typingCoroutine = StartCoroutine(TypeText(line));
            yield return typingCoroutine;
            yield return new WaitForSeconds(delayBetweenLines);
        }

        panel.gameObject.SetActive(false);
        isPlaying = false;
        allowSkip = false;
    }

    private IEnumerator TypeText(string line)
    {
        tutorialText.text = "";

        foreach(char letter in line.ToCharArray())
        {
            tutorialText.text += letter;

            AudioManager.instance.Play("Typing");

            yield return new WaitForSeconds(0.02f);
        }
    }

    private void SkipTutorial()
    {
        if(sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        if(typingCoroutine != null) StopCoroutine(typingCoroutine);

        isPlaying = false;
        panel.gameObject.SetActive(false);
    }
}
