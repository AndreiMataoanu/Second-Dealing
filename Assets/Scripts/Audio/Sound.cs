using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioClip[] clips;

    public string name;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0f, 1f)] public float pitchVariance = 0.1f;

    [HideInInspector] public AudioSource source;
}
