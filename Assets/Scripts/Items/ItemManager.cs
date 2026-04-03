using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private BlackjackGame blackjackGame;
    [SerializeField] private CursorDetection cursorDetection;
    private int roundsSincePassiveBought = 0;
    [Header("Power ups")]
    [SerializeField] private List<GameObject> buySpawnPoints;
    [SerializeField] private List<GameObject> useSpawnPoints;
    [SerializeField] private List<GameObject> powerUpPrefabs;
    [SerializeField] private float denySoundCooldown = 0.3f;
    private Item currentPassive;
    private int inventoryItems = 0;
    private float nextDenyTime = 0;
    private int totalWeight = 0;
    
    private void Awake()
    {
        foreach(var item in powerUpPrefabs)
        {
            totalWeight += item.GetComponent<Item>().spawnWeight;
        }
    }

    public void SpawnPowerUps()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count == 0 || inventoryItems == useSpawnPoints.Count) return;

        foreach (var buySpawnPoint in buySpawnPoints.ToList())
        {
            var prefab = GetWeightedRandomPrefab(totalWeight);
            var prefabInstance = Instantiate(prefab, buySpawnPoint.transform);
            var item = prefabInstance.GetComponent<Item>();
            item.SetBlackjackGame(blackjackGame);
            item.SetActive(true);
            item.AddAction(OnBuy);
            BlackjackGame.Instance.telemetryData.itemsInShop.Add(item);
        }
    }

    public void DespawnPowerUps()
    {
        foreach (var item in buySpawnPoints)
        {
            if (item.transform.childCount > 0)
                Destroy(item.transform.GetChild(0).gameObject);
        }
    }
    
    private void OnBuy(Item item)
    {
        if (inventoryItems >= useSpawnPoints.Count || !HasEnoughMoney(item)) return;
        
        if(item.passive && currentPassive == null)
        {
            AddToInventory(item);
            item.RemoveAction(OnBuy);
            item.AddAction(Activate);
            item.SetActive(false);
            currentPassive = item;
            item.Activate();
        }
        if(!item.passive)
        {
            AddToInventory(item);
            item.RemoveAction(OnBuy);
            item.AddAction(Activate);
            item.SetActive(false);
        }
        
        cursorDetection.AddRoundActiveClickable(item);

        DeactivateShopItems();
    }
    
    private void Activate(Item item)
    {
        if (!item.Activate())
        {
            AudioManager.instance.Play("ItemDeny");
            return;
        }
        if(!item.passive)
        {
            AudioManager.instance.Play(item.name);
            TooltipManager.instance.HideTooltip();
            Destroy(item.gameObject);
            inventoryItems--;
        }
    }
    public void IsPassiveDone(bool passiveUsed)
    {
        if(currentPassive != null)
        {
            if(currentPassive.PassiveItemRounds == roundsSincePassiveBought || passiveUsed)
            {
                //AudioManager.instance.Play(item.name);
                //TooltipManager.instance.HideTooltip();
                Destroy(currentPassive.gameObject);
                inventoryItems--;
                roundsSincePassiveBought = 0;
                currentPassive = null;
            }
            else
            {
                roundsSincePassiveBought ++;
            }    
        } 
    }

    #region Helper Methods

    private GameObject GetWeightedRandomPrefab(int totalWeight)
    {
        int roll = Random.Range(0, totalWeight);
        int cursor = 0;

        foreach(var prefab in powerUpPrefabs)
        {
            cursor += prefab.GetComponent<Item>().spawnWeight;

            if(roll < cursor) return prefab;
        }

        return powerUpPrefabs[0];
    }
    
    private void AddToInventory(Item item)
    {
        AudioManager.instance.Play("ItemBuy");
        blackjackGame.BuyItem(item.price);
        
        var pos = item.transform.localPosition;
        var rot = item.transform.localRotation;
        var scale = item.transform.localScale;

        foreach (var spawnPoint in useSpawnPoints)
        {
            if (spawnPoint.transform.childCount == 0)
            {
                item.transform.parent = spawnPoint.transform;
                break;
            }
        }
        
        item.transform.localPosition = pos;
        item.transform.localRotation = rot;
        item.transform.localScale = scale;
        
        inventoryItems++;
    }

    private void DeactivateShopItems()
    {
        if (inventoryItems < useSpawnPoints.Count) return;
        
        foreach (var spawnPoint in buySpawnPoints)
        {
            if (spawnPoint.transform.childCount != 0)
            {
                var item = spawnPoint.transform.GetChild(0).GetComponent<Item>();
                item.SetActive(false);
                item.OnRemoveOutline();
            }
        }
    }

    private bool HasEnoughMoney(Item item)
    {
        if (blackjackGame.PlayerMoney > item.price) return true;

        if (Time.time >= nextDenyTime)
        {
            AudioManager.instance.Play("ItemDeny");
            nextDenyTime = Time.time + denySoundCooldown;
        }
        return false;
    }
    
    #endregion
}
