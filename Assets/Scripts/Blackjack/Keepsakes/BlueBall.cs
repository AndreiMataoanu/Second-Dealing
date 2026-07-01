using UnityEngine;

[CreateAssetMenu(fileName = "BlueBall", menuName = "Keepsakes/Blue Ball")]
public class BlueBall : Keepsake
{
    public override int GetDealerBustModifier()
    {
        return 1;
    }
}