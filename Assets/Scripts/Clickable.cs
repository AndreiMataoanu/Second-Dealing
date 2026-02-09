using UnityEngine;
using UnityEngine.Events;

public class Clickable : MonoBehaviour
{
    [SerializeField] private UnityEvent clickEvent;
    private MeshCollider meshCollider;
    
    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    public void OnClick() => clickEvent?.Invoke();
}
