using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
public class ElevatorScript : MonoBehaviour
{

    [SerializeField] private GameObject rightDoor;
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject keepsakesFloor;
    [SerializeField] private GameObject casinoFloor;
    [SerializeField] private GameObject optionsFloor;
    [SerializeField] private CinemachineCamera elevatorCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin noise;
    [SerializeField] private CinemachineCamera blackjackCamera;
    [SerializeField] private float doorTime = 3f;
    [SerializeField] private float moveTime = 3f;

    [Header("Camera Shake Settings")]
    [SerializeField] private float idleAmplitude = 0.2f;
    [SerializeField] private float idleFrequency = 0.5f;
    [SerializeField] private float moveAmplitude = 1.5f;
    [SerializeField] private float moveFrequency = 1.5f;

    private string currentFloor;
    private bool doorsOpen = false;
    private bool isMoving = false;

    private void Start()
    {
        noise.AmplitudeGain = idleAmplitude;
        noise.FrequencyGain = idleFrequency;
    }

    public void BlackJackButton()
    {
        if(isMoving) return;

        StartCoroutine(StartGameCoroutine());
    }

    public void Options()
    {
        if(isMoving) return;

        StartCoroutine(OptionsCoroutine());
    }

    public void KeepSakesMenu()
    {
        if(isMoving) return;

        StartCoroutine(KeepsakesCoroutine());
    }

    public void QuitGame()
    {
        if(isMoving) return;

        Application.Quit();
    }

    private IEnumerator OpenDoors()
    {
        doorsOpen = true;

        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.localPosition.x + 2f, rightDoor.transform.localPosition.y, rightDoor.transform.localPosition.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.localPosition.x - 2f, leftDoor.transform.localPosition.y, leftDoor.transform.localPosition.z);

        float elapsedTime = 0;
        float waitTime = doorTime;

        Vector3 currentRightpos = rightDoor.transform.localPosition;
        Vector3 currentLeftpos = leftDoor.transform.localPosition;

        while(elapsedTime < waitTime)
        {
            leftDoor.transform.localPosition = Vector3.Lerp(currentLeftpos, leftDoorGoToPos, elapsedTime / waitTime);
            rightDoor.transform.localPosition = Vector3.Lerp(currentRightpos, rightDoorGoToPos, elapsedTime / waitTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        rightDoor.transform.localPosition = rightDoorGoToPos;
        leftDoor.transform.localPosition = leftDoorGoToPos;
        doorsOpen = true;

        yield return null;
    }

    private IEnumerator CloseDoors()
    {
        doorsOpen = false;

        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.localPosition.x - 2f, rightDoor.transform.localPosition.y, rightDoor.transform.localPosition.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.localPosition.x + 2f, leftDoor.transform.localPosition.y, leftDoor.transform.localPosition.z);

        float elapsedTime = 0;
        float waitTime = doorTime;

        Vector3 currentRightpos = rightDoor.transform.localPosition;
        Vector3 currentLeftpos = leftDoor.transform.localPosition;

        while(elapsedTime < waitTime)
        {
            leftDoor.transform.localPosition = Vector3.Lerp(currentLeftpos, leftDoorGoToPos, elapsedTime / waitTime);
            rightDoor.transform.localPosition = Vector3.Lerp(currentRightpos, rightDoorGoToPos, elapsedTime / waitTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        rightDoor.transform.localPosition = rightDoorGoToPos;
        leftDoor.transform.localPosition = leftDoorGoToPos;

        yield return null;
    }

    private IEnumerator StartGameCoroutine()
    {
        isMoving = true;

        if(currentFloor == casinoFloor.name)
        {
            isMoving = false;

            yield return null;
        }

        if(doorsOpen)
        {
            StartCoroutine(CloseDoors());

            yield return StartCoroutine(WaitDelay(doorTime));
        }

        StartCoroutine(MoveFloor(casinoFloor, 3f));

        yield return StartCoroutine(WaitDelay(3f));

        StartCoroutine(OpenDoors());

        yield return StartCoroutine(WaitDelay(doorTime));

        Vector3 startCamPos = elevatorCamera.transform.position;
        Vector3 endCamPos = blackjackCamera.transform.position;
        Quaternion startCamRot = elevatorCamera.transform.rotation;
        Quaternion endCamRot = blackjackCamera.transform.rotation;

        float elapsedTime = 0;
        float waitTime = moveTime;

        while(elapsedTime < waitTime)
        {
            //noise.AmplitudeGain = 1;
            elevatorCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, elapsedTime / waitTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elevatorCamera.Priority = 0;
        isMoving = false;

        yield return null;
    }

    private IEnumerator OptionsCoroutine()
    {
        isMoving = true;

        if(currentFloor == optionsFloor.name)
        {
            isMoving = false;

            yield break;
        }

        if(doorsOpen)
        {
            StartCoroutine(CloseDoors());

            yield return StartCoroutine(WaitDelay(doorTime));
        }

        StartCoroutine(MoveFloor(optionsFloor, 3f));

        yield return StartCoroutine(WaitDelay(3f));

        StartCoroutine(OpenDoors());

        yield return StartCoroutine(WaitDelay(doorTime));

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

        if(doorsOpen)
        {
            StartCoroutine(CloseDoors());

            yield return StartCoroutine(WaitDelay(doorTime));
        }

        StartCoroutine(MoveFloor(keepsakesFloor, 3f));

        yield return StartCoroutine(WaitDelay(3f));

        StartCoroutine(OpenDoors());

        yield return StartCoroutine(WaitDelay(doorTime));

        isMoving = false;
    }

    private IEnumerator MoveFloor(GameObject nextFloor, float waitTime)
    {
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

        StartCoroutine(ShakeCamera(idleAmplitude, idleFrequency, 0.5f));

        yield return null;
    }

    private IEnumerator WaitDelay(float duration)
    {
        float timer = 0f;

        yield return new WaitForSeconds(0.1f);

        timer += 0.1f;

        while(timer < duration)
        {
            timer += Time.deltaTime;

            yield return null;
        }
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
}