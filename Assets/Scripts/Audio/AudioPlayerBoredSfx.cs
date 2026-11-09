using UnityEngine;

public class AudioPlayerBoredSfx : MonoBehaviour
{
    public void SfxPlayerBored() => AudioManagerFMOD.Play(AudioEvent.sfxPlayerBored);
}
