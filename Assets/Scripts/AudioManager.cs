using System;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);

            return;
        }

        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            if(s.clips.Length > 0)
            {
                s.source.clip = s.clips[0];
            }

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if(s == null || s.clips.Count() == 0) return;

        AudioClip randomClip = s.clips[UnityEngine.Random.Range(0, s.clips.Count())];

        s.source.clip = randomClip;
        s.source.volume = s.volume;

        float randomPitch = UnityEngine.Random.Range(s.pitch - s.pitchVariance, s.pitch + s.pitchVariance);

        s.source.pitch = randomPitch;
        s.source.Play();
    }
}
