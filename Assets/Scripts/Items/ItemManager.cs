using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private BlackjackGame blackjackGame;
    private CursorDetection cursorDetection;
    private ShopManager shopManager;
    
    #region Setup

    public void SetBlackjackGame(BlackjackGame game)
    {
        blackjackGame = game;
        cursorDetection = game.CursorDetection;
    }

    public void SetShopManager(ShopManager shop)
    {
        Debug.Log("Set shop " + shop);
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

        shopManager.RemoveFromInventory(item);
    }

    private void OnSell(Item item)
    {
        blackjackGame.SellItem(item.GetResalePrice());
        
        AudioManager.instance.Play("ItemBuy");
        
        shopManager.RemoveFromInventory(item);
    }
    
    #endregion

    #region Passive Effects
    
    public void OnRoundEnded()
    {
        if(blackjackGame.isOrganActive && shopManager.organRoundsLeft > 0)
        {
            shopManager.organRoundsLeft--;

            if(shopManager.organRoundsLeft == 0)
            {
                blackjackGame.isOrganActive = false;

                AudioManager.instance.Play("OrganExpire");
                
                shopManager.RemoveFromInventory(ItemType.Organ);
            }
        }
    }

    public void OnRoundStart()
    {
        foreach (var item in shopManager.InventoryItems)
            item.OnRoundStart();
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