using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;


public class PlayerAbilitySystem : MonoBehaviour
{
   public static PlayerAbilitySystem instance;
   public PlayerAbilitySystem[] Abilities;
   
   public void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      else
      {
         Destroy(gameObject);
      }
      
   }

   public void AbilityType()
   {
 
   }

   public void SearchAbility()
   {
      
   }
   
   public void OnCollisionEnter2D(Collision2D collision)
   {
      Enemy enemy;
   }
   

  
}
