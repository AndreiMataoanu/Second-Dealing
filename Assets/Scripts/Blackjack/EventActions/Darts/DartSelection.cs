using System.Collections.Generic;
using UnityEngine;

public class DartSelection : MonoBehaviour
{
    private List<GameObject> darts = new();
    
    private int dartCount;
    private int activeDartCount;

    private void Start()
    {
        dartCount = transform.childCount;
        
        for (int i = 0; i < dartCount; i++)
            darts.Add(transform.GetChild(i).gameObject);
    }
    
    public void SetDartSelectionActive(bool isActive) => gameObject.SetActive(isActive);

    public void SetActiveDartCount(int dartNumber)
    {
        var count = Mathf.Min(dartCount, dartNumber);
        activeDartCount = count;

        for (int i = 0; i < count; i++)
            darts[i].SetActive(true);
        
        for (int i = count; i < dartCount; i++)
            darts[i].SetActive(false);
    }

    public void UseDartAtIndex(int index)
    {
        if (index >= activeDartCount) return;
        
        DeactivateDart(index);

        Debug.Log("throw dart");
    }

    private void DeactivateDart(int index)
    {
        darts[index].SetActive(false);
        
        activeDartCount--;
        if (activeDartCount == 0) SetDartSelectionActive(false);
    }
}
