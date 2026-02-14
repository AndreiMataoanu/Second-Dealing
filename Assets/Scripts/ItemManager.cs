using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private BlackjackGame blackjackGame;
    
    [Header("Spawn power ups")] 
    [SerializeField] private List<GameObject> buySpawnPoints;
    [SerializeField] private List<GameObject> useSpawnPoints;
    [SerializeField] private List<GameObject> powerUpPrefabs;

    public void SpawnPowerUps()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count == 0) return;

        foreach (var buySpawnPoint in buySpawnPoints.ToList())
        {
            var randomIndex = Random.Range(0, powerUpPrefabs.Count);
            var prefab = Instantiate(powerUpPrefabs[randomIndex], buySpawnPoint.transform);
            var item = prefab.GetComponent<Item>();
            item.SetBlackjackGame(blackjackGame);
            item.SetActive(true);
            item.AddAction(OnBuy);
        }
    }

    public void DespawnPowerUps()
    {
        foreach (var item in buySpawnPoints)
        {
            if (item.transform.childCount > 0)
                Destroy(item.transform.GetChild(0).gameObject);
        }
    }
    
    private void OnBuy(Item item)
    {
        Destroy(item.gameObject);
    }
}
