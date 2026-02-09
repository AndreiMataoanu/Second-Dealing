using UnityEngine;

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            var ray = camera.ScreenPointToRay(mousePosition);

            RaycastHit raycastHit;

            bool hasHit = Physics.Raycast(ray, out raycastHit);
            if (hasHit)
                raycastHit.transform.GetComponent<Clickable>()?.OnClick();
        }
    }
}
