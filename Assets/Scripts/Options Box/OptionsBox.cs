using TMPro;
using UnityEngine;
using Utils;

//TODO: Uncomment FMOD stuff when we have it.
public class OptionsBox : MonoBehaviour
{
    [SerializeField] private Transform masterKnob;
    [SerializeField] private Transform musicKnob;
    [SerializeField] private Transform sfxKnob;
    [SerializeField] private Transform speedKnob;
    [SerializeField] private GameObject screenSwitchOn;
    [SerializeField] private GameObject screenSwitchOff;
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI screenText;
    private Quaternion masterStartRot;
    private Quaternion sfxStartRot;
    private Quaternion musicStartRot;
    private Quaternion speedStartRot;
    //Bus masterBus;
    //Bus sfxBus;
    //Bus musicBus;
    private float[] speedOptions = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f, 2.25f, 2.5f, 2.75f, 3.0f };
    private float master;
    private float music;
    private float sfx;
    private int speedIndex;
    private bool isWindowed;

    private void Awake()
    {
        //masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
        //sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
        //musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
    }

    private void Start()
    {
        masterStartRot = masterKnob.localRotation;
        sfxStartRot = sfxKnob.localRotation;
        musicStartRot = musicKnob.localRotation;
        speedStartRot = speedKnob.localRotation;
        master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        speedIndex = PlayerPrefs.GetInt("GameSpeedIndex", 2);
        isWindowed = PlayerPrefs.GetInt("ScreenModeIndex", 0) == 2;

        UpdateAllVisuals();
    }

    public void IncreaseMasterVolume() => AdjustVolume(ref master, 0.1f, masterKnob, masterStartRot, masterText, true);
    public void DecreaseMasterVolume() => AdjustVolume(ref master, -0.1f, masterKnob, masterStartRot, masterText, false);

    public void IncreaseSFXVolume() => AdjustVolume(ref sfx, 0.1f, sfxKnob, sfxStartRot, sfxText, true);
    public void DecreaseSFXVolume() => AdjustVolume(ref sfx, -0.1f, sfxKnob, sfxStartRot, sfxText, false);

    public void IncreaseMusicVolume() => AdjustVolume(ref music, 0.1f, musicKnob, musicStartRot, musicText, true);
    public void DecreaseMusicVolume() => AdjustVolume(ref music, -0.1f, musicKnob, musicStartRot, musicText, false);

    //Add new FMOD.Studio.Bus bus paramater
    private void AdjustVolume(ref float volumeRef, float delta, Transform knobTransform, Quaternion startRot, TextMeshProUGUI label, bool playUpSound)
    {
        volumeRef = Mathf.Clamp01(volumeRef + delta);
        //bus.setVolume(volumeRef);

        UpdateKnobRotation(knobTransform, volumeRef, startRot);
        UpdateVolumeText(label, volumeRef);

        AudioManager.instance.Play(playUpSound ? "BetUp" : "BetDown");
    }

    public void IncreaseGameSpeed()
    {
        speedIndex++;

        if(speedIndex >= speedOptions.Length)
        {
            speedIndex = speedOptions.Length - 1;
        }

        UpdateSpeedVisuals();

        AudioManager.instance.Play("BetUp");
    }

    public void DecreaseGameSpeed()
    {
        speedIndex--;

        if(speedIndex < 0)
        {
            speedIndex = 0;
        }

        UpdateSpeedVisuals();

        AudioManager.instance.Play("BetDown");
    }

    public void ToggleScreenMode()
    {
        isWindowed = !isWindowed;

        UpdateSwitchVisuals();

        AudioManager.instance.Play(isWindowed ? "BetUp" : "BetDown");
    }

    public void SavePreferences()
    {
        AudioManager.instance.Play("BetUp");
        PlayerPrefs.SetFloat("MasterVolume", master);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.SetFloat("MusicVolume", music);
        PlayerPrefs.SetInt("GameSpeedIndex", speedIndex);
        GameUtils.gameSpeedMultiplier = speedOptions[speedIndex];

        int mode = isWindowed ? 2 : 1;

        PlayerPrefs.SetInt("ScreenModeIndex", mode);
        Screen.fullScreenMode = isWindowed ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
        PlayerPrefs.Save();
    }

    private void UpdateAllVisuals()
    {
        UpdateKnobRotation(masterKnob, master, masterStartRot);
        UpdateKnobRotation(sfxKnob, sfx, sfxStartRot);
        UpdateKnobRotation(musicKnob, music, musicStartRot);
        UpdateVolumeText(masterText, master);
        UpdateVolumeText(sfxText, sfx);
        UpdateVolumeText(musicText, music);
        UpdateSpeedVisuals();
        UpdateSwitchVisuals();
    }

    private void UpdateKnobRotation(Transform knob, float value, Quaternion startRotation)
    {
        knob.localRotation = startRotation * Quaternion.Euler(0, 0, -value * 360f);
    }

    private void UpdateVolumeText(TextMeshProUGUI label, float value)
    {
        label.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    private void UpdateSpeedVisuals()
    {
        float percent = (float)speedIndex / (speedOptions.Length - 1);

        speedKnob.localRotation = speedStartRot * Quaternion.Euler(0, 0, -percent * 360f);
        speedText.text = speedOptions[speedIndex].ToString("0.00") + "x";
    }

    private void UpdateSwitchVisuals()
    {
        screenSwitchOn.SetActive(!isWindowed);
        screenSwitchOff.SetActive(isWindowed);
        screenText.text = isWindowed ? "Windowed" : "Borderless";
    }
}