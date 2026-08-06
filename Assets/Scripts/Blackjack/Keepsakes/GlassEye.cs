using UnityEngine;

[CreateAssetMenu(fileName = "GlassEye", menuName = "Keepsakes/Glass Eye")]
public class GlassEye : Keepsake
{
    public override bool ForceRevealDealerCard()
    {
        return true;
    }
}