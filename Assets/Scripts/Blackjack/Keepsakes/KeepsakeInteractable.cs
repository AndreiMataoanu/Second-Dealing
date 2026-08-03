using UnityEngine;

public class KeepsakeInteractable : Clickable
{
    [SerializeField] private Keepsake keepsake;
    [SerializeField] private BlackjackGame blackjackGame;
    [SerializeField] private Material lockedMaterial;

    private Renderer objectRenderer;
    private Material[] originalMaterials;

    public void SetBlackjackGame(BlackjackGame game) => blackjackGame = game;
    public Keepsake GetKeepsake() => keepsake;
    
    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalMaterials = objectRenderer.materials;

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

        if(isLocked)
        {
            OnRemoveOutline(true);

            Material[] lockedMats = new Material[originalMaterials.Length];

            for(int i = 0; i < lockedMats.Length; i++)
            {
                lockedMats[i] = lockedMaterial;
            }

            objectRenderer.materials = lockedMats;
        }
        else if(!isLocked)
        {
            OnRemoveOutline(true);

            objectRenderer.materials = originalMaterials;
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
        }
        else
        {
            bool equipped = KeepsakeManager.instance.EquipKeepsake(keepsake);

            if(equipped)
            {
                AudioManager.instance.Play("ItemBuy");

                keepsake.ApplyInheritance(blackjackGame);
                gameObject.SetActive(false);
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
}