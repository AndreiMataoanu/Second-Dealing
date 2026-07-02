using UnityEngine;
using System.Collections.Generic;

public class KeepsakeManager : MonoBehaviour
{
    public static KeepsakeManager instance;

    [SerializeField] private Transform tableSpawnPoint;
    private GameObject currentTableObject;
    public Keepsake _equippedKeepsake;

    public Keepsake equippedKeepsake
    {
        get { return _equippedKeepsake; }
        set
        {
            _equippedKeepsake = value;

            UpdateTableVisuals();
        }
    }

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

    private void UpdateTableVisuals()
    {
        if(currentTableObject != null)
        {
            Destroy(currentTableObject);
        }

        if(_equippedKeepsake != null && _equippedKeepsake.tablePrefab != null && tableSpawnPoint != null)
        {
            currentTableObject = Instantiate(_equippedKeepsake.tablePrefab, tableSpawnPoint);
            currentTableObject.transform.localPosition = Vector3.zero;
            currentTableObject.transform.localRotation = Quaternion.identity;
        }
    }

    public int ApplyPayoutModifiers(int payout, List<List<CardInstance>> allHands)
    {
        if(equippedKeepsake == null) return payout;

        return equippedKeepsake.ModifyPayout(payout, allHands);
    }

    public void ResetKeepsake()
    {
        if(equippedKeepsake != null)
        {
            equippedKeepsake.OnRoundStart();
        }
    }

    public bool AllowsAnySplit()
    {
        if(equippedKeepsake == null) return false;

        return equippedKeepsake.AllowAnySplit();
    }

    public int GetDealerBustModifier()
    {
        if(equippedKeepsake == null) return 0;

        return equippedKeepsake.GetDealerBustModifier();
    }

    public bool AllowsEndlessDoubleDown()
    {
        if(equippedKeepsake == null) return false;

        return equippedKeepsake.AllowEndlessDoubleDown();
    }

    public bool AllowsOverdraft()
    {
        if(equippedKeepsake == null) return false;

        return equippedKeepsake.AllowOverdraft();
    }

    public int GetPassiveIncome()
    {
        if(equippedKeepsake == null) return 0;

        return equippedKeepsake.GetPassiveIncome();
    }

    public bool TryConsumeKeepsake()
    {
        if(equippedKeepsake == null) return false;

        bool isConsumed = equippedKeepsake.Consume();

        if(isConsumed)
        {
            equippedKeepsake = null;

            return true;
        }

        return false;
    }
}