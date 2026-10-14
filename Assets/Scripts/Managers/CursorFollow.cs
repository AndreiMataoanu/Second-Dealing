using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
        [Header("Camera")]
        [SerializeField] private Camera mainCamera;
        
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

        public void UseCursorAtPosition(bool isActive, CursorType cursorType, Vector3? worldPosition)
        {
            StartCoroutine(UseCursorAtPositionCoroutine(isActive, cursorType, worldPosition));
        }
        
        private IEnumerator UseCursorAtPositionCoroutine(bool isActive, CursorType cursorType, Vector3? worldPosition)
        {
            if (worldPosition == null) yield break;
            
            Cursor.visible = false;
            SetMousePosition((Vector3)worldPosition);

            yield return new WaitForSeconds(0.1f);
            
            SetCursorTypeActive(isActive, cursorType);
        }
        
        private void SetMousePosition(Vector3 worldPosition)
        {
            if (!mainCamera) return;
            
            var screenPoint = mainCamera.WorldToScreenPoint(worldPosition);
            Mouse.current.WarpCursorPosition(screenPoint);
        }
        
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