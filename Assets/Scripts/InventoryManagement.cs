using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryManagement : MonoBehaviour
{
    [Header("Inventory Info")]
    [SerializeField] private Vector3[] itemsPositions;
    [SerializeField] private GameObject inventory;

    [Header("Power-up Selection")]
    public List<GameObject> _powerUps;

    [HideInInspector] public bool inInventory = false;

    private Transform _selection;

    private RaycastHit _raycastHit;
    
    private void Update()
    {
        SelectPowerUp();
    }

    public bool AddItem(GameObject powerUp)
    {
        if(!powerUp) Debug.Log("Power-up is null.");

        if(itemsPositions.Length < _powerUps.Count + 1) return false;
        
        _powerUps.Add(powerUp);

        powerUp.transform.position = itemsPositions[_powerUps.Count - 1];
        powerUp.transform.Rotate(Vector3.up, 90);
        powerUp.transform.SetParent(inventory.transform, true);
        
        return true;
    }

    public void UseItem(GameObject powerUp)
    {
        powerUp.GetComponent<PowerUpInfo>().Activate();

        AudioManager.instance.Play(powerUp.name);
        _powerUps.Remove(powerUp);

        Destroy(powerUp);
        ArrangeItems();

        TooltipManager.instance.HideTooltip();
        CameraController.instance.EnterDefault();
    }

    public void ArrangeItems()
    {
        for(int i = 0; i < _powerUps.Count; i++)
        {
            _powerUps[i].transform.position = itemsPositions[i];
        }
    }
    
    private void SelectPowerUp()
    {
        if(!inInventory) return;
        
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out _raycastHit))
            {
                if(_raycastHit.transform.CompareTag("Selectable"))
                {
                    _selection = _raycastHit.transform;

                    UseItem(_selection.gameObject);
                }
            }
        }
    }
}
