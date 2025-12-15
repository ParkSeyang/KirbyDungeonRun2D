using System.Collections.Generic;
using UnityEngine;

public class CutterBullet : MonoBehaviour
{
    [Header("Life")]
    [SerializeField] private float lifeTime = 2.0f;

    [Header("Move")]
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float maxDistance = 3.0f;       // StartPosition -> MaxRangePosition 거리
    [SerializeField] private float reachThreshold = 0.12f;    // 목표점 도달 판정
    [SerializeField] private float catchRadius = 0.6f;        // StartPosition 근처에 플레이어가 있으면 여기서 소멸

    [Header("Curve (Speed Multiplier)")]
    [SerializeField] private AnimationCurve outCurve = new AnimationCurve(
        new Keyframe(0.0f, 1.0f),
        new Keyframe(1.0f, 0.6f)
    );

    [SerializeField] private AnimationCurve returnCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.7f),
        new Keyframe(1.0f, 1.3f)
    );

    [Header("Damage")]
    [SerializeField] private float damage = 5.0f;

    private Rigidbody2D rb;

    // 핵심 좌표 (2D 통일)
    private Vector2 StartPosition;
    private Vector2 MaxRangePosition;

    // 상태
    private enum Phase
    {
        Out,
        Return,
        Pass,
    }

    private Phase phase = Phase.Out;

    // 방향(던질 때 기준)
    private Vector2 throwDir;

    // 플레이어(던진 위치 비교용)
    private Transform playerTransform;

    // 적 중복 타격 방지(유지해도 무방)
    private HashSet<int> hitEnemyIds;

    // ✅ 추가: “첫 적중 후” 추가 충돌 무시
    private bool hasHitEnemy = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (hitEnemyIds == null)
        {
            hitEnemyIds = new HashSet<int>();
        }
    }

    private void OnEnable()
    {
        if (hitEnemyIds != null)
        {
            hitEnemyIds.Clear();
        }

        phase = Phase.Out;
        hasHitEnemy = false;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // PlayerAbilityController에서 호출하는 시그니처 유지(깨지면 안 됨)
    public void Fire(Vector2 dir, float newSpeed, float newDamage, float newMaxDistance, Vector2 originPos, Transform newPlayerTransform)
    {
        // ===== 예외/가드 =====
        if (newSpeed <= 0.0f)
        {
            return;
        }

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            return;
        }

        Vector2 n = dir;

        if (n.sqrMagnitude <= 0.0001f)
        {
            n = Vector2.right;
        }

        n.Normalize();

        speed = newSpeed;
        damage = newDamage;
        maxDistance = newMaxDistance;

        playerTransform = newPlayerTransform;

        StartPosition = originPos;
        throwDir = n;

        MaxRangePosition = StartPosition + (throwDir * maxDistance);

        phase = Phase.Out;
        hasHitEnemy = false;

        rb.gravityScale = 0.0f;
        rb.linearDamping = 0.0f;

        rb.bodyType = RigidbodyType2D.Kinematic;

        // 시작 위치 고정
        rb.position = originPos;
        transform.position = new Vector3(originPos.x, originPos.y, transform.position.z);
    }

    private void FixedUpdate()
    {
        // ===== 예외/가드 =====
        if (rb == null)
        {
            return;
        }

        // ✅ 이미 적중해서 곧 파괴될 예정이면 이동 불필요
        if (hasHitEnemy == true)
        {
            return;
        }

        float dt = Time.fixedDeltaTime;

        if (dt <= 0.0f)
        {
            return;
        }

        Vector2 currentPos = rb.position;

        if (phase == Phase.Out)
        {
            float progressOut = CalculateProgressOut(currentPos);
            float mult = EvaluateCurveSafe(outCurve, progressOut);
            float step = speed * mult * dt;

            Vector2 next = Vector2.MoveTowards(currentPos, MaxRangePosition, step);
            rb.MovePosition(next);

            if (IsNear(next, MaxRangePosition, reachThreshold) == true)
            {
                phase = Phase.Return;
            }

            return;
        }

        if (phase == Phase.Return)
        {
            float progressReturn = CalculateProgressReturn(currentPos);
            float mult = EvaluateCurveSafe(returnCurve, progressReturn);
            float step = speed * mult * dt;

            Vector2 next = Vector2.MoveTowards(currentPos, StartPosition, step);
            rb.MovePosition(next);

            if (IsNear(next, StartPosition, reachThreshold) == true)
            {
                bool playerNearStart = IsPlayerNearStart();

                if (playerNearStart == true)
                {
                    Destroy(gameObject);
                    return;
                }

                phase = Phase.Pass;
            }

            return;
        }

        if (phase == Phase.Pass)
        {
            Vector2 passDir = new Vector2(-throwDir.x, -throwDir.y);
            Vector2 stepVec = passDir * speed * dt;

            Vector2 next = currentPos + stepVec;
            rb.MovePosition(next);
        }
    }

    private float CalculateProgressOut(Vector2 currentPos)
    {
        float total = maxDistance;

        if (total <= 0.0001f)
        {
            return 1.0f;
        }

        float distToMax = Vector2.Distance(currentPos, MaxRangePosition);
        float progress = 1.0f - (distToMax / total);

        return Mathf.Clamp01(progress);
    }

    private float CalculateProgressReturn(Vector2 currentPos)
    {
        float total = maxDistance;

        if (total <= 0.0001f)
        {
            return 1.0f;
        }

        float distToStart = Vector2.Distance(currentPos, StartPosition);
        float progress = 1.0f - (distToStart / total);

        return Mathf.Clamp01(progress);
    }

    private float EvaluateCurveSafe(AnimationCurve curve, float t)
    {
        if (curve == null)
        {
            return 1.0f;
        }

        return curve.Evaluate(t);
    }

    private bool IsNear(Vector2 a, Vector2 b, float threshold)
    {
        float th = threshold;

        if (th <= 0.0f)
        {
            th = 0.01f;
        }

        Vector2 d = a - b;
        float sq = d.sqrMagnitude;

        return sq <= (th * th);
    }

    private bool IsPlayerNearStart()
    {
        if (playerTransform == null)
        {
            return false;
        }

        float r = catchRadius;

        if (r <= 0.0f)
        {
            r = 0.2f;
        }

        Vector2 p = playerTransform.position;
        Vector2 d = p - StartPosition;

        return d.sqrMagnitude <= (r * r);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ===== 예외/가드 =====
        if (other == null)
        {
            return;
        }

        if (hasHitEnemy == true)
        {
            return;
        }

        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy == null)
        {
            return;
        }

        if (hitEnemyIds == null)
        {
            hitEnemyIds = new HashSet<int>();
        }

        int id = enemy.GetInstanceID();

        if (hitEnemyIds.Contains(id) == true)
        {
            return;
        }

        hitEnemyIds.Add(id);

        if (damage > 0.0f)
        {
            enemy.TakeDamage(damage);
        }

        // ✅ 1타 적중 즉시 소멸(관통 제거)
        hasHitEnemy = true;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject);
    }
}
