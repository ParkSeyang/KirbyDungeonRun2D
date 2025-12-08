using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

// 전투 관련 기능 및 이동 관련 기능 그리고 애니메이션 추가
public class PlayerController : MonoBehaviour
{
    private static readonly int Movement = Animator.StringToHash("Movement");
    private static readonly int IsGround = Animator.StringToHash("IsGround");
    private static readonly int IsRun = Animator.StringToHash("IsRun");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Falling = Animator.StringToHash("Falling");
    private static readonly int Rolling = Animator.StringToHash("Rolling");
    private static readonly int IsBreath = Animator.StringToHash("IsBreath");

    [Header("PlayerAnimationSetting")]
    [SerializeField] private Animator animator { get; set; }
    [SerializeField] private SpriteRenderer KirbySprite;

    [Header("MoveSetting")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float runSpeed = 3.0f;
    
    // 캐릭터 애니메이션 판정
    
    [Header("JumpSetting")]
    [SerializeField] private float JumpHeight = 1.0f;
    [SerializeField] private float JumpUpPower = 1.0f;
    [SerializeField] private float downGravity = 3.0f; 
    // [SerializeField] private bool Isflying = false;
    
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
    
    private enum PlayerJumpState
    {
        Ground,
        Jumping,
        Falling,
    }
    
    [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckerRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
  
    private PlayerJumpState jumpState = PlayerJumpState.Ground;
    [SerializeField]
    private bool isGrounded = true;
    private bool prevIsGrounded;
    private Vector3 startPosition;
    
    public static PlayerController localPlayer;
    
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        KirbySprite = GetComponentInChildren<SpriteRenderer>();

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
    
    private void OnDrawGizmosSelected()
    {
        if (groundChecker == null)
        {
            return;
        }

        Gizmos.color = Color.lawnGreen;
        Gizmos.DrawWireSphere(groundChecker.position, groundCheckerRadius);
        
    }
    
    void MovementUpdate()
    {
        if (isBreathe == true)
        {
            animator.SetFloat(Movement,0.0f);
            animator.SetBool(IsRun,false);
            return;
        }

        Vector2 inputMoveXY = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float absInputXY = Mathf.Abs(inputMoveXY.magnitude);
        animator.SetFloat(Movement, absInputXY);
        
       if (absInputXY > 0)
       {
           animator.transform.localScale = (inputMoveXY.x < 0)
               ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
       }
       
       Vector2 moveVector = new Vector2(inputMoveXY.x, 0.0f);
       transform.Translate(moveVector * moveSpeed * Time.deltaTime);

       if (Input.GetKey(KeyCode.LeftShift))
       {
           animator.SetBool(IsRun, true);
           transform.Translate(moveVector * runSpeed * Time.deltaTime);
       }
       else
       {
           animator.SetBool(IsRun, false);
       }
       
    }
    
    
    
    void JumpUpdate()
    {
        CheckIfGrounded();

        if (prevIsGrounded != isGrounded && jumpState == PlayerJumpState.Ground)
        {
            jumpState = PlayerJumpState.Falling;
            animator.SetTrigger(Falling);
            return;
        }

        
        switch (jumpState)
        {
            case PlayerJumpState.Ground:
                if (isBreathe == false &&Input.GetKey(KeyCode.C))
                {
                    // 땅에서 점프로 상태변화
                    jumpState = PlayerJumpState.Jumping;
                    // startPosition변수에 현재 transform의 포지션을 넣어준다.
                    startPosition = transform.position;
                    animator.SetTrigger(Jump);
                    
                }
                break;
            case PlayerJumpState.Jumping:
                if (transform.position.y >= startPosition.y + JumpHeight)
                {
                    jumpState = PlayerJumpState.Falling;
                    animator.SetTrigger(Rolling);
                    return;
                }
                GravityMovingY(JumpUpPower);
                break;
            case PlayerJumpState.Falling:
                if (isGrounded)
                {
                    jumpState = PlayerJumpState.Ground;
                    return;
                }
                GravityMovingY(-downGravity);
                break;
            default:
                break;
        }

        void GravityMovingY(float yPower)
        {
            float GravityY = transform.position.y + (yPower * Time.deltaTime);

            transform.position = new Vector3(transform.position.x, GravityY, transform.position.z);
        }

    }
    
    private void CheckIfGrounded()
    {
        prevIsGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundChecker.position, groundCheckerRadius, groundLayer);
        
        animator.SetBool(IsGround,isGrounded);
    }
    
    void ActionStateUpdate()
    {
        // bool isBreath = Physics2D.OverlapBox(Breathchecker.position, BreathcheckerRadius, BreathLayer);

        if (jumpState == PlayerJumpState.Jumping || jumpState == PlayerJumpState.Falling)
        {
            return;
        }
   
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isBreathe = true;
            currentActionState = ActionState.Breath;
            animator.SetBool(IsBreath,true);

            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(true);
            }
        }
        if (Input.GetKeyUp(KeyCode.Z))
        {
            isBreathe = false;
            currentActionState = ActionState.None;
            
            animator.SetBool(IsBreath,false);
            
            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(false);
            }
        }
        // 먹기 기능
       // if (Input.GetKey(KeyCode.DownArrow))
       // {
       //     
       // }
       // 
       // if (Input.GetKeyDown(KeyCode.X))
       // {
       //     
       // }
        
        

    }
    
    
}
