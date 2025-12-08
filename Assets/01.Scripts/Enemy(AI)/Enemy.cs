using System;
using UnityEngine;

public class Enemy : EnemyStat
{
   public static Enemy instanceEnemy;
   private static readonly int IsDamege = Animator.StringToHash("IsDamege");
   [SerializeField] public AbilityType enemyType;
   
   private Animator animator;
   
   private void Awake()
   {
      
   }

   private void Update()
   {
      EnemyType();
      
   }
   
   void EnemyType()
   {
      switch (enemyType)
      {
         case AbilityType.Normal:
            name = "normal";
            Hp = 10;
            break;
         case AbilityType.Cutter:
            name = "cutter";
            Hp = 20;
            Attack = 10;
            break;
         case AbilityType.Sword:
            name = "sword";
            Hp = 20;
            Attack = 15;
            break;
         case AbilityType.Bomber:
            name = "bomber";
            Hp = 15;
            Attack = 30;
            break;
      }
   }
   
   
}
