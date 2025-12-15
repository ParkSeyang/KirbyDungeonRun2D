using UnityEngine;

public class AbilityEnemyController : EnemyAIControllerBase
{
    [Header("Ability AI")]
    [SerializeField] private bool alwaysChaseAndAttack = true;

    protected override bool CanChaseAndAttack()
    {
        return alwaysChaseAndAttack;
    }
}