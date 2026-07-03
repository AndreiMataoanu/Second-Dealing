using UnityEngine;

[CreateAssetMenu(fileName = "Tarot", menuName = "Keepsakes/Tarot")]
public class Tarot : Keepsake
{
    public override bool AddsTarotCards()
    {
        return true;
    }
}