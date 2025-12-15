using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerAnimationHandler animationHandler;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerAbilityController playerAbilityController;
    [SerializeField] private PlayerController playerController;

    [Header("MoveSetting")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float runSpeed = 3.0f;

    [Header("JumpSetting")]
    [SerializeField] private float JumpHeight = 1.0f;
    [SerializeField] private float JumpUpPower = 1.0f;
    [SerializeField] private float downGravity = 3.0f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckerRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private enum PlayerJumpState
    {
        Ground,
        Jumping,
        Falling,
    }

    private PlayerJumpState jumpState = PlayerJumpState.Ground;
    [SerializeField] private bool isGrounded = true;
    private bool prevIsGrounded;
    private Vector3 startPosition;

    public bool IsInAir
    {
        get
        {
            if (jumpState == PlayerJumpState.Jumping)
            {
                return true;
            }

            if (jumpState == PlayerJumpState.Falling)
            {
                return true;
            }

            return false;
        }
    }

    private void Awake()
    {
        if (animationHandler == null)
        {
            animationHandler = GetComponent<PlayerAnimationHandler>();
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (playerAbilityController == null)
        {
            playerAbilityController = GetComponent<PlayerAbilityController>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        // playerInput은 이 스크립트의 필수 의존성이라 생각하고, 여기서만 경고를 남김(폴백 없음)
        if (playerInput == null)
        {
            Debug.LogWarning("[PlayerMovement] PlayerInput reference is missing. Movement/Jump will not run.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundChecker == null)
        {
            return;
        }

        Gizmos.color = Color.lawnGreen;
        Gizmos.DrawWireSphere(groundChecker.position, groundCheckerRadius);
    }

    // PlayerController.Update()에서 호출될 공개 함수들
    public void MovementUpdate()
    {
        // ===== 예외/가드 =====
        if (playerInput == null)
        {
            return;
        }

        bool breatheLock = false;

        if (playerAbilityController != null)
        {
            breatheLock = playerAbilityController.IsBreathe;
        }

        if (breatheLock == true)
        {
            if (animationHandler != null)
            {
                animationHandler.SetAnimFloat(PlayerAnimationHandler.Movement, 0.0f);
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsRun, false);
            }
            return;
        }

        float inputH = playerInput.Horizontal;
        float inputV = playerInput.Vertical;
        bool runHeld = playerInput.RunHeld;

        Vector2 inputMoveXY = new Vector2(inputH, inputV);
        float absInputXY = Mathf.Abs(inputMoveXY.magnitude);

        Vector2 inputX = new Vector2(inputMoveXY.x, 0.0f);

        Player playerStat = Player.instance;
        BaseStat.AbilityType abilityType = BaseStat.AbilityType.Normal;

        if (playerStat == null)
        {
            abilityType = BaseStat.AbilityType.Normal;
        }
        else
        {
            abilityType = playerStat.PlayerAbility;
        }

        switch (abilityType)
        {
            case BaseStat.AbilityType.Normal:
                HandleNormalMovement(inputMoveXY, inputX, absInputXY, runHeld);
                break;
            case BaseStat.AbilityType.Sword:
                HandleSwordMovement(inputMoveXY, inputX, absInputXY, runHeld);
                break;
            case BaseStat.AbilityType.Cutter:
                HandleCutterMovement(inputMoveXY, inputX, absInputXY, runHeld);
                break;
            case BaseStat.AbilityType.Bomber:
                HandleBomberMovement(inputMoveXY, inputX, absInputXY, runHeld);
                break;
        }
    }

    public void JumpUpdate()
    {
        // ===== 예외/가드 =====
        if (playerInput == null)
        {
            return;
        }

        CheckIfGrounded();

        bool groundChanged = false;

        if (prevIsGrounded == true && isGrounded == false)
        {
            groundChanged = true;
        }
        else if (prevIsGrounded == false && isGrounded == true)
        {
            groundChanged = true;
        }

        if (groundChanged == true && jumpState == PlayerJumpState.Ground)
        {
            jumpState = PlayerJumpState.Falling;

            if (animationHandler != null)
            {
                animationHandler.SetAnimTrigger(PlayerAnimationHandler.Falling);
            }
            return;
        }

        Player playerStat = Player.instance;
        BaseStat.AbilityType abilityType = BaseStat.AbilityType.Normal;

        if (playerStat == null)
        {
            abilityType = BaseStat.AbilityType.Normal;
        }
        else
        {
            abilityType = playerStat.PlayerAbility;
        }

        switch (abilityType)
        {
            case BaseStat.AbilityType.Normal:
                HandleNormalJump();
                break;
            case BaseStat.AbilityType.Sword:
                HandleNormalJump();
                break;
            case BaseStat.AbilityType.Cutter:
                HandleNormalJump();
                break;
            case BaseStat.AbilityType.Bomber:
                HandleNormalJump();
                break;
            default:
                HandleNormalJump();
                break;
        }
    }

    // ===== 이동 관련 =====

    private void HandleNormalMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY, bool runHeld)
    {
        SetAnimFloat(PlayerAnimationHandler.Movement, absInputXY);
        FlipByInput(inputMoveXY, absInputXY);

        float selectedSpeed = moveSpeed;

        if (runHeld == true)
        {
            selectedSpeed = runSpeed;
        }

        ApplyMove(inputX, selectedSpeed);
        ApplyRun(inputX, runSpeed, runHeld);
    }

    private void HandleSwordMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY, bool runHeld)
    {
        SetAnimFloat(PlayerAnimationHandler.Movement, absInputXY);
        FlipByInput(inputMoveXY, absInputXY);

        float selectedSpeed = moveSpeed;

        if (runHeld == true)
        {
            selectedSpeed = runSpeed;
        }

        ApplyMove(inputX, selectedSpeed);
        ApplyRun(inputX, runSpeed, runHeld);
    }

    private void HandleCutterMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY, bool runHeld)
    {
        SetAnimFloat(PlayerAnimationHandler.Movement, absInputXY);
        FlipByInput(inputMoveXY, absInputXY);

        float selectedSpeed = moveSpeed;

        if (runHeld == true)
        {
            selectedSpeed = runSpeed;
        }

        ApplyMove(inputX, selectedSpeed);
        ApplyRun(inputX, runSpeed, runHeld);
    }

    private void HandleBomberMovement(Vector2 inputMoveXY, Vector2 inputX, float absInputXY, bool runHeld)
    {
        SetAnimFloat(PlayerAnimationHandler.Movement, absInputXY);
        FlipByInput(inputMoveXY, absInputXY);

        float selectedSpeed = moveSpeed;

        if (runHeld == true)
        {
            selectedSpeed = runSpeed;
        }

        ApplyMove(inputX, selectedSpeed);
        ApplyRun(inputX, runSpeed, runHeld);
    }

    private void SetAnimFloat(int hash, float value)
    {
        // ===== 예외/가드 =====
        if (animationHandler == null)
        {
            return;
        }

        animationHandler.SetAnimFloat(hash, value);
    }

    private void SetAnimBool(int hash, bool value)
    {
        // ===== 예외/가드 =====
        if (animationHandler == null)
        {
            return;
        }

        animationHandler.SetAnimBool(hash, value);
    }

    private void SetAnimTrigger(int hash)
    {
        // ===== 예외/가드 =====
        if (animationHandler == null)
        {
            return;
        }

        animationHandler.SetAnimTrigger(hash);
    }

    private void FlipByInput(Vector2 inputMoveXY, float absInputXY)
    {
        // ===== 예외/가드 =====
        if (animationHandler == null)
        {
            return;
        }

        animationHandler.FlipByInput(inputMoveXY, absInputXY);
    }

    private void ApplyMove(Vector2 inputX, float selectedSpeed)
    {
        if (Mathf.Approximately(inputX.x, 0.0f))
        {
            return;
        }

        Vector2 moveVector = new Vector2(inputX.x, 0.0f);
        transform.Translate(moveVector * selectedSpeed * Time.deltaTime);
    }

    // ✅ 변경 핵심: 이제 ApplyRun은 "이동"을 하지 않고, 애니메이션만 제어
    private void ApplyRun(Vector2 inputX, float runSpeedValue, bool runHeld)
    {
        SetAnimBool(PlayerAnimationHandler.IsRun, runHeld);

        // 이동은 ApplyMove에서 이미 selectedSpeed로 처리됨.
    }

    // ===== 점프 관련 =====

    private void HandleNormalJump()
    {
        // ===== 예외/가드 =====
        if (playerInput == null)
        {
            return;
        }

        bool breatheLock = false;

        if (playerAbilityController != null)
        {
            breatheLock = playerAbilityController.IsBreathe;
        }

        bool jumpHeld = playerInput.JumpHeld;

        switch (jumpState)
        {
            case PlayerJumpState.Ground:
                if (breatheLock == false && jumpHeld == true)
                {
                    jumpState = PlayerJumpState.Jumping;
                    startPosition = transform.position;

                    SetAnimTrigger(PlayerAnimationHandler.Jump);
                }
                break;

            case PlayerJumpState.Jumping:
                if (transform.position.y >= startPosition.y + JumpHeight)
                {
                    jumpState = PlayerJumpState.Falling;

                    SetAnimTrigger(PlayerAnimationHandler.Rolling);
                    return;
                }

                GravityMovingY(JumpUpPower);
                break;

            case PlayerJumpState.Falling:
                if (isGrounded == true)
                {
                    jumpState = PlayerJumpState.Ground;
                    return;
                }

                GravityMovingY(-downGravity);
                break;
        }
    }

    private void GravityMovingY(float yPower)
    {
        float gravityY = transform.position.y + (yPower * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, gravityY, transform.position.z);
    }

    private void CheckIfGrounded()
    {
        prevIsGrounded = isGrounded;

        if (groundChecker == null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(groundChecker.position, groundCheckerRadius, groundLayer);
        }

        if (animationHandler != null)
        {
            animationHandler.SetAnimBool(PlayerAnimationHandler.IsGround, isGrounded);
        }
    }
}
