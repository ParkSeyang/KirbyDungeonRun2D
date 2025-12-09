using UnityEngine;

public enum PlayerJumpState
{
    Ground,
    Jumping,
    Falling,
}

// 이동 / 점프 / 지면 체크만 담당하는 컴포넌트로 변경
public class PlayerMovement : MonoBehaviour
{
    [Header("MoveSetting")]
    [SerializeField] private float moveSpeed = 2.0f;   
    [SerializeField] private float runSpeed = 3.0f;    

    [Header("JumpSetting")]
    [SerializeField] private float JumpHeight = 1.0f;  
    [SerializeField] private float JumpUpPower = 1.0f; 
    [SerializeField] private float downGravity = 3.0f; 

    [Header("GroundSetting")]
    [SerializeField] private Transform groundChecker;       
    [SerializeField] private float groundCheckerRadius = 0.1f; 
    [SerializeField] private LayerMask groundLayer;        

    private PlayerJumpState jumpState = PlayerJumpState.Ground; 
    [SerializeField]
    private bool isGrounded = true;     
    private bool prevIsGrounded;        
    private Vector3 startPosition;      

    // 새로 추가되는 공개 상태 값들 (PlayerController에서 읽을 용도)
    public float CurrentMoveX { get; private set; }      // 새 이름
    public bool IsRun { get; private set; }              // 새 이름
    public bool IsGrounded { get { return isGrounded; } } // 기존 isGrounded를 감싼 프로퍼티
    public PlayerJumpState CurrentJumpState { get { return jumpState; } } // jumpState 읽기용

    // PlayerController가 이동/점프를 잠글 때 사용하는 플래그 (새 이름)
    public bool CanMove { get; set; }
    public bool CanJump { get; set; }

    private void Awake()
    {
        CanMove = true;
        CanJump = true;
    }

    private void Update()
    {
        CheckIfGrounded(); 
        UpdateMoveX();     // 기존 MovementUpdate에서 실제 이동하던 부분
        UpdateJump();      // 기존 HandleNormalJump + GravityMovingY 로직
    }

    private void UpdateMoveX()
    {
        if (CanMove == false)
        {
            CurrentMoveX = 0.0f;
            IsRun = false;
            return;
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        CurrentMoveX = inputX;

        bool shiftKey = Input.GetKey(KeyCode.LeftShift);
        IsRun = shiftKey;
        
        // Mathf.Approximately(float a, float b)두 변수 a, b가 같은지(근사한지) 비교합니다.
        if (Mathf.Approximately(inputX, 0.0f))
        {
            return;
        }

        float speed = moveSpeed;

        if (shiftKey == true)
        {
            speed = runSpeed;
        }

        Vector2 moveVector = new Vector2(inputX, 0.0f);
        transform.Translate(moveVector * speed * Time.deltaTime);
    }

    private void UpdateJump()
    {
        switch (jumpState)
        {
            case PlayerJumpState.Ground:
                
                if (CanJump == true && isGrounded == true && Input.GetKey(KeyCode.C))
                {
                    jumpState = PlayerJumpState.Jumping;
                    
                    // startPosition변수에 현재 transform의 포지션을 넣어준다.
                    startPosition = transform.position;
                }
                break;

            case PlayerJumpState.Jumping:
                if (transform.position.y >= startPosition.y + JumpHeight)
                {
                    // 점프 최대 거리까지 올라가면 낙하로 변경
                    jumpState = PlayerJumpState.Falling;
                    return;
                }
                GravityMovingY(JumpUpPower);
                break;

            case PlayerJumpState.Falling:
                if (isGrounded == true)
                {
                    // 착지
                    jumpState = PlayerJumpState.Ground;
                    return;
                }
                GravityMovingY(-downGravity);
                break;
        }

        // 원래 JumpUpdate() 첫 부분에 있던 “지면에서 떨어졌을 때 Falling 처리” 로직
        bool leftGroundFromGround =
            (prevIsGrounded != isGrounded && jumpState == PlayerJumpState.Ground);

        if (leftGroundFromGround == true)
        {
            jumpState = PlayerJumpState.Falling;
        }
    }

    private void CheckIfGrounded()
    {
        prevIsGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundChecker.position, groundCheckerRadius, groundLayer);
        
    }

    private void GravityMovingY(float yPower)
    {
        float GravityY = transform.position.y + (yPower * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, GravityY, transform.position.z);
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
}
