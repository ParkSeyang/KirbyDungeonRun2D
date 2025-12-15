using System.Collections.Generic;
using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    [Header("HitBox")]
    [SerializeField] private Collider2D hitCollider;

    [Header("Runtime")]
    [SerializeField] private float damage = 1.0f;

    [Header("Option")]
    [SerializeField] private bool disableGameObjectWhenIdle = true;

    private bool isActive = false;
    private HashSet<int> hitEnemyIds;

    private bool isInitialized = false;

    private void Awake()
    {
        EnsureInitialized();

        // 기본은 꺼진 상태로 시작
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        isActive = false;

        // ⚠️ 여기서 gameObject.SetActive(false)를 자동으로 하지 않는 이유:
        // - 프리팹/애니메이션 미리보기/편집할 때 불편해질 수 있어서
        // 대신 "Hierarchy에서 기본 OFF"로 세팅하는 것을 추천.
    }

    private void OnDisable()
    {
        // 비활성화될 때도 안전하게 정리
        isActive = false;

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        if (hitEnemyIds != null)
        {
            hitEnemyIds.Clear();
        }
    }

    private void EnsureInitialized()
    {
        if (isInitialized == true)
        {
            return;
        }

        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider2D>();
        }

        if (hitEnemyIds == null)
        {
            hitEnemyIds = new HashSet<int>();
        }

        // Trigger가 아니면 HitBox로 동작 불가
        if (hitCollider != null && hitCollider.isTrigger == false)
        {
            Debug.LogWarning("[AttackHitBox] Collider2D isTrigger가 꺼져있습니다. HitBox는 Trigger여야 합니다.");
        }

        isInitialized = true;
    }

    public void BeginHitBox(float newDamage)
    {
        // ===== 예외/가드 =====
        if (newDamage <= 0.0f)
        {
            return;
        }

        EnsureInitialized();

        if (hitCollider == null)
        {
            return;
        }

        if (hitCollider.isTrigger == false)
        {
            return;
        }

        // ✅ 필요할 때만 오브젝트를 켠다
        if (disableGameObjectWhenIdle == true && gameObject.activeSelf == false)
        {
            gameObject.SetActive(true);

            // SetActive(true)로 Awake가 아직 안 돌았을 수 있으니 한 번 더 안전하게
            EnsureInitialized();
        }

        damage = newDamage;

        if (hitEnemyIds == null)
        {
            hitEnemyIds = new HashSet<int>();
        }

        hitEnemyIds.Clear();

        isActive = true;
        hitCollider.enabled = true;
    }

    public void EndHitBox()
    {
        EnsureInitialized();

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        if (hitEnemyIds != null)
        {
            hitEnemyIds.Clear();
        }

        isActive = false;

        // ✅ 판정이 끝나면 오브젝트를 다시 끈다
        if (disableGameObjectWhenIdle == true && gameObject.activeSelf == true)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    // Enable 시점에 이미 겹쳐있는 경우 Enter가 안 뜰 수 있어서 Stay도 같이 받는 게 안전
    private void OnTriggerStay2D(Collider2D other)
    {
        TryHit(other);
    }

    private void TryHit(Collider2D other)
    {
        // ===== 예외/가드 =====
        if (isActive == false)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        if (other.CompareTag("Enemy") == false)
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

        enemy.TakeDamage(damage);
    }
}
