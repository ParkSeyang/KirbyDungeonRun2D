using UnityEngine;

[DisallowMultipleComponent]
public class EnemyProjectile : MonoBehaviour
{
    private GameObject owner;
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private int damage;
    private LayerMask targetMask;

    private Vector3 startPos;

    private bool isInitialized = false;

    public void Initialize(GameObject ownerObject, Vector2 dir, float moveSpeed, float maxDist, int damageAmount, LayerMask mask)
    {
        owner = ownerObject;
        direction = dir.normalized;

        speed = moveSpeed;
        if (speed <= 0.0f) { speed = 6.0f; }

        maxDistance = maxDist;
        if (maxDistance <= 0.0f) { maxDistance = 10.0f; }

        damage = damageAmount;
        if (damage < 0) { damage = 0; }

        targetMask = mask;

        startPos = transform.position;
        isInitialized = true;
    }

    private void Update()
    {
        if (isInitialized == false)
        {
            return;
        }

        transform.Translate(direction * (speed * Time.deltaTime));

        float dist = Vector3.Distance(startPos, transform.position);

        if (dist >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (isInitialized == false)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        if (owner != null)
        {
            bool isOwner = (other.gameObject == owner) || other.transform.IsChildOf(owner.transform);

            if (isOwner == true)
            {
                return;
            }
        }

        Transform playerTr = ResolvePlayerTransform();

        if (playerTr != null)
        {
            bool isPlayer = (other.transform == playerTr) || other.transform.IsChildOf(playerTr);

            if (isPlayer == true)
            {
                if (damage > 0)
                {
                    playerTr.gameObject.SendMessage("TakeDamage", (float)damage, SendMessageOptions.DontRequireReceiver);
                }

                Destroy(gameObject);
                return;
            }
        }

        Destroy(gameObject);
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
}
