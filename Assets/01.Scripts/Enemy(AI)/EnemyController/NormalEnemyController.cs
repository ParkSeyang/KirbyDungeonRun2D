using UnityEngine;

public class NormalEnemyController : EnemyAIControllerBase
{
    public enum NormalAIType
    {
        PatrolOnly,
        PatrolChaseAttack,
    }

    [Header("Normal AI")]
    [SerializeField] private NormalAIType aiType = NormalAIType.PatrolOnly;

    [Header("NormalEnemy4 Option (IsSleep)")]
    [SerializeField] private bool useSleepParamOnIdle = false;
    [SerializeField] private int sleepValueOnIdle = 1;
    [SerializeField] private int sleepValueOnActive = 0;

    protected override bool CanChaseAndAttack()
    {
        return (aiType == NormalAIType.PatrolChaseAttack);
    }

    protected override void OnIdleParamUpdate(bool isIdle)
    {
        if (useSleepParamOnIdle == false)
        {
            return;
        }

        if (isIdle == true)
        {
            SetIntSafe(IS_SLEEP, sleepValueOnIdle);
        }
        else
        {
            SetIntSafe(IS_SLEEP, sleepValueOnActive);
        }
    }
}