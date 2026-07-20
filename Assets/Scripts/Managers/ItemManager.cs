using System.Collections.Generic;
using Managers;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private BlackjackGame blackjackGame;
    private CursorFollow cursorFollow;
    private ShopManager shopManager;

    private List<Item> itemsToRemove = new();
    
    #region Setup

    public void SetBlackjackGame(BlackjackGame game)
    {
        blackjackGame = game;
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
            Debug.Log("deny item " + item.name);
            if(item.type != ItemType.Organ) 
                AudioManager.instance.Play("ItemDeny");
            return;
        }

        if(item.type != ItemType.Scissors || item.type != ItemType.Acid)
            AudioManager.instance.Play(item.name);
        else
            AudioManager.instance.Play("ItemBuy");

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.UseItems);

        if (!item.delayDestroy)
            shopManager.RemoveFromInventory(item);
        else
            AddItemToRemove(item);
    }

    private void OnSell(Item item)
    {
        blackjackGame.SellItem(item.GetResalePrice());
        
        AudioManager.instance.Play("ItemBuy");
        
        shopManager.RemoveFromInventory(item);

        shopManager.PlaySuitcaseOpen();
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

    #endregion
    
    #region Spawn Item

    public bool SpawnInventoryItem(GameObject rewardPrefab)
    {
        var item = shopManager.SpawnItemInventory(rewardPrefab);
        if (!item)
        {
            Debug.Log("spawn no item");
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