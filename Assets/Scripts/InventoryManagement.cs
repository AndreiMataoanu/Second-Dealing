using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryManagement : MonoBehaviour
{
    [Header("Inventory Info")]
    [SerializeField] private Vector3[] itemsPositions;
    [SerializeField] private GameObject inventory;
    [SerializeField] private AudioManager audioManager;

    [Header("Power-up Selection")]
    [SerializeField] private Color outlineColor = new Color(0.4f, 0.0f, 0.7f);
    [SerializeField] private float outlineWidth = 5.0f;
    public List<GameObject> _powerUps;

    [HideInInspector] public bool inInventory = false;
    private Transform _highlight;
    private Transform _selection;
    private RaycastHit _raycastHit;
    
    private void Update()
    {
        HighlightPowerUp();
        SelectPowerUp();
    }

    public bool AddItem(GameObject powerUp)
    {
        if (!powerUp) Debug.Log("Power-up is null.");
        if (itemsPositions.Length < _powerUps.Count + 1) return false;
        
        _powerUps.Add(powerUp);
        powerUp.transform.position = itemsPositions[_powerUps.Count - 1];
        powerUp.transform.Rotate(Vector3.up, 90);
        powerUp.transform.SetParent(inventory.transform, true);
        
        return true;
    }

    public void UseItem(GameObject powerUp)
    {
        powerUp.GetComponent<PowerUpInfo>().Activate();
        audioManager.Play(powerUp.name);
        _powerUps.Remove(powerUp);
        Destroy(powerUp);
        ArrangeItems();
    }

    public void ArrangeItems()
    {
        for (int i = 0; i < _powerUps.Count; i++)
        {
            _powerUps[i].transform.position = itemsPositions[i];
        }
    }
    
    private void HighlightPowerUp()
    {
        if (!inInventory) return;
        
        if (_highlight)
        {
            _highlight.gameObject.GetComponent<Outline>().enabled = false;
            _highlight = null;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out _raycastHit))
        {
            _highlight = _raycastHit.transform;
            if (_highlight.CompareTag($"Selectable") && _highlight != _selection)
            {
                var outline = _highlight.gameObject.GetComponent<Outline>();
                if (outline) outline.enabled = true;
                else
                {
                    outline = _highlight.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                    
                    outline = _highlight.gameObject.GetComponent<Outline>();
                    outline.OutlineColor = outlineColor;
                    outline.OutlineWidth = outlineWidth;
                }
            }
            else _highlight = null;
        }
    }
    
    private void SelectPowerUp()
    {
        if (!inInventory) return;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_highlight)
            {
                _selection = _raycastHit.transform;
                _selection.gameObject.GetComponent<Outline>().enabled = false;
                _highlight = null;

                UseItem(_selection.gameObject);
                // TODO: After using, change camera direction
            }
        }
    }
    
}
