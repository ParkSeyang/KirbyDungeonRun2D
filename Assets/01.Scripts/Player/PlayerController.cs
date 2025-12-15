using UnityEngine;

// PlayerController = 위 4개를 Update에서 호출 및 제어하는 시스템(오케스트레이터)
public class PlayerController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAbilityController playerAbilityController;
    [SerializeField] private PlayerAnimationHandler animationHandler;

    [Header("Visual")]
    [SerializeField] private GameObject baseBodyRoot;

    public static PlayerController localPlayer;

    private void Awake()
    {
        // ===== 예외/가드(규칙: 항상 맨 위) =====
        if (localPlayer == null)
        {
            localPlayer = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerAbilityController == null)
        {
            playerAbilityController = GetComponent<PlayerAbilityController>();
        }

        if (animationHandler == null)
        {
            animationHandler = GetComponent<PlayerAnimationHandler>();
        }
    }

    private void Update()
    {
        MovementUpdate();
        AbilityUpdate();
    }

    private void MovementUpdate()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.MovementUpdate();
        playerMovement.JumpUpdate();
    }

    private void AbilityUpdate()
    {
        if (playerAbilityController == null)
        {
            return;
        }

        Player playerStat = Player.instance;
        BaseStat.AbilityType abilityType = BaseStat.AbilityType.Normal;

        if (playerStat != null)
        {
            abilityType = playerStat.PlayerAbility;
        }

        switch (abilityType)
        {
            case BaseStat.AbilityType.Normal:
                playerAbilityController.NormalAction();
                break;

            case BaseStat.AbilityType.Sword:
                playerAbilityController.SwordAction();
                break;

            case BaseStat.AbilityType.Cutter:
                playerAbilityController.CutterAction();
                break;

            case BaseStat.AbilityType.Bomber:
                break;

            default:
                playerAbilityController.NormalAction();
                break;
        }
    }

    public void StopBreath()
    {
        if (playerAbilityController == null)
        {
            return;
        }

        playerAbilityController.StopBreath();
    }

    public void SetAbilityAnimator(Animator newAnimator)
    {
        if (animationHandler == null)
        {
            animationHandler = GetComponent<PlayerAnimationHandler>();
        }

        if (animationHandler == null)
        {
            return;
        }

        animationHandler.SetAbilityAnimator(newAnimator);
    }

    // ✅ 추가: 능력 프리팹 루트를 애니 핸들러에 등록(Flip 대상)
    public void SetAbilityVisualRoot(Transform newRoot)
    {
        if (animationHandler == null)
        {
            animationHandler = GetComponent<PlayerAnimationHandler>();
        }

        if (animationHandler == null)
        {
            return;
        }

        animationHandler.SetAbilityVisualRoot(newRoot);
    }

    public void ShowBaseBody(bool show)
    {
        if (baseBodyRoot == null)
        {
            return;
        }

        baseBodyRoot.SetActive(show);
    }
}
