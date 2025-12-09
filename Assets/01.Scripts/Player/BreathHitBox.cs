using System;
using System.Collections;
using UnityEngine;

public class BreathHitBox : MonoBehaviour
{
    private Collider2D checkEnemy;
    // 빨아 들이는 속도
    [SerializeField] private float inBreathSpeed = 18.0f;
    // 플레이어와 적의 거리를 eatArea랑 비교해서 eatArea안에 들어오면 먹힘
    [SerializeField] private float eatArea = 0.3f;
    private Transform playerTransform;

    // 흡입을 할 대상 하나
    private Enemy currentEnemy;
    private Transform currentEnmeyTransform;
    
    private void Awake()
    {
        if (Player.instance != null)
        {
            playerTransform = Player.instance.transform;
        }

    }

    private void OnEnable()
    {
        currentEnemy = null;
        currentEnmeyTransform = null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (playerTransform == null)
        {
            return;
        }

        if (other.CompareTag("Enemy") == false)
        {
            return;
        }

        Enemy enemyOnCollider = other.GetComponentInParent<Enemy>();

        if (enemyOnCollider == null)
        {
            return;
        }

        if (currentEnemy == null)
        {
            currentEnemy = enemyOnCollider;
            currentEnmeyTransform = enemyOnCollider.transform;
        }

        if (currentEnemy != enemyOnCollider)
        {
            return;
        }

        // 커비 흡입 기능 처리

        Vector3 direction = (playerTransform.position - currentEnmeyTransform.position).normalized;

        currentEnmeyTransform.position += direction * inBreathSpeed * Time.deltaTime;

        float distance = Vector3.Distance(currentEnmeyTransform.position, playerTransform.position);

        if (distance <= eatArea)
        {
            if (PlayerAbilitySystem.instance !=null)
            {
                PlayerAbilitySystem.instance.EatEnemy(currentEnemy);
            }

            currentEnemy = null;
            currentEnmeyTransform = null;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentEnemy == null)
        {
            return;
        }

        if (other.CompareTag("Enemy") == false)
        {
            return;
        }

        Enemy enemyOnCollider = other.GetComponentInParent<Enemy>();

        if (enemyOnCollider == null)
        {
            return;
        }

        if (enemyOnCollider == currentEnemy)
        {
            currentEnemy = null;
            currentEnmeyTransform = null;
        }

    }
}
