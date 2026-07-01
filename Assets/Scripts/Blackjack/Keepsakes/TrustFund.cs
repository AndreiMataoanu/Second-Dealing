using UnityEngine;

[CreateAssetMenu(fileName = "TrustFund", menuName = "Keepsakes/Trust Fund")]
public class TrustFund : Keepsake
{
    [Tooltip("The amount of passive income the player receives each round.")]
    public int passiveIncomeAmount = 50;

    public override int GetPassiveIncome()
    {
        return passiveIncomeAmount;
    }

    public override bool Consume()
    {
        return true;
    }
}