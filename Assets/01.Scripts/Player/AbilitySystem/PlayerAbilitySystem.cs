using UnityEngine;

public class PlayerAbilitySystem : MonoBehaviour
{
   public static PlayerAbilitySystem instance;

   [Header("커비의 능력 프리팹 목록")]
   [SerializeField] public AbilityPrefabData[] abilityDataes;

   [Header("커비 능력 프리팹을 지정할 부모")]
   [SerializeField] private Transform abilityParent;

   [Header("플레이어 컨트롤러 (흡입 종료 등에 사용)")]
   public PlayerController playerController;

   [Header("플레이어 능력 컨트롤러 (FatMode 등 액션 상태 제어)")]
   [SerializeField] private PlayerAbilityController playerAbilityController;

   // ✅ 인스펙터에 노출/직렬화하지 않음(런타임 전용)
   private GameObject currentAbilityObject;

   // (선택) 디버깅/코드 접근용 읽기 전용 프로퍼티(인스펙터엔 안 뜸)
   public GameObject CurrentAbilityObject
   {
      get { return currentAbilityObject; }
   }

   public void Awake()
   {
      // ===== 예외/가드 =====
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

      if (playerController == null)
      {
         playerController = GetComponent<PlayerController>();
      }

      if (playerAbilityController == null)
      {
         playerAbilityController = GetComponent<PlayerAbilityController>();
      }
   }

   public void EatEnemy(Enemy enemy)
   {
      // ===== 예외/가드 =====
      if (enemy == null)
      {
         return;
      }

      Player playerStat = Player.instance;

      if (playerStat == null)
      {
         return;
      }

      if (playerAbilityController == null)
      {
         playerAbilityController = GetComponent<PlayerAbilityController>();
      }

      BaseStat.AbilityType enemyAbility = enemy.enemyType;

      if (enemyAbility == BaseStat.AbilityType.Normal)
      {
         if (playerAbilityController != null)
         {
            playerAbilityController.OnEatNormalEnemy();
         }
         else
         {
            if (playerController != null)
            {
               playerController.StopBreath();
            }
         }

         Destroy(enemy.gameObject);
         return;
      }

      playerStat.PlayerAbility = enemyAbility;
      playerStat.StatSetting();

      EquipAbilityPrefab(enemyAbility);

      Destroy(enemy.gameObject);

      if (playerController != null)
      {
         playerController.StopBreath();
      }
   }

   public void DropAbility()
   {
      // ===== 예외/가드 =====
      Player playerStat = Player.instance;

      if (playerStat == null)
      {
         return;
      }

      playerStat.PlayerAbility = BaseStat.AbilityType.Normal;
      playerStat.StatSetting();

      if (currentAbilityObject != null)
      {
         Destroy(currentAbilityObject);
         currentAbilityObject = null;
      }

      if (playerController != null)
      {
         playerController.SetAbilityAnimator(null);
         playerController.ShowBaseBody(true);
         playerController.StopBreath();
      }
   }

   private void EquipAbilityPrefab(BaseStat.AbilityType abilityType)
   {
      // ===== 예외/가드 =====
      if (abilityDataes == null)
      {
         return;
      }

      if (abilityDataes.Length <= 0)
      {
         return;
      }

      if (currentAbilityObject != null)
      {
         Destroy(currentAbilityObject);
         currentAbilityObject = null;
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
         return;
      }

      Transform parent = abilityParent == null ? transform : abilityParent;

      currentAbilityObject = GameObject.Instantiate(prefabToSpawn, parent);
      currentAbilityObject.transform.localPosition = Vector3.zero;

      Animator abilityAnim = currentAbilityObject.GetComponentInChildren<Animator>();

      if (playerController == null)
      {
         return;
      }

      if (abilityAnim == null)
      {
         playerController.SetAbilityAnimator(null);
         playerController.ShowBaseBody(true);
         return;
      }

      playerController.SetAbilityAnimator(abilityAnim);
      playerController.ShowBaseBody(false);
   }
}
