using System.Collections.Generic;
using UnityEngine;

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private List<Clickable> roundActiveClickables;
    [SerializeField] private List<Clickable> roundInactiveClickables;
    [SerializeField] private List<Transform> cardTransforms;
    
    private List<Clickable> cardClickables;
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        int mouseButton = -1;

        if(Input.GetMouseButtonDown(0)) mouseButton = 0;
        else if(Input.GetMouseButtonDown(1)) mouseButton = 1;

        if (mouseButton != -1)
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                var clickable = hit.transform.GetComponent<Clickable>();

                if(clickable != null)
                {
                    clickable.OnClick(mouseButton);
                }
            }
        }
    }

    public void OnRoundActive()
    {
        SetClickables(roundActiveClickables, true);
        SetClickables(roundInactiveClickables, false);
    }

    public void OnRoundInactive()
    {
        SetClickables(roundActiveClickables, false);
        SetClickables(roundInactiveClickables, true);
    }

    public void OnDealerTurn()
    {
        SetClickables(roundActiveClickables, false);
        SetClickables(roundInactiveClickables, false);
    }

    private void SetClickables(List<Clickable> clickables, bool isActive)
    {
        foreach (var clickable in clickables)
        {
            clickable.SetActive(isActive);
            clickable.OnRemoveOutline();
        }
    }

    public void AddRoundActiveClickable(Clickable clickable)
    {
        roundActiveClickables.Add(clickable);
    }

    public void OnUseScissors(Item item)
    {
        AddAllClickableCards();
        SetClickables(cardClickables, true);
    }

    private void AddAllClickableCards()
    {
        cardClickables = new List<Clickable>();
        foreach (var cardsTransform in cardTransforms)
            AddClickableCards(cardsTransform);
    }

    private void AddClickableCards(Transform cardsTransform)
    {
        foreach (Transform card in cardsTransform)
        {
            var cardDisplay = card.GetComponent<CardDisplay>();
            var face = card.transform.GetChild(0);
            if (face)
            {
                var clickable = face.GetComponent<ClickableCard>();
                if (clickable)
                {
                    clickable.SetCardInstance(cardDisplay.GetCardInstance());
                    cardClickables.Add(clickable);
                }
            }
        }
    }
}
