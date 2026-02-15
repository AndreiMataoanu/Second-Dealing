using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private BlackjackGame blackjackGame;
    [SerializeField] private CursorDetection cursorDetection;
    
    [Header("Power ups")]
    [SerializeField] private List<GameObject> buySpawnPoints;
    [SerializeField] private List<GameObject> useSpawnPoints;
    [SerializeField] private List<GameObject> powerUpPrefabs;
    [SerializeField] private float denySoundCooldown = 0.3f;

    private int inventoryItems = 0;
    private float nextDenyTime = 0f;
    
    public void SpawnPowerUps()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count == 0 || inventoryItems == useSpawnPoints.Count) return;

        foreach (var buySpawnPoint in buySpawnPoints.ToList())
        {
            var randomIndex = Random.Range(0, powerUpPrefabs.Count);
            var prefab = Instantiate(powerUpPrefabs[randomIndex], buySpawnPoint.transform);
            var item = prefab.GetComponent<Item>();
            item.SetBlackjackGame(blackjackGame);
            item.SetActive(true);
            item.AddAction(OnBuy);
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
        
        AddToInventory(item);
        item.RemoveAction(OnBuy);
        item.AddAction(Activate);
        item.SetActive(false);
        cursorDetection.AddRoundActiveClickable(item);

        DeactivateShopItems();
    }
    
    private void Activate(Item item)
    {
        if (!blackjackGame) return;
        
        switch(item.type)
        {
            case PowerUpType.Knife:
                blackjackGame.ActivateKnife();
                break;
            case PowerUpType.Scissors:
                blackjackGame.ActivateScissors();
                break;
            case PowerUpType.Crucifix:
                blackjackGame.ActivatePrayerBeads();
                break;
            case PowerUpType.Sunglasses:
                blackjackGame.ActivateSunglasses();
                break;
            default:
                return;
        }

        inventoryItems--;
        AudioManager.instance.Play(item.name);
        TooltipManager.instance.HideTooltip();
        Destroy(item.gameObject);
    }

    #region Helper Methods

    private void AddToInventory(Item item)
    {
        AudioManager.instance.Play("ItemBuy");
        blackjackGame.LoseAmount(item.price);
        
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
