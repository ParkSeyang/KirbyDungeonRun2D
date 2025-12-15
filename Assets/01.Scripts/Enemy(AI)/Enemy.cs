using UnityEngine;
using EnemySystem.Data;
using EnemySystem.Runtime;

public class Enemy : MonoBehaviour
{
    [Header("Debug/Compatibility")]
    [SerializeField] public BaseStat.AbilityType enemyType = BaseStat.AbilityType.Normal;

    private Animator animator;

    // 중복 처리 방지(컨텍스트 없는 폴백에서만 사용)
    private bool isDead = false;

    // 값복사 기반 런타임 스탯(컨텍스트 없을 때 폴백용)
    private EnemyRuntimeStat runtimeStat;
    private bool hasRuntimeStat = false;

    // 컨텍스트 캐시
    private EnemySpawnedContext spawnedContext;

    public EnemyRuntimeStat RuntimeStat
    {
        get { return runtimeStat; }
    }

    public BaseStat.AbilityType enemyAbilityType
    {
        get { return enemyType; }
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        EnsureRuntimeStatFallback();
    }

    public void ApplyRuntimeStat(EnemyRuntimeStat newRuntime)
    {
        if (isDead == true)
        {
            return;
        }

        runtimeStat = newRuntime;
        hasRuntimeStat = true;

        enemyType = runtimeStat.Template.AbilityType;
    }

    // ✅ 통일 API (source, hitPos 포함)
    public bool ApplyDamage(GameObject source, int damage, Vector3 hitPosition)
    {
        if (damage <= 0)
        {
            return false;
        }

        EnemySpawnedContext ctx = GetSpawnedContext();

        if (ctx != null && ctx.IsInitialized == true)
        {
            return ctx.ApplyDamage(source, damage, hitPosition);
        }

        // ===== 폴백(컨텍스트 없는 경우) =====
        if (isDead == true)
        {
            return true;
        }

        if (hasRuntimeStat == false)
        {
            EnsureRuntimeStatFallback();
        }

        bool died = runtimeStat.ApplyDamage(damage);

        if (died == true)
        {
            DieFallback();
            return true;
        }

        return false;
    }

    // ✅ 새 표준 입력(추천): 패킷 기반
    public void TakeDamage(DamagePacket packet)
    {
        if (packet == null)
        {
            return;
        }

        ApplyDamage(packet.Source, packet.Amount, packet.Position);
    }

    // ✅ 기존 호환 입력
    public void TakeDamage(float amount)
    {
        if (amount <= 0.0f)
        {
            return;
        }

        if (float.IsNaN(amount) == true)
        {
            return;
        }

        if (float.IsInfinity(amount) == true)
        {
            return;
        }

        int damageInt = Mathf.RoundToInt(amount);

        if (damageInt <= 0)
        {
            return;
        }

        ApplyDamage(null, damageInt, transform.position);
    }

    private EnemySpawnedContext GetSpawnedContext()
    {
        if (spawnedContext != null)
        {
            return spawnedContext;
        }

        spawnedContext = GetComponent<EnemySpawnedContext>();
        return spawnedContext;
    }

    private void EnsureRuntimeStatFallback()
    {
        if (hasRuntimeStat == true)
        {
            return;
        }

        EnemyStatTemplate t = new EnemyStatTemplate();
        t.Name = enemyType.ToString();
        t.AbilityType = enemyType;

        if (enemyType == BaseStat.AbilityType.Normal)
        {
            t.MaxHp = 10;
            t.Attack = 5;
        }
        else if (enemyType == BaseStat.AbilityType.Cutter)
        {
            t.MaxHp = 20;
            t.Attack = 10;
        }
        else if (enemyType == BaseStat.AbilityType.Sword)
        {
            t.MaxHp = 20;
            t.Attack = 15;
        }
        else
        {
            t.MaxHp = 10;
            t.Attack = 5;
        }

        runtimeStat = t.CreateRuntime();
        hasRuntimeStat = true;
    }

    private void DieFallback()
    {
        if (isDead == true)
        {
            return;
        }

        isDead = true;

        Collider2D[] cols = GetComponentsInChildren<Collider2D>();

        if (cols != null)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] != null)
                {
                    cols[i].enabled = false;
                }
            }
        }

        Destroy(gameObject);
    }
}
