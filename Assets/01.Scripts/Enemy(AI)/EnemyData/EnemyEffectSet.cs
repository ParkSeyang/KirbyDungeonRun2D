using System;
using UnityEngine;

namespace EnemySystem.Data
{
    [Serializable]
    public struct EnemyEffectSet
    {
        [Header("Death Effect")]
        public GameObject DeadEffectPrefab;
        public Vector3 DeadEffectOffset;

        [Header("Projectile (Ranged)")]
        public GameObject ProjectilePrefab;
        public Vector3 ProjectileSpawnOffset;

        [Header("Hit Effect Shader Settings (Optional)")]
        public float HitEffectDuration;
        public float HitEffectBlendValue;

        public void Sanitize()
        {
            if (HitEffectDuration < 0.0f)
            {
                HitEffectDuration = 0.0f;
            }

            if (HitEffectBlendValue < 0.0f)
            {
                HitEffectBlendValue = 0.0f;
            }
        }
    }
}