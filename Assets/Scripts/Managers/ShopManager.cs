using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    private ShopState state = ShopState.Closed;
    public ShopState State => state;

    [Header("Item")]
    [SerializeField] private List<GameObject> buySpawnPoints;
    [SerializeField] private List<GameObject> useSpawnPoints;
    [SerializeField] private List<GameObject> itemPrefabs;
    
    [Header("Sound")]
    [SerializeField] private float denySoundCooldown = 0.3f;

    [Header("Suitcase")] 
    [SerializeField] private Animator suitcaseAnimator;
    
    private int inventoryItemCount = 0;
    private float nextDenyTime = 0;
    private float coinMultiplier = 1.0f;
    private bool delayOpen;
    
    private List<Item> inventoryItems = new();
    private List<Item> allItems = new();
    private BlackjackGame blackjackGame;

    private Action<Item> BuyAction;
    
    #region Getters

    public List<Item> InventoryItems => inventoryItems;
    public List<Item> AllInventoryItems => allItems;

    #endregion
    
    #region Setters

    public void SetBlackjackGame(BlackjackGame game) => blackjackGame = game;

    public void SetBuyAction(Action<Item> buyAction) => BuyAction = buyAction;

    public void SetDelayOpen(bool delay) => delayOpen = delay;
    
    #endregion

    #region Open Shop

    private void SpawnPowerUps()
    {
        foreach (var buySpawnPoint in buySpawnPoints.ToList())
        {
            var prefab = GetWeightedRandomPrefab();
            var item = SpawnItem(prefab, buySpawnPoint.transform);

            item.SetMultiplier(coinMultiplier);
            item.SetActive(true);
            item.AddAction(BuyAction);
        }
    }
    
    private GameObject GetWeightedRandomPrefab()
    {
        bool hasOrgan = OrganBagItem.isOrganActive;
        int currentTotalWeight = 0;

        // revise valid prefabs should be member
        List<GameObject> validPrefabs = new();
        foreach(var prefab in itemPrefabs)
        {
            var item = prefab.GetComponent<Item>();

            if((OrganBagItem.isInShop || hasOrgan) && item.type == ItemType.Organ) continue;

            validPrefabs.Add(prefab);
            currentTotalWeight += item.spawnWeight;

            if (item.type == ItemType.Organ) OrganBagItem.isInShop = true;
        }

        int roll = Random.Range(0, currentTotalWeight);
        int cursor = 0;

        foreach(var prefab in validPrefabs)
        {
            cursor += prefab.GetComponent<Item>().spawnWeight;

            if(roll < cursor) return prefab;
        }

        return itemPrefabs[0];
    }

    #endregion

    #region Close Shop
    
    public void OnCloseShop()
    {
        OrganBagItem.isInShop = false;
        if (state != ShopState.Open || ItemsInShop() && inventoryItemCount != buySpawnPoints.Count) return;

        StartCoroutine(DespawnCoroutine());
    }

    public void DespawnShopItems()
    {
        StartCoroutine(DespawnCoroutine());
    }
    
    private IEnumerator DespawnCoroutine()
    {
        state = ShopState.Closing;

        AudioManager.instance.Play("SuitcaseClose");
        yield return new WaitForSeconds(0.6f);
        suitcaseAnimator.Play("Suitcase_Closing");

        yield return new WaitForSeconds(suitcaseAnimator.GetCurrentAnimatorStateInfo(0).length);
        if(ItemsInShop())
        {
            foreach(var spawnPoint in buySpawnPoints)
            {
                if(spawnPoint.transform.childCount > 0)
                {
                    Destroy(spawnPoint.transform.GetChild(0).gameObject);
                }
            }
        }
        ResetShopPrices();

        state = ShopState.Closed;
    }

    #endregion

    #region Inventory
    
    public void SetInventoryActive(bool isActive)
    {
        foreach (var item in inventoryItems)
            item.SetActive(isActive);
    }

    public void AddToInventory(Item item, bool isFree = false)
    {
        MoveToInventoryPosition(item, isFree);
        OnAddedToInventory(item);
    }
    
    private void MoveToInventoryPosition(Item item, bool isFree=false)
    {
        AudioManager.instance.Play("ItemBuy");

        if(!isFree) blackjackGame.BuyItem(item.GetPrice());
        
        var pos = item.transform.localPosition;
        var rot = item.transform.localRotation;
        var scale = item.transform.localScale;

        foreach(var spawnPoint in useSpawnPoints)
        {
            if(spawnPoint.transform.childCount == 0)
            {
                item.transform.parent = spawnPoint.transform;
                break;
            }
        }
        
        item.transform.localPosition = pos;
        item.transform.localRotation = rot;
        item.transform.localScale = scale;
    }

    private void OnAddedToInventory(Item item)
    {
        item.ActivatePassive();
        item.isPurchased = true;
        inventoryItemCount++;
        inventoryItems.Add(item);
        allItems.Add(item);
    }
    
    public void RemoveFromInventory(Item item)
    {
        if (!item) return;
        
        TooltipManager.instance.HideTooltip();
        
        item.DeactivatePassive();
        if (!item.delayDestroy)
        {
            inventoryItemCount--;
            inventoryItems.Remove(item);
        }

        allItems.Remove(item);
        Destroy(item.gameObject);
    }
    
    public void RemoveFromInventory(ItemType type)
    {
        foreach (var item in inventoryItems)
        {
            if (item.type == type)
            {
                RemoveFromInventory(item);
                return;
            }
        }
    }
    
    public Item SpawnItemInventory(GameObject prefab)
    {
        if(inventoryItemCount >= useSpawnPoints.Count) return null;

        var t = FindEmptyInventorySlot();
        if (!t) return null;
        
        var item = SpawnItem(prefab, t);
        OnAddedToInventory(item);

        return item;
    }

    #endregion

    #region Helper Methods
    
    private Item SpawnItem(GameObject prefab, Transform position)
    {
        if (!prefab) return null;
        
        GameObject prefabInstance = Instantiate(prefab, position);
        Item item = prefabInstance.GetComponent<Item>();
        item.SetBlackjackGame(blackjackGame);
        item.SetMembers();

        return item;
    }

    public bool CanBuyItem(Item item)
    {
        if(inventoryItemCount >= useSpawnPoints.Count || blackjackGame.PlayerMoney > item.GetPrice()) return true;

        if(Time.time >= nextDenyTime)
        {
            AudioManager.instance.Play("ItemDeny");

            nextDenyTime = Time.time + denySoundCooldown;
        }

        return false;
    }

    public void DelayRemoveFromInventory(Item item)
    {
        if (!item.delayDestroy) return;
        inventoryItemCount--;
        inventoryItems.Remove(item);
    }

    private Transform FindEmptyInventorySlot()
    {
        if(inventoryItemCount >= useSpawnPoints.Count) return null;
        
        foreach(var spawnPoint in useSpawnPoints)
            if(spawnPoint.transform.childCount == 0 || !CheckVisibleItems(spawnPoint.transform))
                return spawnPoint.transform;

        return null;
    }

    private bool CheckVisibleItems(Transform inventorySlot)
    {
        for (int i = 0; i < inventorySlot.childCount; i++)
        {
            var item = inventorySlot.GetChild(i).gameObject.GetComponent<Item>();
            if (item.IsVisible) return true;
        }

        return false;
    }
    
    #endregion
    
    #region Coin

    // returns multiplier 0.5 - half off, 2.0 - double the price
    public bool FlipCoin()
    {
        // AudioManager.instance.Play("CoinSound");
        int coinFlip = Random.Range(0, 2);
        coinMultiplier = coinFlip == 0 ? 0.5f : 2.0f;
        
        return coinFlip == 0;
    }

    private void ResetShopPrices() => coinMultiplier = 1.0f;

    #endregion
    
    #region Suitcase Animation

    public void PlaySuitcaseOpen()
    {
        StartCoroutine(PlaySuitcaseOpenCoroutine());
    }
    
    private IEnumerator PlaySuitcaseOpenCoroutine()
    {
        yield return new WaitUntil(() => delayOpen == false);
        
        if (state != ShopState.Closed || inventoryItemCount == buySpawnPoints.Count)
            yield break;

        yield return SuitcaseOpenCoroutine();
    }
    
    private IEnumerator SuitcaseOpenCoroutine()
    {
        SpawnPowerUps();
        state = ShopState.Opening;
        suitcaseAnimator.Play("Suitcase_Opening");

        yield return new WaitForSeconds(0.2f);

        AudioManager.instance.Play("Latch");
        AudioManager.instance.Play("SuitcaseOpen");
        state = ShopState.Open;
    }
    
    #endregion

    private bool ItemsInShop()
    {
        int items = 0;
        
        foreach (var spawnPoint in buySpawnPoints)
            if(spawnPoint.transform.childCount > 0) items++;
        
        if(items == 0) return false;    

        return true;
    }

}
