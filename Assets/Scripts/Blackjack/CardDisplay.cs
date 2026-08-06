using System.Collections;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject cardBack;
    [SerializeField] private GameObject cardFace;
    private Renderer[] renderers;
    private CardInstance cardInstance;
    private MeshCollider faceCollider;

    private void Awake()
    {
        faceCollider = cardFace.GetComponent<MeshCollider>();
        renderers = GetComponentsInChildren<Renderer>();
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

        foreach(var render in renderers)
        {
            render.material.SetFloat("_Negative", floatValue);
        }
    }

    public void SetDoubledVisual(bool isDoubled)
    {
        float floatValue = isDoubled ? 1f : 0f;

        foreach(var render in renderers)
        {
            render.material.SetFloat("_DoubledOnce", floatValue);
        }
    }

    public void SetCutVisual(bool isCut)
    {
        float floatValue = isCut ? 1f : 0f;

        foreach(var render in renderers)
        {
            render.material.SetFloat("_CutOnce", floatValue);
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
}
