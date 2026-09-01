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
        AntiMatter,
        HiddenAce,
        SprayCan,
        Dart,
        None
    }

    public class CursorFollow : MonoBehaviour
    {
        [Header("Cursor types")]
        [SerializeField] private GameObject scissorsFollow;
        [SerializeField] private GameObject acidFollow;
        [SerializeField] private GameObject handFollow;
        [SerializeField] private GameObject tokenFollow;
        [SerializeField] private GameObject antiMatterFollow;
        [SerializeField] private GameObject pyroFollow;
        [SerializeField] private GameObject sprayCanFollow;
        [SerializeField] private GameObject dartFollow;

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
                case CursorType.HiddenAce:
                    tokenFollow?.SetActive(isActive);
                    break;
                case CursorType.AntiMatter:
                    antiMatterFollow?.SetActive(isActive);
                    break;
                case CursorType.Pyro:
                    pyroFollow?.SetActive(isActive);
                    break;
                case CursorType.SprayCan:
                    sprayCanFollow?.SetActive(isActive);
                    break;
                case CursorType.Dart:
                    dartFollow?.SetActive(isActive);
                    break;
            }
        }
    }
}