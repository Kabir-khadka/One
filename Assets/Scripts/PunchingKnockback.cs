using UnityEngine;

public class PunchingKnockback : ObstacleBase
{

    [SerializeField] private float punchForce = 15f;

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return punchForce; //Use custome force instead of baseKnockbackForce
    }
}
