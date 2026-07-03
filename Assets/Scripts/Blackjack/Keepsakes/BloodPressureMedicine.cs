using UnityEngine;

[CreateAssetMenu(fileName = "BloodPressureMedicine", menuName = "Keepsakes/BloodPressureMedicine")]
public class BloodPressureMedicine : Keepsake
{
    public override bool AllowPostStandItem()
    {
        return true;
    }
}