using System.Collections.Generic;
using UnityEngine;

public class DartSelection : MonoBehaviour
{
    [Header("Position on cards")]
    [Tooltip("All children darts have to be after start throw transform")]
    [SerializeField] private Transform startThrow;
    [SerializeField] private Vector3 posRange = new(0.0101f, 0.0f, 0.0055f);
    [SerializeField] private float rotRange = 20f;
    
    private List<GameObject> darts = new();
    private List<Rigidbody> rigidbodies = new();
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
            rigidbodies.Add(dart.GetComponent<Rigidbody>());
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
        dart.transform.position = to;
        
        RandomizePlacement(dart);
    }

    private void RandomizePlacement(GameObject dart)
    {
        var lPos = dart.transform.localPosition;
        var xPos = Random.Range(lPos.x - posRange.x, lPos.x + posRange.x);
        var yPos = lPos.y + 0.08f;
        var zPos = Random.Range(lPos.z - posRange.z, lPos.z + posRange.z);
        dart.transform.localPosition = new Vector3(xPos, yPos, zPos);

        dart.transform.rotation = Quaternion.Euler(-90, 0, 0);
        var leftRight = Random.Range(-rotRange, rotRange);
        var frontBack = Random.Range(-rotRange, rotRange);
        var rotX = Quaternion.AngleAxis(frontBack, Vector3.right);
        var rotZ = Quaternion.AngleAxis(leftRight, Vector3.forward);
        dart.transform.rotation = dart.transform.rotation * rotX * rotZ;
    }

    #endregion
}
