using UnityEngine;

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private Clickable[] roundActiveClickables;
    [SerializeField] private Clickable[] roundInactiveClickables;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            var ray = camera.ScreenPointToRay(mousePosition);

            RaycastHit raycastHit;

            bool hasHit = Physics.Raycast(ray, out raycastHit);
            if (hasHit)
            {
                raycastHit.transform.GetComponent<Clickable>()?.OnClick();
            }
        }
    }

    public void OnRoundActive()
    {
        SetClickables(roundActiveClickables, true);
        SetClickables(roundInactiveClickables, false);
    }

    public void OnRoundInactive()
    {
        SetClickables(roundActiveClickables, false);
        SetClickables(roundInactiveClickables, true);
    }

    private void SetClickables(Clickable[] clickables, bool isActive)
    {
        foreach (var clickable in clickables)
            clickable.SetActive(isActive);
    }
}
