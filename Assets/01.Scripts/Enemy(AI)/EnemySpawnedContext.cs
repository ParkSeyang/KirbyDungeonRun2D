using UnityEngine;

namespace EnemySystem.Runtime
{
    [DisallowMultipleComponent]
    public class EnemySpawnedContext : MonoBehaviour
    {
        [Header("Read Only (Injected at Spawn)")]
        [SerializeField] private bool isInitialized = false;

        [SerializeField] private EnemySystem.Data.EnemyCommonProfile common;
        [SerializeField] private EnemySystem.Data.EnemyRuntimeStat runtimeStat;
        [SerializeField] private EnemySystem.Data.EnemyAIProfile aiProfile;
        [SerializeField] private EnemySystem.Data.EnemyAttackProfile attackProfile;
        [SerializeField] private EnemySystem.Data.EnemyEffectSet effectSet;

        public bool IsInitialized { get { return isInitialized; } }
        public EnemySystem.Data.EnemyCommonProfile Common { get { return common; } }
        public EnemySystem.Data.EnemyAIProfile AIProfile { get { return aiProfile; } }
        public EnemySystem.Data.EnemyAttackProfile AttackProfile { get { return attackProfile; } }
        public EnemySystem.Data.EnemyEffectSet EffectSet { get { return effectSet; } }

        public EnemySystem.Data.EnemyRuntimeStat RuntimeStat
        {
            get { return runtimeStat; }
        }

        public BaseStat.AbilityType EnemyAbilityType
        {
            get
            {
                if (isInitialized == false)
                {
                    return BaseStat.AbilityType.Normal;
                }

                return runtimeStat.Template.AbilityType;
            }
        }

        public int CurrentHp { get { return runtimeStat.Hp; } }
        public int MaxHp { get { return runtimeStat.Template.MaxHp; } }
        public int Attack { get { return runtimeStat.Template.Attack; } }

        public void InitializeFromEntry(EnemySystem.Data.EnemyPrefabEntry entry)
        {
            // ===== 예외/가드 =====
            if (entry == null)
            {
                return;
            }

            common = entry.Common;

            aiProfile = entry.AIProfile;
            aiProfile.Sanitize();

            attackProfile = entry.AttackProfile;
            attackProfile.Sanitize();

            effectSet = entry.EffectSet;
            effectSet.Sanitize();

            runtimeStat = entry.StatTemplate.CreateRuntime();

            isInitialized = true;

            ApplyRuntimeToEnemyComponent();
        }

        // 기존 호출 호환용 (source/position 정보 없을 때)
        public bool ApplyDamage(int damage)
        {
            return ApplyDamage(null, damage, transform.position);
        }

        // ✅ CombatSystem 연동용: source + hitPosition 포함
        public bool ApplyDamage(GameObject source, int damage, Vector3 hitPosition)
        {
            // ===== 예외/가드 =====
            if (isInitialized == false)
            {
                return false;
            }

            if (damage <= 0)
            {
                return false;
            }

            bool isDead = runtimeStat.ApplyDamage(damage);

            // 값복사라서 동기화 필요
            ApplyRuntimeToEnemyComponent();

            // ✅ DamageEvent 발행
            PublishDamageEvent(source, damage, hitPosition);

            // ✅ DeathEvent 발행
            if (isDead == true)
            {
                PublishDeathEvent(source, damage, hitPosition);
            }

            return isDead;
        }

        public void ResetRuntime()
        {
            // ===== 예외/가드 =====
            if (isInitialized == false)
            {
                return;
            }

            runtimeStat.ResetRuntime();
            ApplyRuntimeToEnemyComponent();
        }

        private void ApplyRuntimeToEnemyComponent()
        {
            // ===== 예외/가드 =====
            if (isInitialized == false)
            {
                return;
            }

            global::Enemy enemy = GetComponent<global::Enemy>();

            if (enemy == null)
            {
                return;
            }

            enemy.ApplyRuntimeStat(runtimeStat);
        }

        private void PublishDamageEvent(GameObject source, int damage, Vector3 hitPosition)
        {
            // ===== 예외/가드 =====
            if (ComBatSystem.Instance == null)
            {
                return;
            }

            ComBatSystem.Instance.PublishDamage(source, gameObject, damage, hitPosition);
        }

        private void PublishDeathEvent(GameObject source, int lastDamage, Vector3 position)
        {
            // ===== 예외/가드 =====
            if (ComBatSystem.Instance == null)
            {
                return;
            }

            ComBatSystem.Instance.PublishDeath(source, gameObject, lastDamage, position);
        }
    }
}
