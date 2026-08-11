using UnityEngine;

[CreateAssetMenu(fileName = "LoyaltyCard", menuName = "Keepsakes/Loyalty Card")]
public class LoyaltyCard : Keepsake
{
    [Tooltip("Discount percentage as a decimal (0.10 = 10% off).")]
    [Range(0f, 0.99f)]
    public float discountPercentage = 0.10f;

    public override float GetShopDiscount()
    {
        return discountPercentage;
    }
}
