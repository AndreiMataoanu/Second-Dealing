using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PowerUpShop : MonoBehaviour
{
    [Header("Spawning Power Ups")]
    [SerializeField] private float spaceOffset = 3.0f;
    [SerializeField] private int powerUpCount = 3;
    [SerializeField] private GameObject[] powerUpPrefabs;

    [Header("Power Up Selection")]
    [SerializeField] private Color outlineColor = new Color(0.4f, 0.0f, 0.7f);
    [SerializeField] private float outlineWidth = 5.0f;
    //[SerializeField] private float disappearTime = 1.0f;
    [SerializeField] private GameObject blackjackGameManager;

    private Transform _highlight;
    private Transform _selection;

    private RaycastHit _raycastHit;

    private BlackjackGame _blackjackGame;
    private InventoryManagement _inventoryManagement;

    private float denySoundCooldown = 0.3f;
    private float nextDenyTime = 0f;

    [HideInInspector] public bool hasSelected;
    private bool _hasSpawned = false;
    private bool itemBought = false;

    private void Awake()
    {
        _blackjackGame = blackjackGameManager.GetComponent<BlackjackGame>();
        _inventoryManagement = GetComponent<InventoryManagement>();

        if(powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount) Debug.Log("Not enough power up prefabs added!");
    }

    private void Update()
    {
        HighlightPowerUp();
        SelectPowerUp();
    }

    public void SpawnPowerUps()
    {
        if(_hasSpawned || powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount) return;

        for(int i = 0; i < powerUpCount; i++)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);

            Vector3 prefabPosition = transform.position + Vector3.up * (i * spaceOffset);

            GameObject prefab = Instantiate(powerUpPrefabs[randomIndex], prefabPosition, Quaternion.identity, transform);

            prefab.GetComponent<PowerUpInfo>().SetBlackjackGame(_blackjackGame);
        }

        _hasSpawned = true;
        itemBought = false;
    }

    public void DestroyPowerUps()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        hasSelected = false;
        itemBought = false;
        _hasSpawned = false;
    }

    private void HighlightPowerUp()
    {
        if(_inventoryManagement.inInventory || hasSelected) return;
        
        if(_highlight)
        {
            _highlight.gameObject.GetComponent<Outline>().enabled = false;
            _highlight = null;
        }

        if (Camera.main)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if(!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out _raycastHit))
            {
                _highlight = _raycastHit.transform;

                if(_highlight.CompareTag($"Selectable") && _highlight != _selection)
                {
                    var outline = _highlight.gameObject.GetComponent<Outline>();

                    if(outline)
                    {
                        outline.enabled = true;
                    }
                    else
                    {
                        outline = _highlight.gameObject.AddComponent<Outline>();
                        outline.enabled = true;
                        outline = _highlight.gameObject.GetComponent<Outline>();
                        outline.OutlineColor = outlineColor;
                        outline.OutlineWidth = outlineWidth;
                    }
                }
                else
                {
                    _highlight = null;
                }
            }
        }
    }

    private void SelectPowerUp()
    {
        if(_inventoryManagement.inInventory) return;

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(itemBought)
            {
                if(Time.time >= nextDenyTime)
                {
                    AudioManager.instance.Play("ItemDeny");

                    nextDenyTime = Time.time + denySoundCooldown;
                }

                return;
            }

            if(!hasSelected && _highlight)
            {
                _selection = _raycastHit.transform;
                _selection.gameObject.GetComponent<Outline>().enabled = false;
                _highlight = null;
                
                BuySelectedPowerUp();

                _blackjackGame.UpdateBettingUI();

                CameraController.instance.EnterDefault();
            }
        }
    }

    private void BuySelectedPowerUp()
    {
        var selectionInfo = _selection.gameObject.GetComponent<PowerUpInfo>();

        if(!HasEnoughMoney(selectionInfo)) return;

        if(_inventoryManagement.AddItem(_selection.gameObject))
        {
            AudioManager.instance.Play("ItemBuy");

            _blackjackGame.LoseAmount(selectionInfo.price);

            itemBought = true;
            hasSelected = true;
        }
    }

    private bool HasEnoughMoney(PowerUpInfo selectionInfo)
    {
        if(_blackjackGame.PlayerMoney <= selectionInfo.price)
        {
            AudioManager.instance.Play("ItemDeny");

            _selection = null;
            hasSelected = false;

            return false;
        }

        hasSelected = true;

        return true;
    }
}
