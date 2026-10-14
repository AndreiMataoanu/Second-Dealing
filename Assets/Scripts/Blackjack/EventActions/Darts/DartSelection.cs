using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartSelection : MonoBehaviour
{
    [Header("Position on cards")]
    [Tooltip("All children darts have to be after start throw transform")]
    [SerializeField] private Transform startThrow;
    [SerializeField] private Vector3 positionRange = new(0.0101f, 0.0f, 0.0055f);
    [SerializeField] private float rotationRange = 20f;
    [SerializeField] private float throwTime = 0.3f;
    
    private List<GameObject> darts = new();
    private List<Vector3> originalPositions = new();
    private List<Quaternion> originalRotations = new();
    
    private int dartCount;

    public Vector3 GetDartPositionAtIndex(int index) => darts[index].transform.position;

    #region Setup

    private void Start()
    {
        dartCount = transform.childCount - 1;

        for (int i = 1; i < transform.childCount; i++)
        {
            var dart = transform.GetChild(i).gameObject;
            darts.Add(dart);
            originalPositions.Add(dart.transform.position);
            originalRotations.Add(dart.transform.rotation);
        }
    }
    
    public void ResetDarts()
    {
        for (int i = 0; i < dartCount; i++)
        {
            darts[i].transform.position = originalPositions[i];
            darts[i].transform.rotation = originalRotations[i];
            darts[i].SetActive(false);
        }
    }

    #endregion

    #region Set darts active
    
    public void SetDartSelectionActive(bool isActive) => gameObject.SetActive(isActive);

    public void SetActiveDartCount(int dartNumber)
    {
        var count = Mathf.Min(dartCount, dartNumber);

        for (int i = 0; i < count; i++)
            darts[i].SetActive(true);
        
        for (int i = count; i < dartCount; i++)
            darts[i].SetActive(false);
    }

    public void DeactivateDartAtIndex(int index)
    {
        if (index >= dartCount) return;
        darts[index].SetActive(false);
    }

    #endregion

    #region Throw darts

    public void ThrowDart(int index, Vector3 to)
    {
        var dart = darts[index];
        dart.SetActive(true);
        dart.transform.position = startThrow.position;
        dart.transform.rotation = startThrow.rotation;
        dart.transform.LookAt(to);
        
        StartCoroutine(ThrowMovementCoroutine(dart,
            dart.transform.position, GetRandomPosition(to),
            dart.transform.rotation, GetRandomRotation()));
    }

    private IEnumerator ThrowMovementCoroutine(GameObject dart, 
        Vector3 startPosition, Vector3 endPosition,
        Quaternion startRotation, Quaternion endRotation)
    {
        var elapsed = 0.0f;

        while (elapsed < throwTime)
        {
            elapsed += Time.deltaTime;
            dart.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / throwTime);
            dart.transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / throwTime);
            yield return null;
        }
        
        dart.transform.position = endPosition;
        dart.transform.rotation = endRotation;
    }

    #endregion

    #region Randomize placement

    private Vector3 GetRandomPosition(Vector3 pos)
    {
        var xPos = Random.Range(pos.x - positionRange.x, pos.x + positionRange.x);
        var yPos = pos.y + 0.08f;
        var zPos = Random.Range(pos.z - positionRange.z, pos.z + positionRange.z);
        
        return new Vector3(xPos, yPos, zPos);
    }

    private Quaternion GetRandomRotation()
    {
        var start = Quaternion.Euler(90, 0, 0);
        var leftRight = Random.Range(-rotationRange, rotationRange);
        var frontBack = Random.Range(-rotationRange, rotationRange);
        var rotX = Quaternion.AngleAxis(frontBack, Vector3.right);
        var rotZ = Quaternion.AngleAxis(leftRight, Vector3.forward);
        
        return start * rotX * rotZ;
    }

    #endregion
}
