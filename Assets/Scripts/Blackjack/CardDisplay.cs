using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject cardBack;
    [SerializeField] private GameObject cardFace;
    private Renderer render;

    private void Awake()
    {
        render = GetComponentInChildren<Renderer>();
    }

    public void SetHidden(bool isHidden)
    {
        if(cardBack != null)
        {
            cardBack.SetActive(isHidden);
        }

        if(cardFace != null)
        {
            cardFace.SetActive(!isHidden);
        }
    }

    public void SetNegativeVisual(bool isNegative)
    {
        float boolValue = isNegative ? 1f : 0f;

        render.material.SetFloat("_Negative", boolValue);
    }

    public void SetDoubledVisual(bool isDoubled)
    {
        float boolValue = isDoubled ? 1f : 0f;

        render.material.SetFloat("_Doubled", boolValue);
    }

    public void SetCutVisual(bool isCut)
    {
        float boolValue = isCut ? 1f : 0f;

        render.material.SetFloat("_CutInHalf", boolValue);
    }
}
