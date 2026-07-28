using UnityEngine;

[CreateAssetMenu(fileName = "Inheritance", menuName = "Keepsakes/Inheritance")]
public class Inheritance : Keepsake
{
    public override void ApplyInheritance(BlackjackGame game)
    {
        int previousMoney = PlayerPrefs.GetInt("PreviousRunMoney", 0);
        int inheritance = Mathf.FloorToInt(previousMoney / 2f);

        if(inheritance > 100000)
        {
            inheritance = 100000;
        }

        if(inheritance > 0)
        {
            game.AddInheritanceMoney(inheritance);

            PlayerPrefs.SetInt("PreviousRunMoney", 0);
        }
    }

    public override string GetDescription()
    {
        int previousMoney = PlayerPrefs.GetInt("PreviousRunMoney", 0);

        return $"{description}\n\nInherited this run: ${previousMoney / 2f}";
    }
}