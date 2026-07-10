using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenusController : MonoBehaviour
{
    [Header("Set-up")]
    [SerializeField] private GameObject darkImage;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Options")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI screenModeText;
    private float[] speedOptions = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f, 2.25f, 2.5f, 2.75f, 3.0f };
    private FullScreenMode[] screenModes = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
    private string[] screenModeNames = { "Fullscreen", "Borderless", "Windowed" };
    private int currentSpeedIndex = 1;
    private int currentScreenModeIndex = 0;

    //FMOD. Commented lines are for FMOD integration, which is not currently used, but works as intended.
    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile normalVolume;
    [SerializeField] private VolumeProfile menuVolume;
    //Bus masterBus;
    //Bus sfxBus;
    //Bus musicBus;

    private bool isPaused;

    private void Awake()
    {
        //masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
        //sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");

        if(darkImage == null || pausePanel == null || mainMenuPanel == null) return;
    }

    private void Start()
    {
        LoadSettings();
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

        if(optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    #region Pause Menu
    public void PauseGame()
    {
        pausePanel.SetActive(true);
        darkImage.SetActive(true);
        //volume.profile = menuVolume;
        isPaused = true;

        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        darkImage.SetActive(false);
        optionsPanel.SetActive(false);
        //volume.profile = normalVolume;
        isPaused = false;

        Time.timeScale = 1;
    }

    public void ReturnToMainMenu()
    {
        //masterBus.stopAllEvents(STOP_MODE.IMMEDIATE);

        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
        //SceneManager.LoadSceneAsync("Main Menu");
    }
    #endregion

    #region Common Methods
    public void RestartGame()
    {
        //masterBus.stopAllEvents(STOP_MODE.IMMEDIATE);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

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

    #region Options
    public void ShowOptions()
    {
        optionsPanel.SetActive(true);

        if(pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        else if(mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    public void HideOptions()
    {
        optionsPanel.SetActive(false);

        if(pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        else if(mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    private void LoadSettings()
    {
        //masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        //sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        //musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        currentSpeedIndex = PlayerPrefs.GetInt("GameSpeedIndex", 2);
        currentScreenModeIndex = PlayerPrefs.GetInt("ScreenModeIndex", 0);

        //SetMasterVolume(masterSlider.value);
        //SetSFXVolume(sfxSlider.value);
        //SetMusicVolume(musicSlider.value);

        //masterSlider.onValueChanged.AddListener(SetMasterVolume);
        //sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        //musicSlider.onValueChanged.AddListener(SetMusicVolume);

        ApplySpeed();
        ApplyScreenMode();
    }

    public void SetMasterVolume(float volume)
    {
        //masterBus.setVolume(volume);

        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        //sfxBus.setVolume(volume);

        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        //musicBus.setVolume(volume);

        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void IncreaseSpeed()
    {
        currentSpeedIndex++;

        if(currentSpeedIndex >= speedOptions.Length)
        {
            currentSpeedIndex = speedOptions.Length - 1;
        }

        ApplySpeed();
    }

    public void DecreaseSpeed()
    {
        currentSpeedIndex--;

        if(currentSpeedIndex < 0)
        {
            currentSpeedIndex = 0;
        }

        ApplySpeed();
    }

    private void ApplySpeed()
    {
        BlackjackGame.gameSpeedMultiplier = speedOptions[currentSpeedIndex];
        PlayerPrefs.SetInt("GameSpeedIndex", currentSpeedIndex);
        PlayerPrefs.Save();

        UpdateSpeedUI();
    }

    private void UpdateSpeedUI()
    {
        if(speedText != null)
        {
            speedText.text = speedOptions[currentSpeedIndex].ToString("0.00") + "x";
        }
    }

    public void IncreaseScreenMode()
    {
        currentScreenModeIndex++;

        if(currentScreenModeIndex >= screenModes.Length)
        {
            currentScreenModeIndex = screenModes.Length - 1;
        }

        ApplyScreenMode();
    }

    public void DecreaseScreenMode()
    {
        currentScreenModeIndex--;

        if(currentScreenModeIndex < 0)
        {
            currentScreenModeIndex = 0;
        }

        ApplyScreenMode();
    }

    private void ApplyScreenMode()
    {
        Screen.fullScreenMode = screenModes[currentScreenModeIndex];

        PlayerPrefs.SetInt("ScreenModeIndex", currentScreenModeIndex);
        PlayerPrefs.Save();

        UpdateScreenModeUI();
    }

    private void UpdateScreenModeUI()
    {
        if(screenModeText != null)
        {
            screenModeText.text = screenModeNames[currentScreenModeIndex];
        }
    }
    #endregion
}
