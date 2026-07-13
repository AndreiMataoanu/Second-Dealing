using System.Collections.Generic;
using UnityEngine;

public class SimpleCursorDetect : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private List<Clickable> clickables;
    
    private void Awake()
    {
        SetClickables(clickables,true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void Update()
    {
        int mouseButton = -1;

        if(Input.GetMouseButtonDown(0)) mouseButton = 0;
        else if(Input.GetMouseButtonDown(1)) mouseButton = 1;

        if (mouseButton != -1)
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("wauw");
                var clickable = hit.transform.GetComponent<Clickable>();

                if(clickable)
                {
                    clickable.OnClick(mouseButton);
                }
            }
        }
    }

    private void SetClickables(List<Clickable> clickables, bool isActive)
    {
        foreach (var clickable in clickables)
        {
            clickable.SetActive(isActive);
            clickable.OnRemoveOutline();
        }
    }

}
