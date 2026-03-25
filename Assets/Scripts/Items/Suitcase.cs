using UnityEngine;

namespace Items
{
    public class Suitcase : MonoBehaviour
    {
        [SerializeField] private ItemManager itemManager;

        public void OnOpenSuitcase()
        {
            itemManager.SpawnPowerUps();
        }
    }
}
