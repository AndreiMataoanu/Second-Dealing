public class CardInstance
{
    public Card cardData;

    public CardDisplay displayComponent;

    public bool isHidden;

    public int jokerValue = 0;

    public CardInstance(Card card, CardDisplay display, bool hidden = false)
    {
        cardData = card;
        displayComponent = display;

        if(displayComponent != null)
        {
            displayComponent.SetCardInstance(this);
        }

        isHidden = hidden;
    }
}
