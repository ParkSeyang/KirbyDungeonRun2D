using UnityEngine;
using EnemySystem.Runtime;

[DisallowMultipleComponent]
public class EnemyVfxSystem : MonoBehaviour
{
    [SerializeField] private Transform effectRoot;
    [SerializeField] private bool useEventPosition = true;
    [SerializeField] private bool flipOffsetByScaleX = true;

    private bool isBound = false;

    private void OnEnable()
    {
        Bind();
    }

    private void OnDisable()
    {
        Unbind();
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

        EnemySpawnedContext spawnedContext = combatEvent.Target.GetComponent<EnemySpawnedContext>();

        if (spawnedContext == null)
        {
            return; // Enemy만 처리
        }

        EnemySystem.Data.EnemyEffectSet effectSet = spawnedContext.EffectSet;

        if (effectSet.DeadEffectPrefab == null)
        {
            return;
        }

        Vector3 basePosition = (useEventPosition == true) ? combatEvent.Position : combatEvent.Target.transform.position;

        Vector3 offset = effectSet.DeadEffectOffset;

        if (flipOffsetByScaleX == true)
        {
            float sx = combatEvent.Target.transform.localScale.x;
            int sign = (sx >= 0.0f) ? 1 : -1;
            offset.x = offset.x * sign;
        }

        Vector3 spawnPosition = basePosition + offset;

        if (effectRoot != null)
        {
            Instantiate(effectSet.DeadEffectPrefab, spawnPosition, Quaternion.identity, effectRoot);
            return;
        }

        Instantiate(effectSet.DeadEffectPrefab, spawnPosition, Quaternion.identity);
    }
}
