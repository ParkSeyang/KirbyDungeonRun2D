using UnityEngine;

/// <summary>
/// 애니메이션 전담 컴포넌트.
/// 기본 Kirby Animator + 능력 Kirby Animator 둘 다 한 곳에서 관리.
/// 다른 스크립트는 Animator를 직접 건드리지 말고 이 컴포넌트를 통해서만 조작.
/// </summary>
public class PlayerAnimationHandler : MonoBehaviour
{
    [Header("Animator References")]
    [SerializeField] private Animator animator;        // 기본 Kirby 애니메이터
    [SerializeField] private Animator abilityAnimator; // 능력 Kirby 애니메이터 (있을 때만)

    // ✅ Flip 대상 루트(기본 / 능력)
    [Header("Flip Roots")]
    [SerializeField] private Transform baseFlipRoot;
    [SerializeField] private Transform abilityFlipRoot;

    // 공통 파라미터 해시
    public static readonly int Movement   = Animator.StringToHash("Movement");
    public static readonly int IsRun      = Animator.StringToHash("IsRun");
    public static readonly int Jump       = Animator.StringToHash("Jump");
    public static readonly int Falling    = Animator.StringToHash("Falling");
    public static readonly int Rolling    = Animator.StringToHash("Rolling");
    public static readonly int Flying     = Animator.StringToHash("Flying");
    public static readonly int FlyReady   = Animator.StringToHash("FlyReady");
    public static readonly int IsGround   = Animator.StringToHash("IsGround");
    public static readonly int IsHurt     = Animator.StringToHash("IsHurt");

    // Normal Kirby 전용
    public static readonly int IsBreath   = Animator.StringToHash("IsBreath");
    public static readonly int BreathTime = Animator.StringToHash("BreathTime");
    public static readonly int GetEnemy   = Animator.StringToHash("GetEnemy");
    public static readonly int Eating     = Animator.StringToHash("Eating");
    public static readonly int FatMode    = Animator.StringToHash("FatMode");
    public static readonly int Shooting   = Animator.StringToHash("Shooting");

    // 능력 Kirby 공통
    public static readonly int IsAttack   = Animator.StringToHash("IsAttack");

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (baseFlipRoot == null && animator != null)
        {
            baseFlipRoot = animator.transform;
        }
    }

    // AbilitySystem에서 능력 애니메이터를 등록할 때 사용
    public void SetAbilityAnimator(Animator newAnimator)
    {
        abilityAnimator = newAnimator;
    }

    public void ClearAbilityAnimator()
    {
        abilityAnimator = null;
    }

    // ✅ 추가: 능력 프리팹 루트(visual root)를 등록 (중요)
    public void SetAbilityVisualRoot(Transform newRoot)
    {
        abilityFlipRoot = newRoot;

        // 등록 즉시, 현재 바라보는 방향을 그대로 복사해준다(이동 입력이 없어도 즉시 정상 방향)
        if (abilityFlipRoot == null)
        {
            return;
        }

        float signX = 1.0f;

        if (baseFlipRoot != null && baseFlipRoot.localScale.x < 0.0f)
        {
            signX = -1.0f;
        }

        Vector3 s = abilityFlipRoot.localScale;
        float absX = Mathf.Abs(s.x);
        abilityFlipRoot.localScale = new Vector3(absX * signX, s.y, s.z);
    }

    // ✅ 현재 “실제로 공격/능력 애니가 도는” Animator를 반환
    public Animator GetCurrentAnimator()
    {
        if (abilityAnimator != null)
        {
            return abilityAnimator;
        }

        return animator;
    }

    // ✅ 현재 바라보는 방향(좌:-1 / 우:+1)
    public float GetFacingSignX()
    {
        Transform t = null;

        if (abilityFlipRoot != null)
        {
            t = abilityFlipRoot;
        }
        else
        {
            t = baseFlipRoot;
        }

        if (t == null)
        {
            return 1.0f;
        }

        if (t.localScale.x < 0.0f)
        {
            return -1.0f;
        }

        return 1.0f;
    }

    private bool HasParameter(Animator anim, int hash, AnimatorControllerParameterType type)
    {
        if (anim == null)
        {
            return false;
        }

        foreach (var checkparameter in anim.parameters)
        {
            if (checkparameter.nameHash == hash && checkparameter.type == type)
            {
                return true;
            }
        }

        return false;
    }

    public void SetAnimFloat(int hash, float value)
    {
        if (HasParameter(animator, hash, AnimatorControllerParameterType.Float))
        {
            animator.SetFloat(hash, value);
        }

        if (HasParameter(abilityAnimator, hash, AnimatorControllerParameterType.Float))
        {
            abilityAnimator.SetFloat(hash, value);
        }
    }

    public void SetAnimBool(int hash, bool value)
    {
        if (HasParameter(animator, hash, AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(hash, value);
        }

        if (HasParameter(abilityAnimator, hash, AnimatorControllerParameterType.Bool))
        {
            abilityAnimator.SetBool(hash, value);
        }
    }

    public void SetAnimTrigger(int hash)
    {
        if (HasParameter(animator, hash, AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(hash);
        }

        if (HasParameter(abilityAnimator, hash, AnimatorControllerParameterType.Trigger))
        {
            abilityAnimator.SetTrigger(hash);
        }
    }

    public void FlipByInput(Vector2 inputMoveXY, float absInputXY)
    {
        // ===== 예외/가드 =====
        if (absInputXY <= 0.0f)
        {
            return;
        }

        float signX = 1.0f;

        if (inputMoveXY.x < 0.0f)
        {
            signX = -1.0f;
        }

        // 기본 바디 루트 Flip
        if (baseFlipRoot != null)
        {
            Vector3 s = baseFlipRoot.localScale;
            float absX = Mathf.Abs(s.x);
            baseFlipRoot.localScale = new Vector3(absX * signX, s.y, s.z);
        }

        // ✅ 능력 프리팹 루트 Flip (CutterKirby가 여기서 해결됨)
        if (abilityFlipRoot != null)
        {
            Vector3 s = abilityFlipRoot.localScale;
            float absX = Mathf.Abs(s.x);
            abilityFlipRoot.localScale = new Vector3(absX * signX, s.y, s.z);
        }
    }
}
