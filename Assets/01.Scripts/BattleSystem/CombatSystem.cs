using System;
using UnityEngine;

public class ComBatSystem : MonoBehaviour
{
    public static ComBatSystem Instance { get; private set; }

    public event Action<CombatEvent> OnEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Publish(CombatEvent combatEvent)
    {
        if (OnEvent == null)
        {
            return;
        }

        OnEvent(combatEvent);
    }

    public void PublishDamage(GameObject source, GameObject target, int amount, Vector3 position)
    {
        if (target == null)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        Publish(new CombatEvent
        {
            Type = EventType.DamageEvent,
            Source = source,
            Target = target,
            Amount = amount,
            Position = position,
        });
    }

    public void PublishDeath(GameObject source, GameObject target, int lastDamage, Vector3 position)
    {
        if (target == null)
        {
            return;
        }

        Publish(new CombatEvent
        {
            Type = EventType.DeathEvent,
            Source = source,
            Target = target,
            Amount = lastDamage,
            Position = position,
        });
    }
}