using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVfxSystem : MonoBehaviour
{
    [SerializeField] private Transform effectRoot;
    [SerializeField] private bool useEventPosition = true;

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

        PlayerVfxProfile profile = combatEvent.Target.GetComponent<PlayerVfxProfile>();

        if (profile == null)
        {
            return; // Player만 처리
        }

        if (profile.DeadEffectPrefab == null)
        {
            return;
        }

        Vector3 basePosition = (useEventPosition == true) ? combatEvent.Position : combatEvent.Target.transform.position;
        Vector3 spawnPosition = basePosition + profile.DeadEffectOffset;

        if (effectRoot != null)
        {
            Instantiate(profile.DeadEffectPrefab, spawnPosition, Quaternion.identity, effectRoot);
            return;
        }

        Instantiate(profile.DeadEffectPrefab, spawnPosition, Quaternion.identity);
    }
}