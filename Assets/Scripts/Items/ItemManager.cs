using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

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

    [Header("Suitcase")] 
    [SerializeField] private Animator suitcaseAnimator;
    private Item currentPassive;
    private int inventoryItems = 0;
    private float nextDenyTime = 0;

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

        if (inventoryItems == buySpawnPoints.Count)
        {
            DespawnPowerUps();
            StartCoroutine(SuitcaseCloseCoroutine());
        }
        
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
            if (item.type != ItemType.Scissors)
                AudioManager.instance.Play(item.name);
            else
                AudioManager.instance.Play("ItemBuy");
            TooltipManager.instance.HideTooltip();
            Destroy(item.gameObject);
            inventoryItems--;
        }
    }
    public void IsPassiveDone(bool passiveUsed)
    {
        if(currentPassive != null)
        {
            if(passiveUsed || (currentPassive.type != ItemType.Lotto && currentPassive.PassiveItemRounds == roundsSincePassiveBought))
            {
                Destroy(currentPassive.gameObject);
                inventoryItems--;
                roundsSincePassiveBought = 0;
                currentPassive = null;
            }
            else if(currentPassive.type != ItemType.Lotto)
            {
                roundsSincePassiveBought ++;
            }    
        } 
    }

    #region Helper Methods
    private GameObject GetWeightedRandomPrefab()
    {
        bool hasTicket = currentPassive != null && currentPassive.type == ItemType.Lotto;
        int currentTotalWeight = 0;

        List<GameObject> validPrefabs = new List<GameObject>();

        foreach(var prefab in powerUpPrefabs)
        {
            var powerUp = prefab.GetComponent<Item>();

            if(hasTicket && powerUp.type == ItemType.Lotto) continue;

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
        if (blackjackGame.PlayerMoney > item.price) return true;

        if (Time.time >= nextDenyTime)
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
}
