public class CardInstance
{
    public Card cardData;

    public CardDisplay displayComponent;

    public bool isHidden;

    public CardInstance(Card card, CardDisplay display, bool hidden = false)
    {
        cardData = card;
        displayComponent = display;
        isHidden = hidden;
    }
}
