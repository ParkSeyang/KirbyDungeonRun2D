using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySystem.Runtime;

[DisallowMultipleComponent]
public class EnemyDeathSystem : MonoBehaviour
{
    [Header("Destroy/Deactivate")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private bool deactivateOnDeath = false;
    [SerializeField] private float delay = 0.0f;

    [Header("Disable on Death")]
    [SerializeField] private bool disableColliders = true;
    [SerializeField] private bool stopRigidbody2D = true;
    [SerializeField] private bool disableMonoBehaviours = true;

    private readonly HashSet<int> handledTargetIds = new HashSet<int>();
    private bool isBound = false;

    private void OnEnable()
    {
        Bind();
    }

    private void Start()
    {
        Bind();
    }

    private void OnDisable()
    {
        Unbind();
        handledTargetIds.Clear();
    }

    private void Bind()
    {
        if (isBound == true)
        {
            return;
        }

        if (ComBatSystem.Instance == null)
        {
            return;
        }

        ComBatSystem.Instance.OnEvent += OnCombatEvent;
        isBound = true;
    }

    private void Unbind()
    {
        if (isBound == false)
        {
            return;
        }

        if (ComBatSystem.Instance != null)
        {
            ComBatSystem.Instance.OnEvent -= OnCombatEvent;
        }

        isBound = false;
    }

    private void OnCombatEvent(CombatEvent combatEvent)
    {
        if (combatEvent.Type != EventType.DeathEvent)
        {
            return;
        }

        if (combatEvent.Target == null)
        {
            return;
        }

        EnemySpawnedContext ctx = combatEvent.Target.GetComponent<EnemySpawnedContext>();

        if (ctx == null)
        {
            return; // Enemy만 처리
        }

        int id = combatEvent.Target.GetInstanceID();

        if (handledTargetIds.Contains(id) == true)
        {
            return;
        }

        handledTargetIds.Add(id);

        HandleEnemyDeath(combatEvent.Target);

        StartCoroutine(FinalizeAfterDelay(combatEvent.Target, id));
    }

    private void HandleEnemyDeath(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (disableColliders == true)
        {
            Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>(true);

            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        colliders[i].enabled = false;
                    }
                }
            }
        }

        if (stopRigidbody2D == true)
        {
            Rigidbody2D[] bodies = target.GetComponentsInChildren<Rigidbody2D>(true);

            if (bodies != null)
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    Rigidbody2D rb = bodies[i];

                    if (rb == null)
                    {
                        continue;
                    }

                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0.0f;
                    rb.simulated = false;
                }
            }
        }

        if (disableMonoBehaviours == true)
        {
            MonoBehaviour[] behaviours = target.GetComponentsInChildren<MonoBehaviour>(true);

            if (behaviours != null)
            {
                for (int i = 0; i < behaviours.Length; i++)
                {
                    MonoBehaviour mb = behaviours[i];

                    if (mb == null)
                    {
                        continue;
                    }

                    // 죽음 파이프라인 핵심은 남겨둠
                    if (mb is EnemySpawnedContext)
                    {
                        continue;
                    }

                    mb.enabled = false;
                }
            }
        }

        target.SendMessage("OnEnemyDeath", SendMessageOptions.DontRequireReceiver);
    }

    private IEnumerator FinalizeAfterDelay(GameObject target, int id)
    {
        if (delay > 0.0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (deactivateOnDeath == true)
        {
            if (target != null)
            {
                target.SetActive(false);
            }
        }

        if (destroyOnDeath == true)
        {
            if (target != null)
            {
                Destroy(target);
            }
        }

        handledTargetIds.Remove(id);
    }
}
