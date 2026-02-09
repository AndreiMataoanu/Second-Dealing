using UnityEngine;

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private Camera camera;

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
                Debug.Log("Detected something");
                raycastHit.transform.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                Debug.Log("Nothing detected");
            }
        }
    }
}
