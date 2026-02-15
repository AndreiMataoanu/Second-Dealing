using System.Collections.Generic;
using UnityEngine;

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private List<Clickable> roundActiveClickables;
    [SerializeField] private List<Clickable> roundInactiveClickables;
    
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

    public void OnDealerTurn()
    {
        SetClickables(roundActiveClickables, false);
        SetClickables(roundInactiveClickables, false);
    }

    private void SetClickables(List<Clickable> clickables, bool isActive)
    {
        foreach (var clickable in clickables)
        {
            clickable.SetActive(isActive);
            clickable.OnRemoveOutline();
        }
    }

    public void AddRoundActiveClickable(Clickable clickable)
    {
        roundActiveClickables.Add(clickable);
    }
}
