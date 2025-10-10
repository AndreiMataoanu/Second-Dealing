using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject cardBack;
    [SerializeField] private GameObject cardFace;

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
}
