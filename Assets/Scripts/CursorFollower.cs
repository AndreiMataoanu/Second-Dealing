using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float distanceFromCamera = 0.8f;

    private void LateUpdate()
    {
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z = distanceFromCamera;

        transform.position = mainCamera.ScreenToWorldPoint(mousePosition);
    }
}