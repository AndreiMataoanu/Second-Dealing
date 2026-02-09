using UnityEngine;

public class NewCameraController : MonoBehaviour
{
    private static NewCameraController _instance;

    [SerializeField] private GameObject blackjackManager;

    private BlackjackGame _blackjackGame;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);

            return;
        }
        
        _blackjackGame = blackjackManager.GetComponent<BlackjackGame>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
