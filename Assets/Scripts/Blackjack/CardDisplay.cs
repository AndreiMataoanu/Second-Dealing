using System.Collections;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject cardBack;
    [SerializeField] private GameObject cardFace;
    private Renderer render;
    private CardInstance cardInstance;
    private MeshCollider faceCollider;

    private void Awake()
    {
        faceCollider = cardFace.GetComponent<MeshCollider>();
        render = GetComponentInChildren<Renderer>();
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
        float boolValue = isNegative ? 1f : 0f;

        render.material.SetFloat("_Negative", boolValue);
    }

    public void SetDoubledVisual(bool isDoubled)
    {
        float boolValue = isDoubled ? 1f : 0f;

        render.material.SetFloat("_DoubledOnce", boolValue);
    }

    public void SetCutVisual(bool isCut)
    {
        float boolValue = isCut ? 1f : 0f;

        render.material.SetFloat("_CutOnce", boolValue);
    }

    public IEnumerator SetDissolvedVisual(float dissolveTime,Color color, float dissolveBorder)
    {
        render.material.SetFloat("_DissolveEdge", dissolveBorder);
        render.material.SetColor("_DissolveColor",color);
        
        float elapsedTime = 0f;
        while (elapsedTime < dissolveTime)
        {
            render.material.SetFloat("_Dissolve",Mathf.Lerp(0,1,elapsedTime/dissolveTime));   
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        render.material.SetFloat("_Dissolve", 1f);
    }
}
