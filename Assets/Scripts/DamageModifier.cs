using UnityEngine;

public class DamageModifier : MonoBehaviour
{
    public float IncomingDamageMultiplier { get; set; } = 1f;
    public float OutgoingDamageMultiplier { get; set; } = 1f;
    public int BonusDamageOnNextHit { get; set; }

    public void ConsumeBonusDamageOnHit()
    {
        BonusDamageOnNextHit = 0;
    }
}
