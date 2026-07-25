using UnityEngine;

public class CardInstance
{
    public Card cardData;

    public CardDisplay displayComponent;

    public TarotCard tarotData;

    public bool isHidden;

    public int jokerValue = 0;
    
    public GameObject CardObject => displayComponent?.gameObject; 

    public CardInstance(Card card, CardDisplay display, bool hidden = false)
    {
        cardData = card;
        displayComponent = display;

        if(displayComponent)
        {
            displayComponent.SetCardInstance(this);
            tarotData = display.GetComponent<TarotCard>();
        }

        isHidden = hidden;
    }
}
