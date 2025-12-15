using System.Collections;
using UnityEngine;
using EnemySystem.Data;
using EnemySystem.Runtime;

public abstract class EnemyAIControllerBase : MonoBehaviour, IEnemySpawnedInit
{
    protected static readonly int IS_MOVE   = Animator.StringToHash("IsMove");
    protected static readonly int ATTACK    = Animator.StringToHash("Attack");
    protected static readonly int IN_RANGE  = Animator.StringToHash("InRange");
    protected static readonly int IS_DAMAGE = Animator.StringToHash("IsDamage");
    protected static readonly int IS_SLEEP  = Animator.StringToHash("IsSleep");

    [Header("Reference")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected Transform patrolPoint;

    [Header("Move/AI")]
    [SerializeField] protected float moveSpeed = 1.2f;
    [SerializeField] protected float traceRange = 6.0f;
    [SerializeField] protected float attackRange = 2.0f;

    [Header("Timing")]
    [SerializeField] protected float idleWaitTime = 1.5f;
    [SerializeField] protected float attackCooldown = 2.0f;
    [SerializeField] protected float baseUpdateTerm = 0.1f;

    protected IEnumerator NextStateCoroutine;

    protected Vector2 pointA;
    protected Vector2 pointB;
    protected Vector2 goalPoint;

    private Coroutine damageFlagCoroutine;
    private EnemyAttackController attackController;
    private Facing2D facing;

    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }
        }

        attackController = GetComponent<EnemyAttackController>();

        facing = GetComponent<Facing2D>();
        if (facing == null)
        {
            facing = gameObject.AddComponent<Facing2D>();
        }

        pointA = transform.position;

        if (patrolPoint != null)
        {
            pointB = patrolPoint.position;
        }
        else
        {
            pointB = pointA + Vector2.right * 2.0f;
        }

        goalPoint = pointB;
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(FiniteStateMachineCoroutine());
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        damageFlagCoroutine = null;
    }

    public void OnSpawned(EnemySpawnedContext ctx)
    {
        if (ctx == null)
        {
            return;
        }

        if (ctx.IsInitialized == false)
        {
            return;
        }

        EnemyAIProfile ai = ctx.AIProfile;
        ai.Sanitize();

        if (ai.BaseUpdateTerm > 0.0f) { baseUpdateTerm = ai.BaseUpdateTerm; }
        if (ai.MoveSpeed > 0.0f) { moveSpeed = ai.MoveSpeed; }
        if (ai.TraceRange > 0.0f) { traceRange = ai.TraceRange; }

        if (ai.PatrolLocalOffset != Vector2.zero)
        {
            pointA = transform.position;
            pointB = pointA + ai.PatrolLocalOffset;
            goalPoint = pointB;
        }

        EnemyAttackProfile atk = ctx.AttackProfile;
        atk.Sanitize();

        if (atk.AttackCooldown > 0.0f) { attackCooldown = atk.AttackCooldown; }

        EnemyStatTemplate template = ctx.RuntimeStat.Template;

        if (template.AttackRange > 0.0f)
        {
            attackRange = template.AttackRange;
        }
        else if (atk.AttackType == EnemyAttackType.Ranged && atk.RangedAttackRange > 0.0f)
        {
            attackRange = atk.RangedAttackRange;
        }
    }

    private IEnumerator FiniteStateMachineCoroutine()
    {
        NextStateCoroutine = IdleStateCoroutine();

        while (gameObject != null && gameObject.activeInHierarchy == true)
        {
            yield return StartCoroutine(NextStateCoroutine);
        }
    }

    protected virtual IEnumerator IdleStateCoroutine()
    {
        SetBoolSafe(IS_MOVE, false);

        float wait = 0.0f;
        WaitForSeconds waitTerm = new WaitForSeconds(baseUpdateTerm);

        OnIdleParamUpdate(true);

        while (true)
        {
            if (TryEnterAttack() == true)
            {
                OnIdleParamUpdate(false);
                yield break;
            }

            if (wait >= idleWaitTime)
            {
                NextStateCoroutine = PatrolStateCoroutine();
                OnIdleParamUpdate(false);
                yield break;
            }

            yield return waitTerm;
            wait += baseUpdateTerm;
        }
    }

    protected virtual IEnumerator PatrolStateCoroutine()
    {
        SetBoolSafe(IS_MOVE, true);

        while (true)
        {
            if (TryEnterAttack() == true)
            {
                SetBoolSafe(IS_MOVE, false);
                yield break;
            }

            if (IsNear(transform.position, goalPoint, 0.1f) == true)
            {
                goalPoint = (IsNear(transform.position, pointB, 0.1f) == true) ? pointA : pointB;
                SetBoolSafe(IS_MOVE, false);
                NextStateCoroutine = IdleStateCoroutine();
                yield break;
            }

            MoveTowardsX(goalPoint);
            yield return null;
        }
    }

    protected virtual IEnumerator AttackStateCoroutine()
    {
        while (true)
        {
            Transform playerTr = ResolvePlayerTransform();

            if (playerTr == null)
            {
                SetBoolSafe(IN_RANGE, false);
                SetBoolSafe(IS_MOVE, false);
                NextStateCoroutine = IdleStateCoroutine();
                yield break;
            }

            bool inTrace = IsInRange(playerTr.position, traceRange);
            bool inAttack = IsInRange(playerTr.position, attackRange);

            SetBoolSafe(IN_RANGE, inAttack);

            if (inTrace == false)
            {
                SetBoolSafe(IN_RANGE, false);
                NextStateCoroutine = PatrolStateCoroutine();
                yield break;
            }

            if (inAttack == true)
            {
                SetBoolSafe(IS_MOVE, false);

                if (attackController != null)
                {
                    attackController.RequestAttack(playerTr);
                }

                TriggerSafe(ATTACK);
                yield return new WaitForSeconds(attackCooldown);
            }
            else
            {
                SetBoolSafe(IS_MOVE, true);
                MoveTowardsX(playerTr.position);
                yield return null;
            }
        }
    }

    protected abstract bool CanChaseAndAttack();

    protected virtual bool TryEnterAttack()
    {
        if (CanChaseAndAttack() == false)
        {
            return false;
        }

        Transform playerTr = ResolvePlayerTransform();

        if (playerTr == null)
        {
            return false;
        }

        bool inTrace = IsInRange(playerTr.position, traceRange);

        if (inTrace == false)
        {
            return false;
        }

        NextStateCoroutine = AttackStateCoroutine();
        return true;
    }

    protected virtual void OnIdleParamUpdate(bool isIdle)
    {
    }

    public void PlayDamageFlag()
    {
        if (animator == null)
        {
            return;
        }

        if (HasParam(animator, IS_DAMAGE, AnimatorControllerParameterType.Bool) == false)
        {
            return;
        }

        if (damageFlagCoroutine != null)
        {
            StopCoroutine(damageFlagCoroutine);
            damageFlagCoroutine = null;
        }

        damageFlagCoroutine = StartCoroutine(DamageFlagCoroutine());
    }

    private IEnumerator DamageFlagCoroutine()
    {
        SetBoolSafe(IS_DAMAGE, true);
        yield return null;
        SetBoolSafe(IS_DAMAGE, false);
        damageFlagCoroutine = null;
    }

    protected Transform ResolvePlayerTransform()
    {
        if (PlayerController.localPlayer != null)
        {
            return PlayerController.localPlayer.transform;
        }

        if (Player.instance != null)
        {
            return Player.instance.transform;
        }

        return null;
    }

    protected bool IsInRange(Vector2 point, float range)
    {
        return (Vector2.Distance(transform.position, point) <= range);
    }

    protected bool IsNear(Vector2 a, Vector2 b, float threshold)
    {
        float th = threshold;

        if (th <= 0.0f)
        {
            th = 0.01f;
        }

        Vector2 d = a - b;
        return d.sqrMagnitude <= (th * th);
    }

    protected void MoveTowardsX(Vector3 goal)
    {
        float dx = goal.x - transform.position.x;

        if (Mathf.Abs(dx) <= 0.0001f)
        {
            return;
        }

        int dir = (dx >= 0.0f) ? 1 : -1;

        transform.Translate(new Vector3(dir * moveSpeed * Time.deltaTime, 0.0f, 0.0f));

        if (facing != null)
        {
            facing.SetFacingDir(dir);
        }
    }

    protected void SetBoolSafe(int hash, bool value)
    {
        if (animator == null)
        {
            return;
        }

        if (HasParam(animator, hash, AnimatorControllerParameterType.Bool) == false)
        {
            return;
        }

        animator.SetBool(hash, value);
    }

    protected void SetIntSafe(int hash, int value)
    {
        if (animator == null)
        {
            return;
        }

        if (HasParam(animator, hash, AnimatorControllerParameterType.Int) == false)
        {
            return;
        }

        animator.SetInteger(hash, value);
    }

    protected void TriggerSafe(int hash)
    {
        if (animator == null)
        {
            return;
        }

        if (HasParam(animator, hash, AnimatorControllerParameterType.Trigger) == false)
        {
            return;
        }

        animator.SetTrigger(hash);
    }

    protected bool HasParam(Animator anim, int hash, AnimatorControllerParameterType type)
    {
        if (anim == null)
        {
            return false;
        }

        var ps = anim.parameters;

        if (ps == null)
        {
            return false;
        }

        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].nameHash == hash && ps[i].type == type)
            {
                return true;
            }
        }

        return false;
    }
}
