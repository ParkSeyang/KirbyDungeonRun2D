using System;
using UnityEngine;
using UnityEngine.Serialization;

// 전투 관련 기능 및 이동 관련 기능 그리고 애니메이션 추가
public class PlayerController : MonoBehaviour
{   
    // 공통으로 쓰는 파라미터
    private static readonly int Movement = Animator.StringToHash("Movement");
    private static readonly int IsRun = Animator.StringToHash("IsRun");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Falling = Animator.StringToHash("Falling");
    private static readonly int Rolling = Animator.StringToHash("Rolling");
    private static readonly int Flying = Animator.StringToHash("Flying");
    private static readonly int FlyReady = Animator.StringToHash("FlyReady");
    private static readonly int IsGround = Animator.StringToHash("IsGround");
    private static readonly int IsHurt = Animator.StringToHash("IsHurt");
    
    // 기본커비가 쓰는 파라미터
    private static readonly int IsBreath = Animator.StringToHash("IsBreath");
    private static readonly int BreathTime = Animator.StringToHash("BreathTime");
    private static readonly int GetEnemy = Animator.StringToHash("GetEnemy");
    private static readonly int Eating = Animator.StringToHash("Eating");
    private static readonly int FatMode = Animator.StringToHash("FatMode");
    private static readonly int Shooting = Animator.StringToHash("Shooting");
    
    // 능력 커비들이 사용하는 파라미터
    private static readonly int IsAttack = Animator.StringToHash("IsAttack");
    
    [Header("PlayerAnimationSetting")] 
    // 기본 커비 애니메이션
    [SerializeField] private Animator animator;
    // 기본커비 스프라이트
    [SerializeField] private SpriteRenderer KirbySprite; 
    // 기본 Kirby 바디 백업용(Normal 비주얼)
    [SerializeField] private GameObject baseBodyRoot;
    
    [Header("능력 Kriby 애니메이션")]
    [SerializeField] private Animator abilityAnimator;

    [Header("ActionSetting")] 
    [SerializeField] private GameObject breatheCheckBox;
    [SerializeField] public bool isBreathe = false;
    [SerializeField] private float timeChecker = 0.0f;
    [SerializeField] private ActionState currentActionState = ActionState.None;
    
    public enum ActionState
    {
        None,
        Attack,
        Breath,
        Eat,
    }

    // 점프 애니메이션 상태 추적용 (실제 점프 물리는 PlayerMovement가 담당)
    private PlayerJumpState jumpState = PlayerJumpState.Ground;
    
    public static PlayerController localPlayer;

    // ★ 새로 추가: 이동/점프 전담 컴포넌트
    private PlayerMovement playerMovement;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        KirbySprite = GetComponentInChildren<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();

        if (breatheCheckBox != null)
        {
            breatheCheckBox.SetActive(false);
        }

        if (localPlayer == null)
        {
            localPlayer = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        MovementUpdate();
        JumpUpdate();
        ActionStateUpdate();
    }

    void MovementUpdate()
    {
        if (playerMovement == null)
        {
            return;
        }

        // 숨 들이마시는 동안에는 이동/점프 잠금 + 애니메이션 정지
        if (isBreathe == true)
        {
            SetAnimFloat(Movement, 0.0f);
            SetAnimBool(IsRun, false);

            playerMovement.CanMove = false;
            playerMovement.CanJump = false;
            return;
        }

        // 숨을 안 들이마시면 다시 이동 / 점프 허용
        playerMovement.CanMove = true;
        playerMovement.CanJump = true;

        float currentX = playerMovement.CurrentMoveX;
        Vector2 inputMoveXY = new Vector2(currentX, 0.0f);
        float absInputXY = Mathf.Abs(inputMoveXY.magnitude);
        Vector2 inputX = new Vector2(currentX, 0.0f);
        
        Player playerStat = Player.instance;
        BaseStat.AbilityType abilityType = BaseStat.AbilityType.Normal;
        
        if (playerStat != null)
        {
            abilityType = playerStat.PlayerAbility;
        }

        switch (abilityType)
        {
            case BaseStat.AbilityType.Normal:
                HandleNormalMovement(inputMoveXY, inputX, absInputXY);
                break;
            case BaseStat.AbilityType.Sword:
                HandleSwordMovement(inputMoveXY, inputX, absInputXY);
                break;
            case BaseStat.AbilityType.Cutter:
                HandleCutterMovement(inputMoveXY, inputX, absInputXY);
                break;
            case BaseStat.AbilityType.Bomber:
                HandleBomberMovement(inputMoveXY, inputX, absInputXY);
                break;
            default:
                break;
        }

        // 달리기 애니메이션 (물리는 PlayerMovement.IsRun 사용)
        SetAnimBool(IsRun, playerMovement.IsRun);
        // 지면 애니메이션 (물리는 PlayerMovement.IsGrounded 사용)
        SetAnimBool(IsGround, playerMovement.IsGrounded);
    }

    void HandleNormalMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY)
    {
        // 애니메이션 파라미터
        SetAnimFloat(Movement, absInputXY);
        // 좌우 반전
        FlipByInput(inputMoveXY, absInputXY);
        // 실제 이동/달리기 물리는 PlayerMovement가 담당
    }

    void HandleSwordMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY)
    {
        SetAnimFloat(Movement, absInputXY);
        FlipByInput(inputMoveXY, absInputXY);
    }

    void HandleCutterMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY)
    {
        SetAnimFloat(Movement, absInputXY);
        FlipByInput(inputMoveXY, absInputXY);
    }

    void HandleBomberMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY)
    {
        SetAnimFloat(Movement, absInputXY);
        FlipByInput(inputMoveXY, absInputXY);
    }

    void SetAnimFloat(int hash, float value)
    {
        if (animator != null)
        {
            animator.SetFloat(hash, value);
        }

        if (abilityAnimator != null)
        {
            abilityAnimator.SetFloat(hash, value);
        }
    }

    void SetAnimBool(int hash, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(hash, value);
        }

        if (abilityAnimator != null)
        {
            abilityAnimator.SetBool(hash, value);
        }
    }

    void SetAnimTrigger(int hash)
    {
        if (animator != null)
        {
            animator.SetTrigger(hash);
        }

        if (abilityAnimator != null)
        {
            abilityAnimator.SetTrigger(hash);
        }
    }

    void FlipByInput(Vector2 inputMoveXY, float absInputXY)
    {
        // absInputXY가 0보다 높다면 반전시키지말것
        if (absInputXY <= 0.0f)
        {
            return;
        }
        // absInputXY가 0보다 높다면 반전시킨다.
        if (absInputXY > 0.0f) 
        {
            animator.transform.localScale = (inputMoveXY.x < 0.0f) 
                ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1); 
        }
    }

    void JumpUpdate()
    {
        if (playerMovement == null)
        {
            return;
        }

        PlayerJumpState currentJumpState = playerMovement.CurrentJumpState;

        // Ground → Jumping : 점프 시작
        if (jumpState == PlayerJumpState.Ground &&
            currentJumpState == PlayerJumpState.Jumping)
        {
            SetAnimTrigger(Jump);
        }

        // Jumping → Falling : 점프 정점 이후 구르기/떨어지기
        if (jumpState == PlayerJumpState.Jumping &&
            currentJumpState == PlayerJumpState.Falling)
        {
            SetAnimTrigger(Rolling);
        }

        // Ground → Falling : 계단 끝에서 바로 떨어질 때
        if (jumpState == PlayerJumpState.Ground &&
            currentJumpState == PlayerJumpState.Falling)
        {
            SetAnimTrigger(Falling);
        }

        // 이번 프레임 상태 저장 (ActionStateUpdate에서 사용)
        jumpState = currentJumpState;
    }
    
    void ActionStateUpdate()
    {
        // 점프 중에는 액션 막기 (기존 로직 그대로)
        if (jumpState == PlayerJumpState.Jumping || jumpState == PlayerJumpState.Falling)
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
                NormalAction();
                break;
            case BaseStat.AbilityType.Sword:
                SwordAction();
                break;
            case BaseStat.AbilityType.Cutter:
                CutterAction();
                break;
            case BaseStat.AbilityType.Bomber:
                // 나중에 BomberAction 추가
                break;
            default:
                break;
        }
    }

    public void NormalAction()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isBreathe = true;
            currentActionState = ActionState.Breath;
            SetAnimBool(IsBreath, true);

            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(true);
            }
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            isBreathe = false;
            currentActionState = ActionState.None;
            
            SetAnimBool(IsBreath, false);
            
            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(false);
            }
        }

        // 먹기 기능은 나중에 추가
    }

    public void SwordAction()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isBreathe = true;
            currentActionState = ActionState.None;
            SetAnimBool(IsBreath, false);

            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(false);
            }

            if (PlayerAbilitySystem.instance != null)
            {
               // PlayerAbilitySystem.instance.DropAbility();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            // 공격 기능 추가할것.
            if (abilityAnimator != null)
            {
                abilityAnimator.SetBool(IsAttack, true);
            }
        }

        if (abilityAnimator != null)
        {
            abilityAnimator.SetBool(IsAttack, false);
        }
    }

    public void CutterAction()
    {
        // Z : 능력 버리기
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isBreathe = false;
            currentActionState = ActionState.None;
            SetAnimBool(IsBreath, false);

            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(false);
            }

            if (PlayerAbilitySystem.instance != null)
            {
               // PlayerAbilitySystem.instance.DropAbility();
            }
        }

        // X : 나중에 Cutter 공격 넣을 자리
        if (Input.GetKeyDown(KeyCode.X))
        {
            // TODO: Cutter 공격 로직 (커터 날리기 등)
        }
    }

    // 추천했던 선택 사항
    public void StopBreath()
    {
        isBreathe = false;
        SetAnimBool(IsBreath, false);

        if (breatheCheckBox != null)
        {
            breatheCheckBox.SetActive(false);
        }
    }

    // 능력 Kirby 프리팹의 Animator를 등록하는 함수(매개변수 애니메이터)
    public void SetAbilityAnimator(Animator newAnimator)
    {
        abilityAnimator = newAnimator;
    }

    // 기본 Kirby의 바디(Character)를 보이게 or 숨기게 할수 있는기능
    public void ShowBaseBody(bool show)
    {
        if (baseBodyRoot == null)
        {
            return;
        }
        
        baseBodyRoot.SetActive(show);
    }
}
