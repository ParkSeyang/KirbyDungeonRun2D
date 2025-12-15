using UnityEngine;

public enum EventType
{
    DamageEvent,
    HealEvent,
    DeathEvent,
}

public struct CombatEvent
{
    public EventType Type;
    public int Amount;
    public Vector3 Position;

    public GameObject Source;
    public GameObject Target;
}