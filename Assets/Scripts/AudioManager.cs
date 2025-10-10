using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sound;
    
    void Awake()
    {
        foreach (Sound s in sound)
        {
           s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sound, sound => sound.name == name);
        if (s != null) 
        {
            s.source.Play();
        }
    }
    
}
