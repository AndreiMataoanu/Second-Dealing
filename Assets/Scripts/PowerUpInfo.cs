using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PowerUpInfo : MonoBehaviour
{
    [SerializeField] public int price;
    [SerializeField] public PowerUpType type;

    private BlackjackGame _blackjackGame;

    public void Activate()
    {
        if (!_blackjackGame)
        {
            Debug.Log("No blackjack game");
            return;
        }
        
        switch (type)
        {
            // TODO: Implement power-up methods
            
            case PowerUpType.Knife:
                _blackjackGame.ActivateKnife();
                break;
            case PowerUpType.Scissors:
                _blackjackGame.ActivateScissors();
                break;
            case PowerUpType.Crucifix:
                _blackjackGame.ActivatePrayerBeads();
                break;
            case PowerUpType.Glove:
                Debug.Log("Glove used");
                break;
            case PowerUpType.Sunglasses:
                _blackjackGame.ActivateSunglasses();
                break;
            case PowerUpType.Cuffs:
                Debug.Log("Cuffs used");
                break;
        }
    }

    public void SetBlackjackGame(BlackjackGame blackjackGame)
    {
        _blackjackGame = blackjackGame;
    }
}
