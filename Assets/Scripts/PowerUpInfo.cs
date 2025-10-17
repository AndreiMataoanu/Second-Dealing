using UnityEngine;

public class PowerUpInfo : MonoBehaviour
{
    [SerializeField] public int price;
    [SerializeField] public PowerUpType type;

    private BlackjackGame _blackjackGame;

    public void Activate()
    {
        if(!_blackjackGame)
        {
            Debug.Log("No blackjack game");

            return;
        }
        
        switch(type)
        {
            case PowerUpType.Knife:
                _blackjackGame.ActivateKnife();
            break;

            case PowerUpType.Scissors:
                _blackjackGame.ActivateScissors();
            break;

            case PowerUpType.Crucifix:
                _blackjackGame.ActivatePrayerBeads();
            break;

            case PowerUpType.Sunglasses:
                _blackjackGame.ActivateSunglasses();
            break;
        }
    }

    public void SetBlackjackGame(BlackjackGame blackjackGame)
    {
        _blackjackGame = blackjackGame;
    }
}
