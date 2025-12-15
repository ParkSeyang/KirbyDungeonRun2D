using System;
using UnityEngine;

namespace EnemySystem.Data
{
    [Serializable]
    public struct EnemyStatTemplate
    {
        [Header("Identity")]
        public string Name;
        public BaseStat.AbilityType AbilityType;

        [Header("Combat")]
        public int MaxHp;
        public int Attack;

        [Header("AI (optional)")]
        public float MoveSpeed;
        public float TraceRange;
        public float AttackRange;
        public float AttackCooldown;

        public void SanitizeTemplate()
        {
            if (MaxHp < 1) { MaxHp = 1; }
            if (Attack < 0) { Attack = 0; }

            if (MoveSpeed < 0.0f) { MoveSpeed = 0.0f; }
            if (TraceRange < 0.0f) { TraceRange = 0.0f; }
            if (AttackRange < 0.0f) { AttackRange = 0.0f; }
            if (AttackCooldown < 0.0f) { AttackCooldown = 0.0f; }
        }

        public EnemyRuntimeStat CreateRuntime()
        {
            EnemyStatTemplate t = this;
            t.SanitizeTemplate();

            EnemyRuntimeStat runtime = new EnemyRuntimeStat(t);
            return runtime;
        }
    }

    [Serializable]
    public struct EnemyRuntimeStat
    {
        public EnemyStatTemplate Template;
        public int Hp;

        public EnemyRuntimeStat(EnemyStatTemplate template)
        {
            Template = template;
            Hp = template.MaxHp;
        }

        public void ResetRuntime()
        {
            if (Template.MaxHp < 1)
            {
                Template.MaxHp = 1;
            }

            Hp = Template.MaxHp;
        }

        public bool ApplyDamage(int damage)
        {
            if (damage <= 0)
            {
                return false;
            }

            Hp -= damage;

            if (Hp <= 0)
            {
                Hp = 0;
                return true;
            }

            return false;
        }
    }
}