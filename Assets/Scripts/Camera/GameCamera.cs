using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public enum CameraType
{
    Sitting,
    Playing,
    Event
}

public class GameCamera : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private float cameraTransitionTime;
    [SerializeField] private CinemachineBasicMultiChannelPerlin noise;
    
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera sittingCamera;
    [SerializeField] private CinemachineCamera playingCamera;
    [SerializeField] private CinemachineCamera eventCamera;

    [Header("VFX")]
    [SerializeField] public GameObject distortion;

    private Coroutine swayCoroutine;
    
    private void Start()
    {
        cinemachineBrain.DefaultBlend.Time = cameraTransitionTime;
    }

    public void ResetNoise()
    {
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }
    
    public void ChangeToCamera(CameraType cameraType)
    {
        sittingCamera.Priority = 0;
        eventCamera.Priority = 0;
        playingCamera.Priority = 0;
        
        switch (cameraType)
        {
            case CameraType.Sitting:
                sittingCamera.Priority = 10;
                break;
            case CameraType.Playing:
                playingCamera.Priority = 10;
                break;
            case CameraType.Event:
                eventCamera.Priority = 10;
                break;
        }
    }

    public Coroutine TiltPlayerCameraUpDown(Quaternion tiltDegree, float duration=1f)
    {
        return StartCoroutine(TiltPlayerCameraUpDownCoroutine(tiltDegree, duration));
    }
    
    
    private IEnumerator TiltPlayerCameraUpDownCoroutine(Quaternion tiltDegree, float duration)
    {
        Quaternion startRot = playingCamera.transform.rotation;
        Quaternion targetRot = startRot * tiltDegree;
    
        float halfDuration = duration / 2f;

        yield return RotatePlayerCamera(halfDuration, startRot, targetRot);
        yield return RotatePlayerCamera(halfDuration, targetRot, startRot);
    }

    private IEnumerator RotatePlayerCamera(float duration, Quaternion startRot, Quaternion targetRot)
    {
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
    
            float tLerp = elapsedTime / duration;
            float smoothT = tLerp * tLerp * (3f - 2f * tLerp);
    
            playingCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
    
            yield return null;
        }
    }

    private void StartCameraSway(float minAmp, float maxAmp, float minFreq, float maxFreq, float speed)
    {
        swayCoroutine = StartCoroutine(AlcoholCameraSwayCoroutine(minAmp, maxAmp, minFreq, maxFreq, speed));
    }

    private void StopCameraSway()
    {
        ResetNoise();
        
        if (swayCoroutine != null)
        {
            StopCoroutine(swayCoroutine);
            swayCoroutine = null;
        }
    }
    
    private IEnumerator AlcoholCameraSwayCoroutine(float minAmp, float maxAmp, float minFreq, float maxFreq, float speed)
    {
        var elapsedTime = 0f;

        while(true)
        {
            elapsedTime += Time.deltaTime * speed;
            float lerpValue = Mathf.PingPong(elapsedTime, 1f);

            lerpValue = lerpValue * lerpValue * (3f - 2f * lerpValue);

            noise.AmplitudeGain = Mathf.Lerp(minAmp, maxAmp, lerpValue);
            noise.FrequencyGain = Mathf.Lerp(minFreq, maxFreq, lerpValue);

            yield return null;
        }
    }
    
        
    public void UseDistortedVision()
    {
        AudioManager.instance.isMuffled = true;
        distortion.SetActive(true);
        StartCameraSway(0f, 0.2f, 0f, 0.1f, 1f);
    }

    public void UseClearVision()
    {
        AudioManager.instance.isMuffled = false;
        distortion.SetActive(false);
        StopCameraSway();
    }
}
