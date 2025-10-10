using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioClip clip;

    public string name;

    public float volume;
    public float pitch;

    [HideInInspector]
    public AudioSource source;
}
