using System;
using System.Collections;
using UnityEngine;

public class BreathHitBox : MonoBehaviour
{
    private Collider2D checkEnemy;
    [SerializeField] private float inBreathSpeed = 10.0f;
    private Transform playerTransform;
    
    private void Awake()
    {
        if (Player.instancePlayer != null)
        {
            playerTransform = Player.instancePlayer.transform;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (playerTransform == null)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            StartCoroutine(BreathInEnemy(other.transform));
        }
    }

    private IEnumerator BreathInEnemy(Transform enemy)
    {
        if (enemy == null || playerTransform == null)
        {
            yield break;
        }

        Vector3 startPos = enemy.position;
        Vector3 targetPos = playerTransform.position;
        float loadingTime = 0.0f;

        while (loadingTime < inBreathSpeed)
        {
            loadingTime += Time.deltaTime;
            float runtime = loadingTime / inBreathSpeed;

            enemy.position = Vector3.Lerp(startPos, targetPos, runtime *  inBreathSpeed);
            
            yield return null;
        }
        if (enemy != null && playerTransform != null)
        {
            enemy.position = playerTransform.position;
        }
    }

}
