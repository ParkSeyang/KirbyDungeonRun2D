using System;
using UnityEngine;

namespace EnemySystem.Data
{
    [Serializable]
    public class EnemyPrefabEntry
    {
        [Header("Common")]
        public EnemyCommonProfile Common;

        [Header("Prefab")]
        public GameObject EnemyPrefab;

        [Header("Stat Template (값복사 원본)")]
        public EnemyStatTemplate StatTemplate;

        [Header("AI Profile")]
        public EnemyAIProfile AIProfile;

        [Header("Attack Profile")]
        public EnemyAttackProfile AttackProfile;

        [Header("Effect Set (Prefab 모음)")]
        public EnemyEffectSet EffectSet;
    }

    [CreateAssetMenu(fileName = "EnemyPrefabsData", menuName = "Game/Enemy/EnemyPrefabsData")]
    public class EnemyPrefabsData : ScriptableObject
    {
        [Header("Enemy Database")]
        [SerializeField] public EnemyPrefabEntry[] enemyDataes;

        public bool TryGet(EnemyKey key, out EnemyPrefabEntry entry)
        {
            entry = null;

            if (enemyDataes == null)
            {
                return false;
            }

            if (enemyDataes.Length <= 0)
            {
                return false;
            }

            for (int i = 0; i < enemyDataes.Length; i++)
            {
                EnemyPrefabEntry e = enemyDataes[i];

                if (e == null)
                {
                    continue;
                }

                if (e.Common.Key == key)
                {
                    entry = e;
                    return true;
                }
            }

            return false;
        }
    }
}