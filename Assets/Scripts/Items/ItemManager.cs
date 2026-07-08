using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

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
    [HideInInspector] public int organRoundsLeft = 0;

    [Header("Suitcase")] 
    [SerializeField] private Animator suitcaseAnimator;
    private int inventoryItems = 0;
    private float nextDenyTime = 0;
    private float coinMultiplier = 1.0f;

    public void PlaySuitcaseOpen()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count == 0 || inventoryItems == useSpawnPoints.Count) return;

        StartCoroutine(SuitcaseOpenCoroutine());
        SpawnPowerUps();
    }

    public void SpawnPowerUps()
    {
        foreach (var buySpawnPoint in buySpawnPoints.ToList())
        {
            var prefab = GetWeightedRandomPrefab();
            var prefabInstance = Instantiate(prefab, buySpawnPoint.transform);
            var item = prefabInstance.GetComponent<Item>();

            item.SetMultiplier(coinMultiplier);
            item.SetBlackjackGame(blackjackGame);
            item.SetActive(true);
            item.AddAction(OnBuy);
        }
    }

    public void DespawnPowerUps()
    {
        StartCoroutine(DespawnCoroutine());
    }

    private IEnumerator DespawnCoroutine()
    {
        StartCoroutine(SuitcaseCloseCoroutine());

        yield return null;
        yield return new WaitForSeconds(suitcaseAnimator.GetCurrentAnimatorStateInfo(0).length);

        foreach(var spawnPoint in buySpawnPoints)
        {
            if(spawnPoint.transform.childCount > 0)
            {
                Destroy(spawnPoint.transform.GetChild(0).gameObject);
            }
        }

        ResetShopPrices();
    }
    
    private void OnBuy(Item item)
    {
        if (inventoryItems >= useSpawnPoints.Count || !HasEnoughMoney(item)) return;

        if(item.type == ItemType.Organ && blackjackGame.isOrganActive)
        {
            AudioManager.instance.Play("ItemDeny");

            return;
        }

        AddToInventory(item);

        item.isPurchased = true;
        item.RemoveAction(OnBuy);
        item.AddAction(OnSell);
        
        if(item.type == ItemType.Organ)
        {
            blackjackGame.ActivateOrgan();
            organRoundsLeft = 2;
        }

        if (item.type == ItemType.Nft)
        {
            item.SetNftRoundsLeft();
        }

        if (inventoryItems == buySpawnPoints.Count)
        {
            DespawnPowerUps();
            StartCoroutine(SuitcaseCloseCoroutine());
        }
        
        DeactivateShopItems();
    }
    
    private void Activate(Item item)
    {
        if(!item.Activate())
        {
            if(item.type != ItemType.Organ)
            {
                AudioManager.instance.Play("ItemDeny");
            }

            return;
        }

        if(item.type != ItemType.Scissors)
        {
            AudioManager.instance.Play(item.name);
        }
        else
        {
            AudioManager.instance.Play("ItemBuy");
        }

        TooltipManager.instance.HideTooltip();

        Destroy(item.gameObject);

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.UseItems);

        inventoryItems--;
    }

    private void OnSell(Item item)
    {
        blackjackGame.SellItem(item.GetResalePrice());
        inventoryItems--;
        
        AudioManager.instance.Play("ItemBuy");
        
        switch (item.type)
        {
            case ItemType.Organ:
                blackjackGame.DeactivateOrgan();
                break;
        }
        
        Destroy(item.gameObject);
    }

    public void ChangeItemAction(bool isRoundActive)
    {
        if (inventoryItems == 0) return;
        
        foreach (var usePoint in useSpawnPoints)
        {
            if(usePoint.transform.childCount > 0)
            {
                var item = usePoint.transform.GetChild(0).gameObject.GetComponent<Item>();

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
    }

    public void OnRoundEnded()
    {
        if(blackjackGame.isOrganActive && organRoundsLeft > 0)
        {
            organRoundsLeft--;

            if(organRoundsLeft == 0)
            {
                blackjackGame.isOrganActive = false;

                AudioManager.instance.Play("OrganExpire");

                RemoveItemOfType(ItemType.Organ);
            }
        }
    }

    public void OnRoundStart()
    {
        foreach (var spawnPoint in useSpawnPoints)
        {
            if (spawnPoint.transform.childCount > 0)
            {
                Item item = spawnPoint.transform.GetChild(0).GetComponent<Item>();
                item.OnRoundStart();
            }
        }
    }

    public void RemoveItemOfType(ItemType type)
    {
        foreach(var spawnPoint in useSpawnPoints)
        {
            if(spawnPoint.transform.childCount > 0)
            {
                Item item = spawnPoint.transform.GetChild(0).GetComponent<Item>();

                if(item != null && item.type == type)
                {
                    TooltipManager.instance.HideTooltip();

                    Destroy(item.gameObject);

                    inventoryItems--;

                    break;
                }
            }
        }
    }

    #region Helper Methods
    private GameObject GetWeightedRandomPrefab()
    {
        bool hasOrgan = blackjackGame.isOrganActive;
        int currentTotalWeight = 0;

        List<GameObject> validPrefabs = new List<GameObject>();

        foreach(var prefab in powerUpPrefabs)
        {
            var powerUp = prefab.GetComponent<Item>();

            if(hasOrgan && powerUp.type == ItemType.Organ) continue;

            validPrefabs.Add(prefab);
            currentTotalWeight += powerUp.spawnWeight;
        }

        int roll = Random.Range(0, currentTotalWeight);
        int cursor = 0;

        foreach(var prefab in powerUpPrefabs)
        {
            cursor += prefab.GetComponent<Item>().spawnWeight;

            if(roll < cursor) return prefab;
        }

        return powerUpPrefabs[0];
    }
    
    private void AddToInventory(Item item, bool isFree = false)
    {
        AudioManager.instance.Play("ItemBuy");

        if(!isFree)
        {
            blackjackGame.BuyItem(item.GetPrice());
        }
        
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
        
        inventoryItems++;
    }

    private void DeactivateShopItems()
    {
        if(inventoryItems < useSpawnPoints.Count) return;
        
        foreach(var spawnPoint in buySpawnPoints)
        {
            if(spawnPoint.transform.childCount != 0)
            {
                var item = spawnPoint.transform.GetChild(0).GetComponent<Item>();

                if(item != null)
                {
                    item.SetActive(false);
                    item.OnRemoveOutline();
                } 
            }
        }
    }

    private bool HasEnoughMoney(Item item)
    {
        if(blackjackGame.PlayerMoney > item.GetPrice()) return true;

        if(Time.time >= nextDenyTime)
        {
            AudioManager.instance.Play("ItemDeny");

            nextDenyTime = Time.time + denySoundCooldown;
        }

        return false;
    }
    
    #endregion

    private IEnumerator SuitcaseOpenCoroutine()
    {
        suitcaseAnimator.Play("Suitcase_Opening");

        yield return new WaitForSeconds(0.2f);

        AudioManager.instance.Play("Latch");
        AudioManager.instance.Play("SuitcaseOpen");
    }

    private IEnumerator SuitcaseCloseCoroutine()
    {
        AudioManager.instance.Play("SuitcaseClose");

        yield return new WaitForSeconds(0.6f);

        suitcaseAnimator.Play("Suitcase_Closing");
    }

    public bool GiveSpecificItem(GameObject prefab)
    {
        if(inventoryItems >= useSpawnPoints.Count || prefab == null) return false;

        GameObject prefabInstance = Instantiate(prefab, useSpawnPoints[0].transform);
        Item item = prefabInstance.GetComponent<Item>();

        item.SetBlackjackGame(blackjackGame);

        AddToInventory(item, true);

        item.isPurchased = true;
        item.AddAction(Activate);
        item.SetActive(true);

        if(item.type == ItemType.Organ)
        {
            blackjackGame.ActivateOrgan();
            organRoundsLeft = 2;
        }

        cursorDetection.AddRoundActiveClickable(item);

        return true;
    }

    #region Coin

    // returns multiplier 0.5 - half off, 2.0 - double the price
    public bool FlipCoin()
    {
        // AudioManager.instance.Play("CoinSound");
        int coinFlip = Random.Range(0, 2);
        coinMultiplier = coinFlip == 0 ? 0.5f : 2.0f;
        
        return coinFlip == 0; // return if lucky
    }

    private void ResetShopPrices() => coinMultiplier = 1.0f;

    #endregion
}