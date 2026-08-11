using UnityEngine;

[CreateAssetMenu(fileName = "TrustFund", menuName = "Keepsakes/Trust Fund")]
public class TrustFund : Keepsake
{
    [Tooltip("The amount of passive income the player receives each round.")]
    public int passiveIncomeAmount = 50;
    private int multiplier = 1;

    private void OnEnable()
    {
        multiplier = 1;
    }

    public override void SetMembers(BlackjackGame blackjackGame)
    {
        multiplier = 1;
    }

    public void ScaleIncome()
    {
        multiplier *= 2;
    }

    public override int GetPassiveIncome()
    {
        return passiveIncomeAmount * multiplier;
    }

    public override bool Consume()
    {
        return true;
    }

    public override string GetDescription()
    {
        return $"{description}\n\nCurrent Income: ${GetPassiveIncome()}";
    }
}