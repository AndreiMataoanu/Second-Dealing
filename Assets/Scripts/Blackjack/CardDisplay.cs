using System.Collections;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject cardBack;
    [SerializeField] private GameObject cardFace;
    [SerializeField] private Texture2D textureAce11;
    [SerializeField] private Texture2D textureAce1;
    private Renderer[] renderers;
    private Renderer[] faceRenderers;
    private CardInstance cardInstance;
    private MeshCollider faceCollider;
    private Coroutine aceTransitionCoroutine;
    private bool isAce1 = false;
    private bool isAceInitialized = false;

    private void Awake()
    {
        faceCollider = cardFace.GetComponent<MeshCollider>();
        renderers = GetComponentsInChildren<Renderer>();
        faceRenderers = cardFace.GetComponentsInChildren<Renderer>();
    }

    public void SetFaceColliderActive(bool isActive) => faceCollider.enabled = isActive;
    public void SetCardInstance(CardInstance instance) => cardInstance = instance;
    public CardInstance GetCardInstance() => cardInstance;
    
    public void SetHidden(bool isHidden)
    {
        cardBack?.SetActive(isHidden);
        cardFace?.SetActive(!isHidden);
    }

    public void SetNegativeVisual(bool isNegative)
    {
        float floatValue = isNegative ? 1f : 0f;

        foreach(var render in faceRenderers)
        {
            render.material.SetFloat("_Negative", floatValue);
        }
    }

    public void SetDoubledOnceVisual(bool isDoubled)
    {
        float floatValue = isDoubled ? 1f : 0f;

        foreach(var render in faceRenderers)
        {
            render.material.SetFloat("_DoubledOnce", floatValue);
        }
    }

    public void SetDoubledTwiceVisual(bool isDoubled)
    {
        float floatValue = isDoubled ? 1f : 0f;

        foreach(var render in faceRenderers)
        {
            render.material.SetFloat("_DoubledTwice", floatValue);
        }
    }

    public void SetCutOnceVisual(bool isCut)
    {
        float floatValue = isCut ? 1f : 0f;

        foreach(var render in renderers)
        {
            render.material.SetFloat("_CutOnce", floatValue);
        }
    }

    public void SetCutTwiceVisual(bool isCut)
    {
        float floatValue = isCut ? 1f : 0f;

        foreach(var render in renderers)
        {
            render.material.SetFloat("_CutTwice", floatValue);
        }
    }

    public IEnumerator SetDissolvedVisual(float dissolveTime,Color color, float dissolveBorder)
    {
        foreach(var render in renderers)
        {
            render.material.SetFloat("_DissolveEdge", dissolveBorder);
            render.material.SetColor("_DissolveColor", color);
        }

        float elapsedTime = 0f;

        while(elapsedTime < dissolveTime)
        {
            float lerpValue = Mathf.Lerp(0, 1, elapsedTime / dissolveTime);

            foreach(var render in renderers)
            {
                render.material.SetFloat("_Dissolve", lerpValue);
            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        foreach(var render in renderers)
        {
            render.material.SetFloat("_Dissolve", 1f);
        }
    }

    public void SetColorSwapVisual(bool isSwapped)
    {
        float floatValue = isSwapped ? 1f : 0f;

        foreach(var render in renderers)
        {
            render.material.SetFloat("_ColorSwapped", floatValue);
        }
    }

    public void SetAceValueVisual(bool isValue1)
    {
        if(textureAce1 == null || textureAce11 == null) return;

        if(isAce1 == isValue1) return;

        isAce1 = isValue1;

        Texture2D targetTexture = isValue1 ? textureAce1 : textureAce11;
        Texture2D fromTexture = isValue1 ? textureAce11 : textureAce1;

        if(aceTransitionCoroutine != null) StopCoroutine(aceTransitionCoroutine);

        aceTransitionCoroutine = StartCoroutine(TransitionAceTexture(targetTexture, fromTexture));
    }

    private IEnumerator TransitionAceTexture(Texture2D targetTexture, Texture2D fromTexture)
    {
        Renderer faceRenderer = cardFace.GetComponent<Renderer>();

        if(faceRenderer == null) yield break;

        foreach(Transform child in cardFace.transform.parent)
        {
            if(child.name == "TempDissolveFace") Destroy(child.gameObject);
        }

        GameObject tempFace = Instantiate(cardFace, cardFace.transform.parent);

        tempFace.name = "TempDissolveFace";

        Vector3 newLocalPos = cardFace.transform.localPosition;

        newLocalPos.z -= 0.0005f;
        tempFace.transform.localPosition = newLocalPos;
        tempFace.transform.localRotation = cardFace.transform.localRotation;
        tempFace.transform.localScale = cardFace.transform.localScale;

        Collider tempCollider = tempFace.GetComponent<Collider>();

        if(tempCollider != null) Destroy(tempCollider);

        Renderer tempRenderer = tempFace.GetComponent<Renderer>();

        tempRenderer.material.SetTexture("_MainTex", fromTexture);
        faceRenderer.material.SetTexture("_MainTex", targetTexture);

        float dissolveTime = 1f;

        tempRenderer.material.SetColor("_DissolveColor", Color.aliceBlue);
        tempRenderer.material.SetFloat("_DissolveEdge", 1.2f);

        float elapsedTime = 0f;

        while(elapsedTime < dissolveTime)
        {
            float lerpValue = Mathf.Lerp(0, 1, elapsedTime / dissolveTime);

            tempRenderer.material.SetFloat("_Dissolve", lerpValue);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        Destroy(tempFace);
    }
}
