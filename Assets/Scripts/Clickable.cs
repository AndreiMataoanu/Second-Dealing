using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : MonoBehaviour
{
    [SerializeField] private UnityEvent clickEvent;
    [SerializeField] private Material outline;
    
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private bool hasOutline;
    
    protected bool IsActive;

    public void SetActive(bool active) => IsActive = active;
    
    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnMouseEnter()
    {
        if (!outline || !IsActive) return;
        
        var materials = new List<Material>(meshRenderer.materials) { outline };
        meshRenderer.materials = materials.ToArray();
        hasOutline = true;
    }

    private void OnMouseExit()
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
    }

    public virtual void OnClick()
    {
        if (!IsActive) return;
        clickEvent?.Invoke();
        OnRemoveOutline();
    }
}
