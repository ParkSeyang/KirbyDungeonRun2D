using System.Collections;
using UnityEngine;
using EnemySystem.Data;
using EnemySystem.Runtime;

[DisallowMultipleComponent]
public class EnemyAttackController : MonoBehaviour, IEnemySpawnedInit
{
    [Header("Optional Filter")]
    [SerializeField] private LayerMask targetMask = -1;

    private EnemySpawnedContext ctx;
    private EnemyAttackProfile attackProfile;
    private EnemyEffectSet effectSet;

    private bool isInitialized = false;

    private float nextAttackTime = 0.0f;
    private float nextContactHitTime = 0.0f;

    private Coroutine meleeCoroutine;
    private readonly Collider2D[] overlapResults = new Collider2D[8];

    private Facing2D facing;

    private void Awake()
    {
        facing = GetComponent<Facing2D>();
        if (facing == null)
        {
            facing = gameObject.AddComponent<Facing2D>();
        }
    }

    public void OnSpawned(EnemySpawnedContext context)
    {
        if (context == null)
        {
            return;
        }

        if (context.IsInitialized == false)
        {
            return;
        }

        ctx = context;

        attackProfile = ctx.AttackProfile;
        attackProfile.Sanitize();

        effectSet = ctx.EffectSet;
        effectSet.Sanitize();

        isInitialized = true;
        nextAttackTime = 0.0f;
        nextContactHitTime = 0.0f;
    }

    public void RequestAttack(Transform target)
    {
        if (isInitialized == false)
        {
            return;
        }

        if (ctx == null)
        {
            return;
        }

        if (attackProfile.AttackType == EnemyAttackType.None)
        {
            return;
        }

        if (attackProfile.AttackType == EnemyAttackType.Contact)
        {
            return;
        }

        float now = Time.time;

        if (now < nextAttackTime)
        {
            return;
        }

        float cd = attackProfile.AttackCooldown;

        if (cd < 0.0f)
        {
            cd = 0.0f;
        }

        nextAttackTime = now + cd;

        if (attackProfile.AttackType == EnemyAttackType.Melee)
        {
            StartMeleeAttack();
            return;
        }

        if (attackProfile.AttackType == EnemyAttackType.Ranged)
        {
            FireProjectile(target);
            return;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryContactDamage(other);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryContactDamage(collision.collider);
    }

    private void TryContactDamage(Collider2D other)
    {
        if (isInitialized == false)
        {
            return;
        }

        if (ctx == null)
        {
            return;
        }

        if (attackProfile.AttackType != EnemyAttackType.Contact)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        Transform playerTr = ResolvePlayerTransform();

        if (playerTr == null)
        {
            return;
        }

        bool isPlayer = (other.transform == playerTr) || other.transform.IsChildOf(playerTr);

        if (isPlayer == false)
        {
            return;
        }

        float now = Time.time;

        if (now < nextContactHitTime)
        {
            return;
        }

        float interval = attackProfile.ContactHitInterval;

        if (interval < 0.0f)
        {
            interval = 0.0f;
        }

        nextContactHitTime = now + interval;

        ApplyDamageToPlayer(playerTr, ctx.Attack);
    }

    private void StartMeleeAttack()
    {
        if (meleeCoroutine != null)
        {
            StopCoroutine(meleeCoroutine);
            meleeCoroutine = null;
        }

        meleeCoroutine = StartCoroutine(MeleeWindowCoroutine());
    }

    private IEnumerator MeleeWindowCoroutine()
    {
        if (ctx == null)
        {
            yield break;
        }

        Transform playerTr = ResolvePlayerTransform();

        if (playerTr == null)
        {
            yield break;
        }

        float activeTime = attackProfile.MeleeActiveTime;

        if (activeTime <= 0.0f)
        {
            activeTime = 0.05f;
        }

        float endTime = Time.time + activeTime;
        bool hitApplied = false;

        while (Time.time < endTime)
        {
            if (hitApplied == false)
            {
                int count = Physics2D.OverlapBoxNonAlloc(
                    GetMeleeCenter(),
                    attackProfile.MeleeHitBoxSize,
                    0.0f,
                    overlapResults,
                    targetMask
                );

                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        Collider2D col = overlapResults[i];

                        if (col == null)
                        {
                            continue;
                        }

                        bool isPlayer = (col.transform == playerTr) || col.transform.IsChildOf(playerTr);

                        if (isPlayer == true)
                        {
                            ApplyDamageToPlayer(playerTr, ctx.Attack);
                            hitApplied = true;
                            break;
                        }
                    }
                }
            }

            yield return null;
        }

        meleeCoroutine = null;
    }

    private Vector2 GetMeleeCenter()
    {
        int dir = GetFacingDir();

        Vector2 offset = attackProfile.MeleeHitBoxOffset;
        offset.x = offset.x * dir;

        return (Vector2)transform.position + offset;
    }

    private void FireProjectile(Transform target)
    {
        if (effectSet.ProjectilePrefab == null)
        {
            return;
        }

        int dir = GetFacingDir();

        Vector3 spawnPos = transform.position + GetProjectileOffset(dir);
        GameObject projObj = Instantiate(effectSet.ProjectilePrefab, spawnPos, Quaternion.identity);

        if (projObj == null)
        {
            return;
        }

        EnemyProjectile proj = projObj.GetComponent<EnemyProjectile>();

        if (proj == null)
        {
            proj = projObj.AddComponent<EnemyProjectile>();
        }

        Vector2 dirVec = (dir >= 0) ? Vector2.right : Vector2.left;

        float speed = attackProfile.ProjectileSpeed;
        float maxDist = attackProfile.ProjectileMaxDistance;

        proj.Initialize(gameObject, dirVec, speed, maxDist, ctx.Attack, targetMask);
    }

    private Vector3 GetProjectileOffset(int dir)
    {
        Vector3 o = effectSet.ProjectileSpawnOffset;
        o.x = o.x * dir;
        return o;
    }

    private int GetFacingDir()
    {
        if (facing != null)
        {
            return facing.GetFacingDir();
        }

        float sx = transform.localScale.x;
        return (sx >= 0.0f) ? 1 : -1;
    }

    private Transform ResolvePlayerTransform()
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

    private void ApplyDamageToPlayer(Transform playerTr, int damage)
    {
        if (playerTr == null)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        playerTr.gameObject.SendMessage("TakeDamage", (float)damage, SendMessageOptions.DontRequireReceiver);
    }
}
