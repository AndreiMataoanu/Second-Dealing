using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ElevatorScript : MonoBehaviour
{

    public GameObject rightDoor;
    public GameObject leftDoor;
    public GameObject keepsakesFloor;
    public GameObject casinoFloor;
    public GameObject optionsFloor;

    String currentFloor;
    bool doorsOpen = false;
    public float doorTime = 3f;
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
        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.position.x + 2f, rightDoor.transform.position.y, rightDoor.transform.position.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.position.x - 2f, leftDoor.transform.position.y, leftDoor.transform.position.z);
        float elapsedTime = 0;
        float waitTime = doorTime;
        Vector3 currentRightpos = rightDoor.transform.position;
        Vector3 currentLeftpos = leftDoor.transform.position;
        while (elapsedTime < waitTime)
        {
            Debug.Log("timeisgoing");
            leftDoor.transform.position = Vector3.Lerp(currentLeftpos, leftDoorGoToPos, elapsedTime / waitTime);
            rightDoor.transform.position = Vector3.Lerp(currentRightpos, rightDoorGoToPos, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        rightDoor.transform.position = rightDoorGoToPos;
        leftDoor.transform.position = leftDoorGoToPos;
        doorsOpen = true;
        yield return null;
    }
    IEnumerator CloseDoors()
    {
        doorsOpen = false;
        Vector3 rightDoorGoToPos = new Vector3(rightDoor.transform.position.x - 2f, rightDoor.transform.position.y, rightDoor.transform.position.z);
        Vector3 leftDoorGoToPos = new Vector3(leftDoor.transform.position.x + 2f, leftDoor.transform.position.y, leftDoor.transform.position.z);
        float elapsedTime = 0;
        float waitTime = doorTime;
        Vector3 currentRightpos = rightDoor.transform.position;
        Vector3 currentLeftpos = leftDoor.transform.position;
        while (elapsedTime < waitTime)
        {
            leftDoor.transform.position = Vector3.Lerp(currentLeftpos, leftDoorGoToPos, elapsedTime / waitTime);
            rightDoor.transform.position = Vector3.Lerp(currentRightpos, rightDoorGoToPos, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        rightDoor.transform.position = rightDoorGoToPos;
        leftDoor.transform.position = leftDoorGoToPos;
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

        SceneManager.LoadScene(1);


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
