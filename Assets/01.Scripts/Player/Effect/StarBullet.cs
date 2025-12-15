using UnityEngine;

public class StarBullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3.0f;
    [SerializeField] private float damage = 10.0f;

    [Header("Physics Fix")]
    [SerializeField] private bool forceNoGravity = true;
    [SerializeField] private float forceLinearDrag = 0.0f;

    [Header("Spin Setting")]
    [SerializeField] private float spinDegreesPerSecond = 180.0f; // 초당 회전 각도(도). 90~360 추천

    private Rigidbody2D rb;

    private void Awake()
    {
        // ===== 예외/가드 =====
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            return;
        }

        if (forceNoGravity == true)
        {
            rb.gravityScale = 0.0f;
        }

        if (forceLinearDrag >= 0.0f)
        {
            rb.linearDamping = forceLinearDrag; // Unity 6 기준
        }

        // 회전 사용
        rb.freezeRotation = false;

        // 천천히 회전(deg/sec)
        rb.angularVelocity = spinDegreesPerSecond;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[StarBullet] Trigger Enter: " + other.name);
        // ===== 예외/가드 =====
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

        if (damage <= 0.0f)
        {
            Destroy(gameObject);
            return;
        }

        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}