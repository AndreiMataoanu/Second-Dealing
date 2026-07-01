using UnityEngine;

namespace Prefabs.Managers
{
    public enum CursorType
    {
        Scissors,
        Acid,
        Flip
    }

    public class CursorFollowManager : MonoBehaviour
    {
        [SerializeField] private GameObject scissorsFollow;
        [SerializeField] private GameObject acidFollow;
        [SerializeField] private GameObject handFollow;

        public void SetCursorTypeActive(bool isActive, CursorType cursorType)
        {
            Cursor.visible = !isActive;

            switch (cursorType)
            {
                case CursorType.Scissors:
                    scissorsFollow?.SetActive(isActive);
                    break;
                case CursorType.Acid:
                    acidFollow?.SetActive(isActive);
                    break;
                case CursorType.Flip:
                    handFollow?.SetActive(isActive);
                    break;
            }
        }
    }
}