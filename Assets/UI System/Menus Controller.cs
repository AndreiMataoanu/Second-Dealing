using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenusController : MonoBehaviour
{
    [SerializeField] private GameObject darkImage;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainMenuPanel;

    private bool isPaused;

    private void Awake()
    {
        if(darkImage == null || pausePanel == null || mainMenuPanel == null) return;
    }

    private void Update()
    {
        if(pausePanel != null)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    private void OnEnable()
    {
        if(darkImage != null)
        {
            darkImage.SetActive(false);
        }

        if(pausePanel != null)
        {
            ResumeGame();
        }
    }

    #region Pause Menu
    public void PauseGame()
    {
        pausePanel.SetActive(true);
        darkImage.SetActive(true);
        isPaused = true;

        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        darkImage.SetActive(false);
        isPaused = false;

        Time.timeScale = 1;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
    }
    #endregion

    #region Common Methods
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        ResumeGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Main Menu
    public void StartGame()
    {
        //Update this to the actual scene index of main map.
        SceneManager.LoadSceneAsync(1);
    }
    #endregion
}
