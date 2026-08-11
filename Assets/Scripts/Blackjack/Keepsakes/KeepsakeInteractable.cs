using System.Collections.Generic;
using UnityEngine;

public class KeepsakeInteractable : Clickable
{
    [SerializeField] private Keepsake keepsake;
    [SerializeField] private BlackjackGame blackjackGame;
    [SerializeField] private Material lockedMaterial;
    [SerializeField] private Color outlineSelect = Color.green;
    [SerializeField] private Color outlineFull = Color.red;
    [SerializeField] private Color outlineDeselect = Color.purple;
    [SerializeField] public Vector3 rotationInHand;
    [SerializeField] public Vector3 scaleInHand;

    private Renderer[] allRenderers;
    private Dictionary<Renderer, Material[]> originalMaterialsDict = new Dictionary<Renderer, Material[]>();

    public void SetBlackjackGame(BlackjackGame game) => blackjackGame = game;
    public Keepsake GetKeepsake() => keepsake;

    private void Start()
    {
        allRenderers = GetComponentsInChildren<Renderer>(true);

        foreach(var r in allRenderers)
        {
            originalMaterialsDict[r] = r.materials;
        }

        KeepsakeUnlockProgression.instance.OnProgressChanged += UpdateVisuals;

        UpdateVisuals();
    }

    private void OnDestroy()
    {
        KeepsakeUnlockProgression.instance.OnProgressChanged -= UpdateVisuals;
    }

    private void UpdateVisuals()
    {
        bool isLocked = !KeepsakeUnlockProgression.instance.HasMetRequirement(keepsake);
        bool isEquipped = KeepsakeManager.instance.equippedKeepsakes.Contains(keepsake);

        if(isLocked || isEquipped)
        {
            OnRemoveOutline(true);

            foreach(var r in allRenderers)
            {
                Material[] lockedMats = new Material[originalMaterialsDict[r].Length];

                for(int i = 0; i < lockedMats.Length; i++)
                {
                    lockedMats[i] = lockedMaterial;
                }

                r.materials = lockedMats;
            }
        }
        else
        {
            OnRemoveOutline(true);

            foreach(var r in allRenderers)
            {
                r.materials = originalMaterialsDict[r];
            }
        }
    }

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        base.OnClick(mouseButton);

        if(!KeepsakeUnlockProgression.instance.HasMetRequirement(keepsake))
        {
            AudioManager.instance.Play("ItemDeny");

            return;
        }

        if(KeepsakeManager.instance.equippedKeepsakes.Contains(keepsake))
        {
            KeepsakeManager.instance.UnequipKeepsake(keepsake);
            AudioManager.instance.Play("ItemBuy");

            UpdateVisuals();
        }
        else
        {
            bool equipped = KeepsakeManager.instance.EquipKeepsake(keepsake);

            if(equipped)
            {
                AudioManager.instance.Play("ItemBuy");

                keepsake.ApplyInheritance(blackjackGame);

                UpdateVisuals();
            }
            else
            {
                AudioManager.instance.Play("ItemDeny");
            }
        }
    }

    protected override string GetTooltipHeader()
    {
        return keepsake.keepsakeName;
    }

    protected override string GetTooltipContent()
    {
        if(!KeepsakeUnlockProgression.instance.HasMetRequirement(keepsake))
        {
            int currentProgress = KeepsakeUnlockProgression.instance.GetProgress(keepsake.requiredChallenge);

            return $"\n{keepsake.description}\n\nUnlock: {keepsake.unlockDescription} ({currentProgress}/{keepsake.requiredTarget})";
        }

        if(this.name.Contains("Inheritance"))
        {
            return keepsake.GetDescription();
        }

        return $"\n{keepsake.description}";
    }

    protected override Color GetOutlineColor()
    {
        bool requirementMet = KeepsakeUnlockProgression.instance.HasMetRequirement(keepsake);

        if(!requirementMet)
        {
            return base.GetOutlineColor();
        }

        if(KeepsakeManager.instance.equippedKeepsakes.Contains(keepsake))
        {
            return outlineDeselect;
        }

        bool isFull = KeepsakeManager.instance.IsKeepsakeEquipFull;

        if(isFull)
        {
            return outlineFull;
        }

        return outlineSelect;
    }
}