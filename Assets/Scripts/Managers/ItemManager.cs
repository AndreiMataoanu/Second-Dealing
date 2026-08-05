using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private BlackjackGame blackjackGame;
    private ShopManager shopManager;

    private List<Item> itemsToRemove = new();
    
    #region Setup

    public void SetBlackjackGame(BlackjackGame game)
    {
        blackjackGame = game;
        shopManager = blackjackGame.ShopManager;
        shopManager.SetBuyAction(OnBuy);
    }

    public void DeactivateItems()
    {
        KnifeItem.isKnifeActive = false;
        ScissorsItem.isScissorsActive = false;
        AcidItem.isAcidActive = false;
        CrucifixItem.isCrucifixActive = false;
        CigarettesItem.isCigaretteActive = false;
        AlcoholItem.isAlcoholActive = false;
    }
    
    #endregion
    
    #region Item Actions
    
    public void ChangeItemAction(bool isRoundActive)
    {
        foreach (var item in shopManager.InventoryItems)
        {
            item.RemoveAction(OnSell);
            item.RemoveAction(Activate);

            if (isRoundActive)
                item.AddAction(Activate);
            else
                item.AddAction(OnSell);
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

        AudioManager.instance.Play("ItemBuy");
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.UseItems);

        if (!item.delayDestroy)
            shopManager.RemoveFromInventory(item);
        else
            AddItemToRemove(item);
    }

    private void OnSell(Item item)
    {
        if(shopManager.State == ShopState.Open || shopManager.State == ShopState.Closed)
        {
            blackjackGame.SellItem(item.GetResalePrice());
            
            AudioManager.instance.Play("ItemBuy");
            
            shopManager.RemoveFromInventory(item); 
        }
        if(shopManager.State == ShopState.Closed)
        {
            shopManager.PlaySuitcaseOpen();   
        }
    }
    
    #endregion

    #region Passive Effects
    
    public void OnRoundEnd()
    {
        foreach (var item in shopManager.AllInventoryItems)
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
        shopManager.DelayRemoveFromInventory(item);
        itemsToRemove.Add(item);
    }

    public void UndoItemToRemove(Item item)
    {
        item.SetVisibility(true);
        item.SetColliderActive(true);
        shopManager.UndoDelayRemoveFromInventory(item);
        itemsToRemove.Remove(item);
    }

    #endregion
    
    #region Spawn Item

    public bool SpawnInventoryItem(GameObject rewardPrefab)
    {
        var item = shopManager.SpawnItemInventory(rewardPrefab);
        if (!item)
        {
            AudioManager.instance.Play("ItemDeny");
            return false;
        }
        
        AudioManager.instance.Play("ItemBuy");
        
        item.AddAction(Activate);
        item.SetActive(true);
        
        return true;
    }

    #endregion
}