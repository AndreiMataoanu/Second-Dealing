using UnityEngine;

namespace Managers
{
    public enum CursorType
    {
        Scissors,
        Acid,
        Flip,
        Pyro,
        HatTrick,
        Antimatter,
        None
    }

    public class CursorFollow : MonoBehaviour
    {
        [Header("Cursor types")]
        [SerializeField] private GameObject scissorsFollow;
        [SerializeField] private GameObject acidFollow;
        [SerializeField] private GameObject handFollow;

        [Header("Deactivate")] 
        [SerializeField] private GameObject rightHand;
        
        public void SetCursorTypeActive(bool isActive, CursorType cursorType)
        {
            if (cursorType == CursorType.None) return;
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
                    rightHand?.SetActive(!isActive);
                    break;
            }
        }
    }
}