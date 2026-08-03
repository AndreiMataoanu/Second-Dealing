using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMilestoneKeepsake : MonoBehaviour
{
    [SerializeField] private List<Transform> keepsakePositions;
    [SerializeField] private GameObject dealerHand;
    [SerializeField] private Transform selectHandPosition;
    
    [HideInInspector] public bool isChoosing;

    private BlackjackGame game;
    private ShopManager shop;
    private DialogueSystem dialogue;
    
    private Vector3 startHandPosition;
    private List<KeepsakeInteractable> interactables = new();

    private void Start()
    {
        dealerHand.SetActive(false);
        startHandPosition = dealerHand.transform.position;
    }

    public void SetBlackjackGame(BlackjackGame blackjackGame)
    {
        game = blackjackGame;
        shop = blackjackGame.ShopManager;
        dialogue = blackjackGame.DialogueSystem;
    }

    public IEnumerator PresentKeepsakeChoice(List<GameObject> keepsakes)
    {
        if (!SpawnKeepsakes(keepsakes)) yield break;

        dealerHand.SetActive(true);
        yield return KeepsakeChoiceCoroutine();
        dealerHand.SetActive(false);
    }

    private IEnumerator KeepsakeChoiceCoroutine()
    {
        yield return new WaitForSeconds(1f);

        shop.SetDelayOpen(true);
        dialogue.ShowKeepsakeTaunts();
        
        yield return MoveHand(1.3f, startHandPosition, selectHandPosition.position);

        isChoosing = true;
        yield return new WaitUntil(() => isChoosing == false);
        
        interactables.ForEach(i => i.SetActive(false));
        
        yield return MoveHand(0.7f, selectHandPosition.position, startHandPosition);
        
        DestroyKeepsakes();
        shop.SetDelayOpen(false);
    }

    private bool SpawnKeepsakes(List<GameObject> keepsakes)
    {
        if (keepsakes == null || keepsakes.Count == 0) return false;
        if (keepsakePositions == null || keepsakePositions.Count == 0) return false;
        if (KeepsakeManager.instance.IsKeepsakeEquipFull) return false;

        int i = 0;
        
        foreach (var keepsakePrefab in keepsakes)
        {
            if (i >= keepsakePositions.Count) break;
            
            var keepsake = Instantiate(keepsakePrefab, keepsakePositions[i]);
            var interactable = keepsake.GetComponent<KeepsakeInteractable>();

            if (KeepsakeUnlockProgression.instance.HasMetRequirement(interactable.GetKeepsake()) &&
                !KeepsakeManager.instance.IsKeepsakeTypeEquipped(interactable.GetKeepsake()))
            {
                keepsake.transform.localPosition = Vector3.zero;
                keepsake.transform.localScale = interactable.scaleInHand;
                keepsake.transform.localRotation = Quaternion.Euler(interactable.rotationInHand);

                RemoveKeepsakeSpotlight(keepsake);
                
                interactable.SetActive(true);
                interactable.SetBlackjackGame(game);
                interactables.Add(interactable);

                i++;
            }
            else Destroy(keepsake);

        }

        return interactables.Count > 0;
    }

    private static void RemoveKeepsakeSpotlight(GameObject keepsake)
    {
        if (keepsake.transform.childCount <= 0) return;
        
        var light = keepsake.transform.GetChild(0);
        light?.gameObject.SetActive(false);
    }

    private IEnumerator MoveHand(float duration, Vector3 start, Vector3 end)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            dealerHand.transform.position = Vector3.Lerp(start, end, t);

            yield return null;
        }
    }

    private void Update()
    {
        if (!isChoosing || !Input.GetMouseButtonDown(0)) return;
        isChoosing = false;
    }

    private void DestroyKeepsakes()
    {
        foreach (var position in keepsakePositions)
        {
            var keepsake = position.transform.GetChild(0).gameObject;
            Destroy(keepsake);
        }
    }
}
