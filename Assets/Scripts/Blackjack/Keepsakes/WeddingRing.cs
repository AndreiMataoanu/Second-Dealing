using UnityEngine;

[CreateAssetMenu(fileName = "WeddingRing", menuName = "Keepsakes/Wedding Ring")]
public class WeddingRing : Keepsake
{
    public override bool AllowEndlessDoubleDown()
    {
        return true;
    }
}