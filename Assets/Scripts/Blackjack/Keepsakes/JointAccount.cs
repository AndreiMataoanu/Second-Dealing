using UnityEngine;

[CreateAssetMenu(fileName = "JointAccount", menuName = "Keepsakes/Joint Account")]
public class JointAccount : Keepsake
{
    public override bool AllowOverdraft()
    {
        return true;
    }
}