using UnityEngine;
using EnemySystem.Runtime;

[DisallowMultipleComponent]
public class DamageDealer2D : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Source (optional)")]
    [SerializeField] private GameObject source;

    [Header("Option")]
    [SerializeField] private bool destroySelfOnHit = false;

    private readonly DamagePacket sharedPacket = new DamagePacket();

    private void Awake()
    {
        if (source == null)
        {
            source = gameObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryDealDamage(collision.collider);
    }

    private void TryDealDamage(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        EnemySpawnedContext ctx = other.GetComponentInParent<EnemySpawnedContext>();

        if (ctx == null || ctx.IsInitialized == false)
        {
            return;
        }

        Vector3 hitPos = other.ClosestPoint(transform.position);

        sharedPacket.Source = source;
        sharedPacket.Amount = damage;
        sharedPacket.Position = hitPos;

        // Enemy.TakeDamage(DamagePacket) → EnemySpawnedContext.ApplyDamage(source, damage, hitPos)로 통일됨
        Enemy enemy = ctx.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(sharedPacket);
        }
        else
        {
            ctx.ApplyDamage(source, damage, hitPos);
        }

        if (destroySelfOnHit == true)
        {
            Destroy(gameObject);
        }
    }
}