using UnityEngine;

public class CursorFollower : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float distanceFromCamera = 0.8f;
    [SerializeField] private Vector2 positionOffset = Vector2.zero;

    private void LateUpdate()
    {
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.x += positionOffset.x;
        mousePosition.y += positionOffset.y;
        mousePosition.z = distanceFromCamera;

        transform.position = mainCamera.ScreenToWorldPoint(mousePosition);
    }
}