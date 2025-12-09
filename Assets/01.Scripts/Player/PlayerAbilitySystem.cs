using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class PlayerAbilitySystem : MonoBehaviour
{
   public static PlayerAbilitySystem instance;

   [Header("커비의 능력 프리팹 목록")] 
   [SerializeField] public AbilityPrefabData[] abilityDataes;
   
   [Header("커비 능력 프리팹을 지정할 부모")] 
   [SerializeField] private Transform abilityParent;
   
   [Header("플레이어 컨트롤러 (흡입 종료 등에 사용)")]
   public PlayerController playerController;

   // 현재 장착중인 능력 오브젝트
   [SerializeField] private GameObject currentAbilityObject;
   
   public void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      else
      {
         Destroy(gameObject);
         return;
      }

      if (abilityParent == null)
      {
         abilityParent = transform;
      }

   }

   public void EatEnemy(Enemy enemy)
   {
      if (enemy == null)
      {
         return;
      }

      BaseStat.AbilityType enemyAbility = enemy.enemyType;
      Debug.Log($"[PlayerAbilitySystem] EatEnemy called, enemyAbility = {enemyAbility}");
      Player playerStat = Player.instance;

      if (playerStat == null)
      {
         Debug.LogWarning("[PlayerAbilitySystem] Player.instancePlayer is null");
         return;
      }
      // 일반 적이면 능력 복사 없이 먹고 삭제
      if (enemyAbility == BaseStat.AbilityType.Normal)
      {
         Debug.Log("[PlayerAbilitySystem] Normal enemy, just destroy.");
         Destroy(enemy.gameObject);
         return;
      }
      // 1) 플레이어의 능력 타입 변경
      playerStat.PlayerAbility = enemyAbility;
      // 2) hp,랑 Life는 유지한채로 능력에 따라 Attack만 변경 
      playerStat.StatSetting();
      // Kirby 능력 프리팹 장착
      Debug.Log($"[PlayerAbilitySystem] PlayerAbility set to {playerStat.PlayerAbility}, Attack = {playerStat.Attack}");
      
      EquipAbilityPrefab(enemyAbility);
      // 먹은적 제거
      Destroy(enemy.gameObject);
      // 예외사항 먹자마자 흡입종료.
      if (playerController != null)
      {
         playerController.StopBreath();
      }

   }

   private void EquipAbilityPrefab(BaseStat.AbilityType abilityType)
   {
      Debug.Log($"[PlayerAbilitySystem] EquipAbilityPrefab called, abilityType = {abilityType}");
      if (currentAbilityObject != null)
      {
         Debug.Log("[PlayerAbilitySystem] Destroy current ability object: " + currentAbilityObject.name);
         Destroy(currentAbilityObject);
         currentAbilityObject = null;
      }

      if (abilityDataes == null || abilityDataes.Length == 0)
      {
         Debug.LogWarning("[PlayerAbilitySystem] abilityPrefabs is empty!");
         return;
      }

      GameObject prefabToSpawn = null;

      for (int i = 0; i < abilityDataes.Length; i++)
      {
         if (abilityDataes[i].abilityType == abilityType)
         {
            prefabToSpawn = abilityDataes[i].abilityPrefab;
            break;
         }
      }

      if (prefabToSpawn == null)
      {
         Debug.LogWarning("[PlayerAbilitySystem] No prefabToSpawn found for abilityType = " + abilityType);
         return;
      }

      Transform parent = abilityParent != null ? abilityParent : transform;
      Debug.Log($"[PlayerAbilitySystem] Instantiate prefab {prefabToSpawn.name} under parent {parent.name}");
      currentAbilityObject = GameObject.Instantiate(prefabToSpawn, parent);
      currentAbilityObject.transform.localPosition = Vector3.zero;

      Animator abilityAnim = currentAbilityObject.GetComponentInChildren<Animator>();

      if (playerController == null)
      {
         Debug.LogWarning("[PlayerAbilitySystem] playerController is null");
         return;
      }

      if (abilityAnim == null)
      {
         Debug.LogWarning("[PlayerAbilitySystem] Spawned ability prefab has no Animator");
         playerController.SetAbilityAnimator(null);
         playerController.ShowBaseBody(true);
      }
      else
      {
         playerController.SetAbilityAnimator(abilityAnim);
         
         playerController.ShowBaseBody(false);
         
         Debug.Log($"[PlayerAbilitySystem] Instantiate prefab {prefabToSpawn.name} under parent {parent.name}");
      }
      // 필요하면 여기서 ability 프리팹의 Animator를 PlayerController에 넘겨줄 수 있음
      // Animator abilityAnim = currentAbilityObject.GetComponentInChildren<Animator>();
      // if (playerController != null && abilityAnim != null)
      // {
      //     playerController.SetAbilityAnimator(abilityAnim);
      // }



   }
   
}
