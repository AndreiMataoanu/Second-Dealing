using UnityEngine;
using System.Collections.Generic;

public class KeepsakeManager : MonoBehaviour
{
    public static KeepsakeManager instance;

    [SerializeField] private List<Transform> tableSpawnPoints;
    public int maxKeepsakes = 3;
    private List<GameObject> currentTableObjects = new List<GameObject>();
    public List<Keepsake> equippedKeepsakes = new List<Keepsake>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool EquipKeepsake(Keepsake keepsake)
    {
        if (equippedKeepsakes.Count >= maxKeepsakes) return false;

        if (equippedKeepsakes.Contains(keepsake)) return false;

        if (IsKeepsakeTypeEquipped(keepsake)) return false;

        equippedKeepsakes.Add(keepsake);

        UpdateTableVisuals();

        return true;
    }

    private bool IsKeepsakeTypeEquipped(Keepsake keepsake)
    {
        foreach (var k in equippedKeepsakes)
        {
            if (k.GetType() == keepsake.GetType())
                return true;
        }

        return false;
    }

    public void UnequipKeepsake(Keepsake keepsake)
    {
        if(equippedKeepsakes.Remove(keepsake))
        {
            keepsake.Deactivate();
            UpdateTableVisuals();
        }
    }

    private void UpdateTableVisuals()
    {
        foreach(GameObject obj in currentTableObjects)
        {
            if(obj != null) Destroy(obj);
        }

        currentTableObjects.Clear();

        for(int i = 0; i < equippedKeepsakes.Count; i++)
        {
            Keepsake keepsake = equippedKeepsakes[i];

            if(keepsake.tablePrefab != null && tableSpawnPoints != null && i < tableSpawnPoints.Count && tableSpawnPoints[i] != null)
            {
                GameObject tableObj = Instantiate(keepsake.tablePrefab, tableSpawnPoints[i]);
                tableObj.transform.localPosition = Vector3.zero;
                tableObj.transform.localRotation = Quaternion.identity;

                currentTableObjects.Add(tableObj);

                TableKeepsakeInteractable interactable = tableObj.GetComponent<TableKeepsakeInteractable>();

                if(interactable != null)
                {
                    interactable.SetKeepsake(keepsake);
                }
            }
        }
    }

    public int ApplyPayoutModifiers(int payout, List<List<CardInstance>> allHands)
    {
        int currentPayout = payout;

        foreach(var keepsake in equippedKeepsakes)
        {
            currentPayout = keepsake.ModifyPayout(currentPayout, allHands);
        }

        return currentPayout;
    }

    public void ResetKeepsake()
    {
        foreach(var keepsake in equippedKeepsakes)
        {
            keepsake.OnRoundStart();
        }

        foreach(GameObject tableObject in currentTableObjects)
        {
            TableKeepsakeInteractable interactable = tableObject.GetComponent<TableKeepsakeInteractable>();

            interactable.ResetUse();
        }
    }

    public void DeactivateKeepsakes()
    {
        AntiMatter.isAntiMatterActive = false;
        Pyro.isPyroActive = false;
        HatTrick.isHatTrickActive = false;
    }

    public void OnDealPlayerCard(CardInstance cardInstance)
    {
        equippedKeepsakes.ForEach(keepsake => keepsake.OnDealPlayerCard(cardInstance));
    }

    public void OnAdvanceHand()
    {
        equippedKeepsakes.ForEach(keepsake => keepsake.OnAdvanceHand());
    }

    public bool AllowAnySplit()
    {
        foreach(var keepsake in equippedKeepsakes)
        {
            if(keepsake.AllowAnySplit()) return true;
        }

        return false;
    }

    public int GetDealerBustModifier()
    {
        int total = 0;

        foreach(var keepsake in equippedKeepsakes)
        {
            total += keepsake.GetDealerBustModifier();
        }

        return total;
    }

    public bool AllowEndlessDoubleDown()
    {
        foreach(var keepsake in equippedKeepsakes)
        {
            if(keepsake.AllowEndlessDoubleDown()) return true;
        }

        return false;
    }

    public bool AllowOverdraft()
    {
        foreach(var keepsake in equippedKeepsakes)
        {
            if(keepsake.AllowOverdraft()) return true;
        }

        return false;
    }

    public int GetPassiveIncome()
    {
        int total = 0;

        foreach(var keepsake in equippedKeepsakes)
        {
            total += keepsake.GetPassiveIncome();
        }

        return total;
    }

    public bool ConsumeKeepsake()
    {
        for(int i = 0; i < equippedKeepsakes.Count; i++)
        {
            if(equippedKeepsakes[i].Consume())
            {
                equippedKeepsakes.RemoveAt(i);

                UpdateTableVisuals();

                return true;
            }
        }

        return false;
    }

    public bool AllowPostStandItem(BlackjackGame game)
    {
        foreach(var keepsake in equippedKeepsakes)
        {
            if(keepsake.AllowPostStandItem(game)) return true;
        }

        return false;
    }

    public void ApplyInheritance(BlackjackGame game)
    {
        foreach(var keepsake in equippedKeepsakes)
        {
            keepsake.ApplyInheritance(game);
        }
    }

    public void RechargeSecondDealing()
    {
        foreach (var keepsake in equippedKeepsakes)
        {
            if (keepsake is SecondDealing secondDealing)
                secondDealing.Recharge();
        }
    }
}