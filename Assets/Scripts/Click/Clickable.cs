using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : MonoBehaviour
{
    [SerializeField] private UnityEvent clickEvent;
    [SerializeField] private Material outline;

    [Header("Tooltip Settings")]
    [SerializeField] public string tooltipHeader;
    [SerializeField] private string tooltipContent;

    private MeshCollider meshCollider;
    private Renderer meshRenderer;
    private bool hasOutline;
    
    protected bool IsActive;

    public void SetActive(bool active) => IsActive = active;
    
    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        
        meshRenderer = GetComponent<Renderer>();
    }

    protected virtual void OnMouseEnter()
    {
        if (!outline || !IsActive) return;
        
        var materials = new List<Material>(meshRenderer.materials) { outline };
        meshRenderer.materials = materials.ToArray();
        hasOutline = true;

        TooltipManager.instance.ShowTooltip(GetTooltipContent(), tooltipHeader);
    }

    protected virtual void OnMouseExit()
    {
        if (!IsActive) return;
        OnRemoveOutline();
    }

    public void OnRemoveOutline()
    {
        if (!outline || !hasOutline) return;
        
        var materials = new List<Material>(meshRenderer.materials);
        materials.RemoveAt(materials.Count - 1);
        meshRenderer.materials = materials.ToArray();
        hasOutline = false;

        TooltipManager.instance.HideTooltip();
    }

    public virtual void OnClick(int mouseButton = 0)
    {
        if (!IsActive) return;

        if(mouseButton == 0)
        {
            clickEvent?.Invoke();
        }

        OnRemoveOutline();
    }

    protected void ApplyOutline()
    {
        if(!outline || hasOutline || meshRenderer == null) return;

        var materials = new List<Material>(meshRenderer.materials) { outline };

        meshRenderer.materials = materials.ToArray();
        hasOutline = true;
    }

    protected virtual string GetTooltipContent()
    {
        return tooltipContent;
    }
}
