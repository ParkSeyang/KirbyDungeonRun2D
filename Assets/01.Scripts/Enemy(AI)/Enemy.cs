using System;
using UnityEngine;

public class Enemy : EnemyStat
{
   
   private static readonly int IsDamege = Animator.StringToHash("IsDamege");
   [SerializeField] public AbilityType enemyType;
   
   private Animator animator;
   
   private void Awake()
   {
      
      animator = GetComponentInChildren<Animator>();
      EnemyType();
      
   }
   
   void EnemyType()
   {
      switch (enemyType)
      {
         case AbilityType.Normal:
            Name = "normal";
            Hp = 10.0f;
            Attack = 5.0f;
            break;
         case AbilityType.Cutter:
            Name = "cutter";
            Hp = 20.0f;
            Attack = 10.0f;
            break;
         case AbilityType.Sword:
            Name = "sword";
            Hp = 20.0f;
            Attack = 15.0f;
            break;
         case AbilityType.Bomber:
            Name = "bomber";
            Hp = 15.0f;
            Attack = 30.0f;
            break;
      }
   }
   
   
}
