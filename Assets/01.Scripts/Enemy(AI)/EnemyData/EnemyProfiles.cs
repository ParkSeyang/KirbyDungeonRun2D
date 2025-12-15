using System;
using UnityEngine;

namespace EnemySystem.Data
{
    public enum EnemyKey
    {
        NormalEnemy,
        NormalEnemy2,
        NormalEnemy3,
        NormalEnemy4,
        NormalEnemy5,

        CutterEnemy,
        SwordEnemy,
    }

    public enum EnemyFamily
    {
        Normal,
        Ability,
    }

    public enum EnemyAIType
    {
        PatrolOnly,
        PatrolAndChase,
    }

    public enum EnemyAttackType
    {
        None,
        Contact,
        Melee,
        Ranged,
    }

    [Serializable]
    public struct EnemyCommonProfile
    {
        [Header("Key/Type")]
        public EnemyKey Key;
        public EnemyFamily Family;

        [Header("Display")]
        public string DisplayName;

        [Header("Ability Drop / Identify")]
        public BaseStat.AbilityType AbilityType;
    }

    [Serializable]
    public struct EnemyAIProfile
    {
        [Header("AI Type")]
        public EnemyAIType AIType;

        [Header("Move")]
        public float MoveSpeed;

        [Header("Patrol (local offset from spawn)")]
        public Vector2 PatrolLocalOffset;

        [Header("Detect/Chase")]
        public float TraceRange;

        [Header("Update Term (Coroutine/AI Tick)")]
        public float BaseUpdateTerm;

        public void Sanitize()
        {
            if (MoveSpeed < 0.0f) { MoveSpeed = 0.0f; }
            if (TraceRange < 0.0f) { TraceRange = 0.0f; }
            if (BaseUpdateTerm <= 0.0f) { BaseUpdateTerm = 0.1f; }
        }
    }

    [Serializable]
    public struct EnemyAttackProfile
    {
        [Header("Attack Type")]
        public EnemyAttackType AttackType;

        [Header("Common Cooldown")]
        public float AttackCooldown;

        [Header("Contact")]
        public float ContactHitInterval;

        [Header("Melee")]
        public Vector2 MeleeHitBoxOffset;
        public Vector2 MeleeHitBoxSize;
        public float MeleeActiveTime;

        [Header("Ranged")]
        public float RangedAttackRange;
        public float ProjectileSpeed;
        public float ProjectileMaxDistance;

        public void Sanitize()
        {
            if (AttackCooldown < 0.0f) { AttackCooldown = 0.0f; }
            if (ContactHitInterval < 0.0f) { ContactHitInterval = 0.0f; }

            if (MeleeHitBoxSize.x < 0.0f) { MeleeHitBoxSize.x = 0.0f; }
            if (MeleeHitBoxSize.y < 0.0f) { MeleeHitBoxSize.y = 0.0f; }
            if (MeleeActiveTime < 0.0f) { MeleeActiveTime = 0.0f; }

            if (RangedAttackRange < 0.0f) { RangedAttackRange = 0.0f; }
            if (ProjectileSpeed < 0.0f) { ProjectileSpeed = 0.0f; }
            if (ProjectileMaxDistance < 0.0f) { ProjectileMaxDistance = 0.0f; }
        }
    }
}
