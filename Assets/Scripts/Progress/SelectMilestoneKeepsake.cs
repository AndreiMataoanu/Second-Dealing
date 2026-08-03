using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMilestoneKeepsake : MonoBehaviour
{
    [SerializeField] private List<Transform> keepsakePositions;
    [SerializeField] private GameObject dealerHand;
    [SerializeField] private Transform selectHandPosition;
    
    [HideInInspector] public bool isChoosing; 
    
    private ShopManager shop;
    private Vector3 startHandPosition;

    private void Start()
    {
        dealerHand.SetActive(false);
        startHandPosition = dealerHand.transform.position;
    }

    public void SetShopManager(ShopManager shopManager) => shop = shopManager;
    
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

        yield return MoveHand(1.3f, startHandPosition, selectHandPosition.position);

        yield return new WaitUntil(() => isChoosing == false);

        yield return MoveHand(0.7f, selectHandPosition.position, startHandPosition);
        
        DestroyKeepsakes();
    }

    private bool SpawnKeepsakes(List<GameObject> keepsakes)
    {
        if (keepsakes == null || keepsakes.Count == 0) return false;
        if (keepsakePositions == null || keepsakePositions.Count == 0) return false;

        isChoosing = true;
        shop.SetDelayOpen(true);
        
        for (int i = 0; i < keepsakePositions.Count; i++)
            if (i < keepsakes.Count)
                Instantiate(keepsakes[i], keepsakePositions[i]);

        return true;
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
        if (isChoosing && Input.GetMouseButtonDown(0))
        {
            isChoosing = false;
        }
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
