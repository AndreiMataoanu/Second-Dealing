using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : MonoBehaviour
{
    [SerializeField] private UnityEvent clickEvent;
    [SerializeField] private Material outline;
    
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private bool isActive;

    public void SetActive(bool active) => isActive = active;
    
    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnMouseEnter()
    {
        if (!outline || !isActive) return;
        
        var materials = new List<Material>(meshRenderer.materials) { outline };
        meshRenderer.materials = materials.ToArray();
    }

    private void OnMouseExit()
    {
        if (!isActive) return;
        OnRemoveOutline();
    }

    public void OnRemoveOutline()
    {
        if (!outline) return;
        
        var materials = new List<Material>(meshRenderer.materials);
        materials.RemoveAt(materials.Count - 1);
        meshRenderer.materials = materials.ToArray();
    }
    
    public virtual void OnClick() => clickEvent?.Invoke();
}
