using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private GameObject rightDoor;
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject keepsakesFloor;
    [SerializeField] private GameObject blackjackFloor;
    [SerializeField] private GameObject optionsFloor;
    [SerializeField] private GameObject hands;
    [SerializeField] private CinemachineCamera elevatorCamera;
    [SerializeField] private CinemachineCamera keepsakeCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin noise;
    [SerializeField] private CinemachineCamera blackjackCamera;
    [SerializeField] private float doorTime = 3f;
    [SerializeField] private float moveTime = 3f;

    [Header("Camera Shake Settings")]
    [SerializeField] private float idleAmplitude = 0.2f;
    [SerializeField] private float idleFrequency = 0.5f;
    [SerializeField] private float moveAmplitude = 4.0f;
    [SerializeField] private float moveFrequency = 5.0f;
    [SerializeField] private float walkAmplitude = 1.5f;
    [SerializeField] private float walkFrequency = 2.0f;

    [Header("Door Shake")]
    [SerializeField] private float doorAmplitude = 6.0f;
    [SerializeField] private float doorFrequency = 8.0f;
    [SerializeField] private float doorDuration = 0.3f;

    private string currentFloor;
    private bool doorsOpen = false;
    private bool isMoving = false;

    private void Start()
    {
        noise.AmplitudeGain = idleAmplitude;
        noise.FrequencyGain = idleFrequency;

        AudioManager.instance.Play("ElevatorAmbience");
    }

    public void BlackjackButton()
    {
        if(isMoving) return;

        StartCoroutine(BlackjackCoroutine());
    }

    public void OptionsButton()
    {
        if(isMoving) return;

        StartCoroutine(OptionsCoroutine());
    }

    public void KeepsakesButton()
    {
        if(isMoving) return;

        StartCoroutine(KeepsakesCoroutine());
    }

    public void QuitButton()
    {
        if(isMoving) return;

        Application.Quit();
    }

    private IEnumerator OpenDoors()
    {
        doorsOpen = true;

        AudioManager.instance.Play("DoorOpen");

        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.localPosition.x + 2f, rightDoor.transform.localPosition.y, rightDoor.transform.localPosition.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.localPosition.x - 2f, leftDoor.transform.localPosition.y, leftDoor.transform.localPosition.z);

        float elapsedTime = 0;

        Vector3 currentRightpos = rightDoor.transform.localPosition;
        Vector3 currentLeftpos = leftDoor.transform.localPosition;

        float startAmp = noise.AmplitudeGain;
        float startFreq = noise.FrequencyGain;

        while(elapsedTime < doorTime)
        {
            leftDoor.transform.localPosition = Vector3.Lerp(currentLeftpos, leftDoorGoToPos, elapsedTime / doorTime);
            rightDoor.transform.localPosition = Vector3.Lerp(currentRightpos, rightDoorGoToPos, elapsedTime / doorTime);
            noise.AmplitudeGain = Mathf.Lerp(startAmp, doorAmplitude, elapsedTime / doorTime);
            noise.FrequencyGain = Mathf.Lerp(startFreq, doorFrequency, elapsedTime / doorTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        rightDoor.transform.localPosition = rightDoorGoToPos;
        leftDoor.transform.localPosition = leftDoorGoToPos;
        noise.AmplitudeGain = doorAmplitude;
        noise.FrequencyGain = doorFrequency;

        AudioManager.instance.Play("DoorSlam");

        StartCoroutine(ShakeCamera(idleAmplitude, idleFrequency, doorDuration));
    }

    private IEnumerator CloseDoors()
    {
        doorsOpen = false;

        AudioManager.instance.Play("DoorClose");

        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.localPosition.x - 2f, rightDoor.transform.localPosition.y, rightDoor.transform.localPosition.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.localPosition.x + 2f, leftDoor.transform.localPosition.y, leftDoor.transform.localPosition.z);

        float elapsedTime = 0;

        Vector3 currentRightpos = rightDoor.transform.localPosition;
        Vector3 currentLeftpos = leftDoor.transform.localPosition;

        float startAmp = noise.AmplitudeGain;
        float startFreq = noise.FrequencyGain;

        while(elapsedTime < doorTime)
        {
            leftDoor.transform.localPosition = Vector3.Lerp(currentLeftpos, leftDoorGoToPos, elapsedTime / doorTime);
            rightDoor.transform.localPosition = Vector3.Lerp(currentRightpos, rightDoorGoToPos, elapsedTime / doorTime);
            noise.AmplitudeGain = Mathf.Lerp(startAmp, doorAmplitude, elapsedTime / doorTime);
            noise.FrequencyGain = Mathf.Lerp(startFreq, doorFrequency, elapsedTime / doorTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        rightDoor.transform.localPosition = rightDoorGoToPos;
        leftDoor.transform.localPosition = leftDoorGoToPos;
        noise.AmplitudeGain = doorAmplitude;
        noise.FrequencyGain = doorFrequency;

        AudioManager.instance.Play("DoorSlam");

        StartCoroutine(ShakeCamera(idleAmplitude, idleFrequency, doorDuration));
    }

    private IEnumerator BlackjackCoroutine()
    {
        isMoving = true;

        if(currentFloor == blackjackFloor.name)
        {
            isMoving = false;

            yield break;
        }

        ResetCameras();

        if(doorsOpen)
        {
            yield return StartCoroutine(CloseDoors());
        }

        yield return StartCoroutine(MoveFloor(blackjackFloor, moveTime));
        yield return StartCoroutine(OpenDoors());

        Vector3 startCamPos = elevatorCamera.transform.position;
        Vector3 endCamPos = blackjackCamera.transform.position;

        float elapsedTime = 0;

        while(elapsedTime < 3f)
        {
            Vector3 basePos = Vector3.Lerp(startCamPos, endCamPos, elapsedTime / 3f);

            float bobOffset = Mathf.Sin(elapsedTime * walkFrequency * Mathf.PI) * walkAmplitude;

            elevatorCamera.transform.position = basePos + new Vector3(0f, bobOffset, 0f);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        blackjackCamera.Priority = 10;
        elevatorCamera.Priority = 0;
        keepsakeCamera.Priority = 0;
        isMoving = false;
        hands.SetActive(true);

        AudioManager.instance.Stop("ElevatorAmbience");
        AudioManager.instance.Play("MainTheme");
    }

    private IEnumerator OptionsCoroutine()
    {
        isMoving = true;

        if(currentFloor == optionsFloor.name)
        {
            isMoving = false;

            yield break;
        }

        ResetCameras();

        if(doorsOpen)
        {
            yield return StartCoroutine(CloseDoors());
        }

        yield return StartCoroutine(MoveFloor(optionsFloor, moveTime));
        yield return StartCoroutine(OpenDoors());

        isMoving = false;
    }

    private IEnumerator KeepsakesCoroutine()
    {
        isMoving = true;

        if(currentFloor == keepsakesFloor.name)
        {
            isMoving = false;

            yield break;
        }

        ResetCameras();

        if(doorsOpen)
        {
            yield return StartCoroutine(CloseDoors());
        }

        yield return StartCoroutine(MoveFloor(keepsakesFloor, moveTime));
        yield return StartCoroutine(OpenDoors());
        yield return new WaitForSeconds(1f);

        isMoving = false;

        elevatorCamera.Priority = 0;
        keepsakeCamera.Priority = 10;
    }

    private IEnumerator MoveFloor(GameObject nextFloor, float waitTime)
    {
        elevatorCamera.Priority = 10;

        AudioManager.instance.Play("ElevatorTravel");
        AudioManager.instance.Play("ElevatorMusic");

        StartCoroutine(ShakeCamera(moveAmplitude, moveFrequency, 0.5f));

        Vector3 nextFloorPos = new Vector3(transform.position.x, nextFloor.transform.position.y, transform.position.z);

        float elapsedTime = 0;

        Vector3 currentPos = transform.position;

        while(elapsedTime < waitTime)
        {
            transform.position = Vector3.Lerp(currentPos, nextFloorPos, elapsedTime / waitTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        currentFloor = nextFloor.name;
        transform.position = nextFloorPos;

        AudioManager.instance.Play("ElevatorDing");

        StartCoroutine(ShakeCamera(idleAmplitude, idleFrequency, 0.5f));
    }

    private IEnumerator ShakeCamera(float targetAmplitude, float targetFrequency, float duration)
    {
        float startAmplitude = noise.AmplitudeGain;
        float startFrequency = noise.FrequencyGain;
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            noise.AmplitudeGain = Mathf.Lerp(startAmplitude, targetAmplitude, elapsedTime / duration);
            noise.FrequencyGain = Mathf.Lerp(startFrequency, targetFrequency, elapsedTime / duration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        noise.AmplitudeGain = targetAmplitude;
        noise.FrequencyGain = targetFrequency;
    }

    private void ResetCameras()
    {
        keepsakeCamera.Priority = 0;
        elevatorCamera.Priority = 10;
    }
}