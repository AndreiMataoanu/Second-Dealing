using System.Collections.Generic;
using Managers;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private BlackjackGame blackjackGame;
    private CursorDetection cursorDetection;
    private CursorFollow cursorFollow;
    private ShopManager shopManager;

    private List<Item> itemsToRemove = new();
    

    //
    // private IEnumerator DissolveCard(CardInstance cardInstance, float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //
    //     var cardObject = cardInstance.displayComponent.gameObject;
    //
    //     activeCardObjects.Remove(cardObject);
    //     gameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit);
    //     
    //     if(dealerHand.Remove(cardInstance))
    //     {
    //         DestroyCard(cardObject);
    //
    //         yield return null;
    //     }
    //
    //     foreach(var playerHand in playerHands)
    //     {
    //         if(playerHand.Remove(cardInstance))
    //         {
    //             DestroyCard(cardObject);
    //
    //             yield return null;
    //         }
    //     }
    //     
    //     peekCardInstance = null;
    //
    //     DestroyCard(cardObject);
    //
    //     yield return null;
    // }
    //
    // private void DestroyCard(GameObject cardObject)
    // {
    //     Destroy(cardObject);
    //
    //     isAcidActive = false;
    //
    //     UpdateUI();
    // }
    
    // public bool ActivateCigarette()
    // {
    //     if(CheckItemAfterStand()) return false;
    //
    //     if(!isRoundActive || (isActionLocked && !useAfterStand) || isCigaretteActive || isSplitting) return false;
    //
    //     isCigaretteActive = true;
    //
    //     StartCoroutine(CigaretteCoroutine());
    //
    //     return true;
    // }
    //
    // private IEnumerator CigaretteCoroutine()
    // {
    //     isActionLocked = true;
    //
    //     if(dealToDealerCoroutine != null)
    //     {
    //         StopCoroutine(dealToDealerCoroutine);
    //
    //         dealToDealerCoroutine = null;
    //     }
    //
    //     int targetIndex = ChooseHandIndex();
    //
    //     currentHandIndex = targetIndex;
    //     isPlayerStand = false;
    //
    //     cursorDetection.OnRoundActive();
    //
    //     List<CardInstance> tempHand = new List<CardInstance>(playerHands[currentHandIndex]);
    //
    //     playerHands[currentHandIndex] = new List<CardInstance>(dealerHand);
    //     dealerHand = new List<CardInstance>(tempHand);
    //
    //     AudioManager.instance.Play("Smoking");
    //
    //     yield return new WaitForSeconds(1f);
    //
    //     smokeParticle.Play();
    //
    //     yield return new WaitForSeconds(1f);
    //
    //     foreach(var card in playerHands[targetIndex])
    //     {
    //         if(card.isHidden)
    //         {
    //             yield return StartCoroutine(FlipCardCoroutine(card.displayComponent, 0.4f));
    //
    //             card.isHidden = false;
    //         }
    //     }
    //
    //     float animDuration = 0.5f;
    //     int maxCards = Mathf.Max(playerHands[currentHandIndex].Count, dealerHand.Count);
    //
    //     Transform currentParent = handPositions[currentHandIndex];
    //
    //     for(int i = 0; i < maxCards; i++)
    //     {
    //         if(i < playerHands[currentHandIndex].Count)
    //         {
    //             CardInstance pCard = playerHands[currentHandIndex][i];
    //
    //             pCard.displayComponent.transform.SetParent(currentParent.parent);
    //
    //             int cardOrderIndex = playerHands[currentHandIndex].Count - 1 - i;
    //             float xOffset = cardOrderIndex * playerCardOffset.x;
    //             float yOffset = cardOrderIndex * playerCardOffset.y;
    //             float zOffset = cardOrderIndex * -zOverlap;
    //
    //             Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);
    //
    //             StartCoroutine(CardAnimationCoroutine(
    //                 pCard.displayComponent.transform,
    //                 currentParent.TransformPoint(targetLocalPos),
    //                 currentParent.rotation,
    //                 cardScaleVector,
    //                 animDuration
    //             ));
    //         }
    //
    //         if(i < dealerHand.Count)
    //         {
    //             CardInstance dCard = dealerHand[i];
    //
    //             dCard.displayComponent.transform.SetParent(dealerCardPosition.parent);
    //
    //             int cardOrderIndex = dealerHand.Count - 1 - i;
    //             float xOffset = cardOrderIndex * dealerCardHorizontalSpacing;
    //             float yOffset = 0f;
    //             float zOffset = cardOrderIndex * -zOverlap;
    //
    //             Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);
    //
    //             StartCoroutine(CardAnimationCoroutine(
    //                 dCard.displayComponent.transform,
    //                 dealerCardPosition.TransformPoint(targetLocalPos),
    //                 dealerCardPosition.rotation,
    //                 cardScaleVector,
    //                 animDuration
    //             ));
    //         }
    //     }
    //
    //     yield return new WaitForSeconds(animDuration);
    //
    //     foreach(CardInstance card in playerHands[currentHandIndex])
    //     {
    //         card.displayComponent.transform.SetParent(currentParent);
    //     }
    //
    //     foreach(CardInstance card in dealerHand)
    //     {
    //         card.displayComponent.transform.SetParent(dealerCardPosition);
    //     }
    //
    //     UpdateHandVisuals(playerHands[currentHandIndex], currentParent, true);
    //     UpdateHandVisuals(dealerHand, dealerCardPosition, false);
    //     UpdateUI(true);
    //
    //     smokeParticle.Stop();
    //
    //     int handValue = CalculateHandValue(playerHands[targetIndex], true);
    //
    //     if(handValue > blackjackGoal || handValue < -blackjackGoal)
    //     {
    //         yield return StartCoroutine(BustCheckCoroutine(playerHands[targetIndex]));
    //     }
    //     else
    //     {
    //         isActionLocked = false;
    //         statusText.text = "";
    //
    //         EvaluateDoubleDownCondition();
    //     }
    // }
    //
    // private int ChooseHandIndex()
    // {
    //     if(!isPlayerStand) return currentHandIndex;
    //
    //     return Mathf.Max(0, currentHandIndex - 1);
    // }
    //
    
    //
    // public bool ActivateFan()
    // {
    //     if(CheckItemAfterStand()) return false;
    //
    //     if(!isRoundActive || (isActionLocked && !useAfterStand)) return false;
    //     
    //     StartCoroutine(FanCoroutine());
    //
    //     return true;
    // }
    //
    // private IEnumerator FanCoroutine()
    // {
    //     isActionLocked = true;
    //     isRoundActive = false;
    //     
    //     if (dealToDealerCoroutine != null)
    //     {
    //         StopCoroutine(dealToDealerCoroutine);
    //         dealToDealerCoroutine = null;
    //
    //         yield return null;
    //     }
    //     
    //     yield return StartCoroutine(AnimateCardsOffScreen());
    //
    //     ClearTable();
    //
    //     playerHands.Add(new List<CardInstance>());
    //     handBets.Add(currentBet);
    //     currentHandIndex = 0;
    //
    //     OnStartGame();
    // }
    //
    // private IEnumerator AnimateCardsOffScreen()
    // {
    //     float animDuration = 2f;
    //
    //     List<Coroutine> moveCoroutines = new List<Coroutine>();
    //
    //     foreach(GameObject card in activeCardObjects)
    //     {
    //         Vector3 randomWindDirection = new Vector3(Random.Range(-25f, -15f), Random.Range(5f, 15f), Random.Range(-10f, 10f));
    //         Vector3 offScreenPos = card.transform.position + randomWindDirection;
    //         Vector3 randomSpin = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));
    //
    //         moveCoroutines.Add(StartCoroutine(BlowCardAwayCoroutine(card.transform, offScreenPos, randomSpin, animDuration)));
    //     }
    //
    //     foreach(Coroutine c in moveCoroutines)
    //     {
    //         yield return c;
    //     }
    // }
    //
    // //Helps with spinning cards away when the fan is used.
    // private IEnumerator BlowCardAwayCoroutine(Transform cardTransform, Vector3 targetPosition, Vector3 spinSpeed, float duration)
    // {
    //     Vector3 startPosition = cardTransform.position;
    //
    //     float time = 0;
    //
    //     while(time < duration)
    //     {
    //         time += Time.deltaTime;
    //
    //         float t = time / duration;
    //         float moveT = t * t * (3f - 2f * t);
    //
    //         cardTransform.position = Vector3.Lerp(startPosition, targetPosition, moveT);
    //         cardTransform.Rotate(spinSpeed * Time.deltaTime, Space.World);
    //
    //         yield return null;
    //     }
    // }
    
    #region Setup

    public void SetBlackjackGame(BlackjackGame game)
    {
        blackjackGame = game;
        cursorDetection = game.CursorDetection;
    }

    public void SetShopManager(ShopManager shop)
    {
        shopManager = shop;
        shopManager.SetBuyAction(OnBuy);
    }
    
    #endregion
    
    #region Item Actions
    
    public void ChangeItemAction(bool isRoundActive)
    {
        foreach (var item in shopManager.InventoryItems)
        {
            if (isRoundActive)
            {
                item.RemoveAction(OnSell);
                item.AddAction(Activate);
            }
            else
            {
                item.AddAction(OnSell);
                item.RemoveAction(Activate);
            }
        }
    }
    
    private void OnBuy(Item item)
    {
        if (!shopManager.CanBuyItem(item)) return;

        item.RemoveAction(OnBuy);
        item.AddAction(OnSell);
        
        shopManager.AddToInventory(item);
        shopManager.OnCloseShop();
    }
    
    private void Activate(Item item)
    {
        if(!item.Activate())
        {
            if(item.type != ItemType.Organ) 
                AudioManager.instance.Play("ItemDeny");
            return;
        }

        if(item.type != ItemType.Scissors || item.type != ItemType.Acid) // TODO: add item.HasCardEffect
            AudioManager.instance.Play(item.name);
        else
            AudioManager.instance.Play("ItemBuy");

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.UseItems);

        if (item.cardEffect == null)
            shopManager.RemoveFromInventory(item);
        else
            AddItemToRemove(item);
    }

    private void OnSell(Item item)
    {
        blackjackGame.SellItem(item.GetResalePrice());
        
        AudioManager.instance.Play("ItemBuy");
        
        shopManager.RemoveFromInventory(item);
    }
    
    #endregion

    #region Passive Effects
    
    public void OnRoundEnd()
    {
        foreach (var item in shopManager.InventoryItems)
            item.OnRoundEnd();

        RemovePassiveItems();
    }

    private void RemovePassiveItems()
    {
        if (itemsToRemove.Count == 0) return;
        
        foreach (var consumed in itemsToRemove)
            shopManager.RemoveFromInventory(consumed);
            
        itemsToRemove.Clear();
    }

    public void OnRoundStart()
    {
        foreach (var item in shopManager.InventoryItems)
            item.OnRoundStart();
    }

    public void AddItemToRemove(Item item)
    {
        item.SetVisibility(false);
        item.SetColliderActive(false);
        itemsToRemove.Add(item);
    }

    #endregion
    
    #region Tarot

    public void OnTarotSpawn(GameObject rewardPrefab)
    {
        var item = shopManager.SpawnItemInventory(rewardPrefab);
        if (item == null)
        {
            AudioManager.instance.Play("ItemDeny");
            return;
        }
        
        AudioManager.instance.Play("ItemBuy");
        
        item.AddAction(Activate);
        item.SetActive(true);
        
        cursorDetection.AddRoundActiveClickable(item);
    }

    #endregion
}