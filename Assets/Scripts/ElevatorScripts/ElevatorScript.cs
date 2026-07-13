using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
public class ElevatorScript : MonoBehaviour
{

    public GameObject rightDoor;
    public GameObject leftDoor;
    public GameObject keepsakesFloor;
    public GameObject casinoFloor;
    public GameObject optionsFloor;
    public CinemachineCamera elevatorCamera;
    public CinemachineBasicMultiChannelPerlin noise;
    public CinemachineCamera blackjackCamera;

    String currentFloor;
    bool doorsOpen = false;
    public float doorTime = 3f;
    public float moveTime = 3f;
    public void BlackJackButton()
    {
        StartCoroutine(StartGameCoroutine());
    }

    public void Options()
    {
        StartCoroutine(OptionsCoroutine());

    }
    public void KeepSakesMenu()
    {
        StartCoroutine(KeepsakesCoroutine());

    }
    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator OpenDoors()
    {
        doorsOpen = true;
        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.localPosition.x + 2f, rightDoor.transform.localPosition.y, rightDoor.transform.localPosition.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.localPosition.x - 2f, leftDoor.transform.localPosition.y, leftDoor.transform.localPosition.z);
        float elapsedTime = 0;
        float waitTime = doorTime;
        Vector3 currentRightpos = rightDoor.transform.localPosition;
        Vector3 currentLeftpos = leftDoor.transform.localPosition;
        while (elapsedTime < waitTime)
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
    IEnumerator CloseDoors()
    {
        doorsOpen = false;
        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.localPosition.x - 2f, rightDoor.transform.localPosition.y, rightDoor.transform.localPosition.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.localPosition.x + 2f, leftDoor.transform.localPosition.y, leftDoor.transform.localPosition.z);
        float elapsedTime = 0;
        float waitTime = doorTime;
        Vector3 currentRightpos = rightDoor.transform.localPosition;
        Vector3 currentLeftpos = leftDoor.transform.localPosition;
        while (elapsedTime < waitTime)
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
    IEnumerator StartGameCoroutine()
    {

        if (currentFloor == casinoFloor.name)
        {
            yield return null;
        }
        if (doorsOpen)
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
        while (elapsedTime < waitTime)
        {
            noise.AmplitudeGain = 1;
            elevatorCamera.transform.position = Vector3.Lerp(startCamPos, endCamPos, elapsedTime / waitTime);
            //elevatorCamera.transform.rotation = Quaternion.Lerp(startCamRot, endCamRot, elapsedTime / 7f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elevatorCamera.Priority = 0;
        yield return null;

    }
    IEnumerator OptionsCoroutine()
    {
        if (currentFloor == optionsFloor.name)
        {
            yield break;
        }
        if (doorsOpen)
        {
            StartCoroutine(CloseDoors());
            yield return StartCoroutine(WaitDelay(doorTime));
        }
        StartCoroutine(MoveFloor(optionsFloor, 3f));
        yield return StartCoroutine(WaitDelay(3f));
        StartCoroutine(OpenDoors());
        yield return StartCoroutine(WaitDelay(doorTime));

    }
    IEnumerator KeepsakesCoroutine()
    {
        if (currentFloor == optionsFloor.name)
        {
            yield break;
        }
        if (doorsOpen)
        {
            StartCoroutine(CloseDoors());
            yield return StartCoroutine(WaitDelay(doorTime));
        }
        StartCoroutine(MoveFloor(keepsakesFloor, 3f));
        yield return StartCoroutine(WaitDelay(3f));
        StartCoroutine(OpenDoors());
        yield return StartCoroutine(WaitDelay(doorTime));

    }


    IEnumerator MoveFloor(GameObject nextFloor, float waitTime)
    {
        Vector3 nextFloorPos = new Vector3(transform.position.x, nextFloor.transform.position.y, transform.position.z);
        float elapsedTime = 0;
        Vector3 currentPos = transform.position;
        while (elapsedTime < waitTime)
        {
            transform.position = Vector3.Lerp(currentPos, nextFloorPos, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        currentFloor = nextFloor.name;
        transform.position = nextFloorPos;
        yield return null;
    }
    private IEnumerator WaitDelay(float duration)
    {
        float timer = 0f;

        yield return new WaitForSeconds(0.1f);

        timer += 0.1f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            yield return null;
        }
    }

}
