using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerAnimationHandler animationHandler;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInput playerInput;

    [Header("Breath Setting")]
    [SerializeField] private GameObject breatheCheckBox;

    [Header("Star Shooting Setting (Fat Kirby)")]
    [SerializeField] private GameObject starBulletPrefab;
    [SerializeField] private Transform starSpawnPoint;
    [SerializeField] private float starBulletSpeed = 8.0f;

    [Header("Sword Attack Setting")]
    [SerializeField] private AttackHitBox swordAttackHitBox;

    // ✅ 추가: Sword Jump Attack HitBox
    [Header("Sword Jump Attack HitBox")]
    [SerializeField] private AttackHitBox swordJumpAttackHitBox;

    [Header("Cutter Attack Setting")]
    [SerializeField] private GameObject cutterBulletPrefab;
    [SerializeField] private Transform cutterSpawnPoint;
    [SerializeField] private float cutterBulletSpeed = 10.0f;
    [SerializeField] private float cutterMaxDistance = 3.0f;

    // ✅ 추가: 스폰 위치가 너무 낮을 때 Y 보정값
    [Header("Cutter Spawn Offset")]
    [SerializeField] private float cutterSpawnYOffset = 0.45f;

    [Header("Cutter Dash Melee HitBox")]
    [SerializeField] private AttackHitBox cutterDashAttackHitBox;
    [SerializeField] private GameObject cutterDashHitBoxObject;

    // 태그 기반 정규화(선입력 방지/버퍼링)
    private const string ANIM_TAG_ATTACK = "Attack";

    public enum ActionState
    {
        None,
        Attack,
        DashAttack,
        JumpAttack,
        DeleteAbility,
        Breath,
        Eat,
    }

    [SerializeField] private ActionState currentActionState = ActionState.None;

    [SerializeField] private bool isBreathe = false;
    [SerializeField] private float timeChecker = 0.0f;

    [SerializeField] private bool isFatMode = false;
    [SerializeField] private bool isGetEnemy = false;

    public bool IsBreathe
    {
        get { return isBreathe; }
    }

    private void Awake()
    {
        if (animationHandler == null)
        {
            animationHandler = GetComponent<PlayerAnimationHandler>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
    }

    // ============================================================
    // 1. NormalAction
    // ============================================================
    public void NormalAction()
    {
        if (isFatMode == true && isGetEnemy == true)
        {
            HandleFatModeInput();
            return;
        }

        bool breathKeyDown = false;
        bool breathKeyUp = false;

        if (playerInput != null)
        {
            breathKeyDown = playerInput.BreathDown;
            breathKeyUp = playerInput.BreathUp;
        }
        else
        {
            breathKeyDown = Input.GetKeyDown(KeyCode.Z);
            breathKeyUp = Input.GetKeyUp(KeyCode.Z);
        }

        if (breathKeyDown == true)
        {
            isBreathe = true;
            currentActionState = ActionState.Breath;

            if (animationHandler != null)
            {
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsBreath, true);
            }

            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(true);
            }
        }

        if (breathKeyUp == true)
        {
            StopBreath();
        }
    }

    // ============================================================
    // 2. SwordAction
    // ============================================================
    public void SwordAction()
    {
        bool dropKeyDown = false;
        bool attackKeyDown = false;

        if (playerInput != null)
        {
            dropKeyDown = playerInput.DropAbilityDown;
            attackKeyDown = playerInput.AttackDown;
        }
        else
        {
            dropKeyDown = Input.GetKeyDown(KeyCode.D);
            attackKeyDown = Input.GetKeyDown(KeyCode.X);
        }

        if (dropKeyDown == true)
        {
            isBreathe = false;
            currentActionState = ActionState.DeleteAbility;

            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(false);
            }

            if (animationHandler != null)
            {
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsBreath, false);
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, false);
            }

            if (PlayerAbilitySystem.instance != null)
            {
                PlayerAbilitySystem.instance.DropAbility();
            }

            swordAttackHitBox = null;
            swordJumpAttackHitBox = null;
            currentActionState = ActionState.None;
            return;
        }

        Animator activeAnimator = ResolveActiveAnimator();

        if (activeAnimator != null)
        {
            bool allowNewAttack = UpdateSwordAnimatorState(activeAnimator, attackKeyDown);

            if (allowNewAttack == false)
            {
                return;
            }
        }

        if (attackKeyDown == true)
        {
            currentActionState = ActionState.Attack;

            if (animationHandler != null)
            {
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, true);
            }
        }
    }

    // ============================================================
    // 3. CutterAction
    // ============================================================
    public void CutterAction()
    {
        bool dropKeyDown = false;
        bool attackKeyDown = false;

        if (playerInput != null)
        {
            dropKeyDown = playerInput.DropAbilityDown;
            attackKeyDown = playerInput.AttackDown;
        }
        else
        {
            dropKeyDown = Input.GetKeyDown(KeyCode.D);
            attackKeyDown = Input.GetKeyDown(KeyCode.X);
        }

        if (dropKeyDown == true)
        {
            isBreathe = false;
            currentActionState = ActionState.DeleteAbility;

            if (animationHandler != null)
            {
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsBreath, false);
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, false);
            }

            if (breatheCheckBox != null)
            {
                breatheCheckBox.SetActive(false);
            }

            if (PlayerAbilitySystem.instance != null)
            {
                PlayerAbilitySystem.instance.DropAbility();
            }

            currentActionState = ActionState.None;
            return;
        }

        Animator activeAnimator = ResolveActiveAnimator();

        if (activeAnimator != null)
        {
            bool allowNewAttack = UpdateCutterAnimatorState(activeAnimator, attackKeyDown);

            if (allowNewAttack == false)
            {
                return;
            }
        }

        if (attackKeyDown == true)
        {
            currentActionState = ActionState.Attack;

            if (animationHandler != null)
            {
                animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, true);
            }
        }
    }

    // ============================================================
    // Animator 선택
    // ============================================================
    private Animator ResolveActiveAnimator()
    {
        if (animationHandler == null)
        {
            return null;
        }

        Animator anim = animationHandler.GetCurrentAnimator();

        if (anim != null)
        {
            return anim;
        }

        return animationHandler.GetComponentInChildren<Animator>(true);
    }

    // ============================================================
    // Sword 정규화 (Attack 태그)
    // ============================================================
    private bool UpdateSwordAnimatorState(Animator anim, bool attackKeyDown)
    {
        if (anim == null)
        {
            return true;
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag(ANIM_TAG_ATTACK) == true)
        {
            float t = stateInfo.normalizedTime;

            if (t < 0.3f)
            {
                if (animationHandler != null)
                {
                    animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, false);
                }
                return false;
            }

            if (t > 0.3f && t < 0.9f)
            {
                bool hasAttackParam = HasBoolParam(anim, PlayerAnimationHandler.IsAttack);

                if (hasAttackParam == true)
                {
                    bool cur = anim.GetBool(PlayerAnimationHandler.IsAttack);

                    if (cur == false && attackKeyDown == true)
                    {
                        if (animationHandler != null)
                        {
                            animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, true);
                        }
                    }
                }

                return false;
            }

            return false;
        }

        return true;
    }

    // ============================================================
    // Cutter 정규화 (Attack 태그)
    // ============================================================
    private bool UpdateCutterAnimatorState(Animator anim, bool attackKeyDown)
    {
        if (anim == null)
        {
            return true;
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag(ANIM_TAG_ATTACK) == true)
        {
            float t = stateInfo.normalizedTime;

            if (t < 0.3f)
            {
                if (animationHandler != null)
                {
                    animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, false);
                }
                return false;
            }

            if (t > 0.3f && t < 0.9f)
            {
                bool hasAttackParam = HasBoolParam(anim, PlayerAnimationHandler.IsAttack);

                if (hasAttackParam == true)
                {
                    bool cur = anim.GetBool(PlayerAnimationHandler.IsAttack);

                    if (cur == false && attackKeyDown == true)
                    {
                        if (animationHandler != null)
                        {
                            animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, true);
                        }
                    }
                }

                return false;
            }

            return false;
        }

        return true;
    }

    private bool HasBoolParam(Animator anim, int hash)
    {
        if (anim == null)
        {
            return false;
        }

        var ps = anim.parameters;

        if (ps == null)
        {
            return false;
        }

        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].nameHash == hash && ps[i].type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }

        return false;
    }

    // ============================================================
    // Cutter Animation Event
    // ============================================================
    public void CutterThrow()
    {
        // ===== 예외/가드 =====
        if (cutterBulletPrefab == null)
        {
            return;
        }

        Transform spawn = cutterSpawnPoint;

        if (spawn == null)
        {
            spawn = transform;
        }

        float dirX = ResolveFacingDirX();
        Vector2 dir = new Vector2(dirX, 0.0f);

        // ✅ 스폰 Y 보정
        Vector3 spawnPos = spawn.position + (Vector3.up * cutterSpawnYOffset);

        GameObject bulletObj = Instantiate(cutterBulletPrefab, spawnPos, Quaternion.identity);

        float dmg = 1.0f;

        if (Player.instance != null)
        {
            dmg = Player.instance.Attack;
        }

        CutterBullet cutterBullet = bulletObj.GetComponent<CutterBullet>();

        if (cutterBullet != null)
        {
            Vector2 originPos = new Vector2(spawnPos.x, spawnPos.y);

            Transform pTr = null;

            if (Player.instance != null)
            {
                pTr = Player.instance.transform;
            }

            cutterBullet.Fire(dir, cutterBulletSpeed, dmg, cutterMaxDistance, originPos, pTr);
        }
        else
        {
            Rigidbody2D rb2 = bulletObj.GetComponent<Rigidbody2D>();

            if (rb2 != null)
            {
                rb2.gravityScale = 0.0f;
                rb2.linearVelocity = dir * cutterBulletSpeed;
            }
        }
    }

    public void CutterAttackEnd()
    {
        if (animationHandler != null)
        {
            animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, false);
        }

        currentActionState = ActionState.None;
    }

    // ============================================================
    // Cutter Dash HitBox
    // ============================================================
    private void ResolveCutterDashHitBox()
    {
        if (cutterDashAttackHitBox != null)
        {
            return;
        }

        AttackHitBox[] all = GetComponentsInChildren<AttackHitBox>(true);

        if (all == null)
        {
            return;
        }

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null)
            {
                continue;
            }

            GameObject go = all[i].gameObject;

            if (go == null)
            {
                continue;
            }

            if (go.name.Contains("CutterDashHitBox") == true)
            {
                cutterDashAttackHitBox = all[i];
                cutterDashHitBoxObject = go;
                return;
            }
        }
    }

    public void CutterDashHitBoxOn()
    {
        ResolveCutterDashHitBox();

        if (cutterDashAttackHitBox == null)
        {
            return;
        }

        if (cutterDashHitBoxObject != null)
        {
            if (cutterDashHitBoxObject.activeSelf == false)
            {
                cutterDashHitBoxObject.SetActive(true);
            }
        }

        Player player = Player.instance;

        if (player == null)
        {
            return;
        }

        float dmg = player.Attack;

        if (dmg <= 0.0f)
        {
            return;
        }

        cutterDashAttackHitBox.BeginHitBox(dmg);
    }

    public void CutterDashHitBoxOff()
    {
        ResolveCutterDashHitBox();

        if (cutterDashAttackHitBox == null)
        {
            return;
        }

        cutterDashAttackHitBox.EndHitBox();

        if (cutterDashHitBoxObject != null)
        {
            if (cutterDashHitBoxObject.activeSelf == true)
            {
                cutterDashHitBoxObject.SetActive(false);
            }
        }
    }

    private float ResolveFacingDirX()
    {
        float dirX = 1.0f;

        Animator checkAnimator = ResolveActiveAnimator();

        if (checkAnimator != null && checkAnimator.transform.localScale.x < 0.0f)
        {
            dirX = -1.0f;
        }

        return dirX;
    }

    // ============================================================
    // 4. OnEatNormalEnemy
    // ============================================================
    public void OnEatNormalEnemy()
    {
        StopBreath();

        isFatMode = true;
        isGetEnemy = true;
        currentActionState = ActionState.Eat;

        if (animationHandler != null)
        {
            animationHandler.SetAnimBool(PlayerAnimationHandler.FatMode, isFatMode);
            animationHandler.SetAnimBool(PlayerAnimationHandler.GetEnemy, isGetEnemy);
        }
    }

    private void HandleFatModeInput()
    {
        bool attackKeyDown = false;

        if (playerInput != null)
        {
            attackKeyDown = playerInput.AttackDown;
        }
        else
        {
            attackKeyDown = Input.GetKeyDown(KeyCode.X);
        }

        if (attackKeyDown == false)
        {
            return;
        }

        currentActionState = ActionState.Attack;

        if (animationHandler != null)
        {
            animationHandler.SetAnimTrigger(PlayerAnimationHandler.Shooting);
        }

        SpawnStarBullet();

        isFatMode = false;
        isGetEnemy = false;
        currentActionState = ActionState.None;

        if (animationHandler != null)
        {
            animationHandler.SetAnimBool(PlayerAnimationHandler.FatMode, isFatMode);
            animationHandler.SetAnimBool(PlayerAnimationHandler.GetEnemy, isGetEnemy);
        }
    }

    private void SpawnStarBullet()
    {
        if (starBulletPrefab == null)
        {
            return;
        }

        Transform spawn = starSpawnPoint;

        if (spawn == null)
        {
            spawn = transform;
        }

        float dirX = ResolveFacingDirX();
        Vector2 shootDir = new Vector2(dirX, 0.0f);

        GameObject star = Instantiate(starBulletPrefab, spawn.position, Quaternion.identity);

        Rigidbody2D rb2 = star.GetComponent<Rigidbody2D>();

        if (rb2 != null)
        {
            rb2.linearVelocity = shootDir * starBulletSpeed;
        }
    }

    // ============================================================
    // 5. StopBreath
    // ============================================================
    public void StopBreath()
    {
        isBreathe = false;
        currentActionState = ActionState.None;

        if (animationHandler != null)
        {
            animationHandler.SetAnimBool(PlayerAnimationHandler.IsBreath, false);
        }

        if (breatheCheckBox != null)
        {
            breatheCheckBox.SetActive(false);
        }
    }

    // ============================================================
    // 6. Sword HitBox (Ground) - Animation Event
    // ============================================================
    private void ResolveSwordHitBox()
    {
        if (swordAttackHitBox != null)
        {
            return;
        }

        AttackHitBox[] all = GetComponentsInChildren<AttackHitBox>(true);

        if (all == null)
        {
            return;
        }

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) { continue; }

            string n = all[i].gameObject.name;

            if (n.Contains("Jump") == false && n.Contains("AttackJump") == false)
            {
                swordAttackHitBox = all[i];
                return;
            }
        }

        if (all.Length > 0)
        {
            swordAttackHitBox = all[0];
        }
    }

    public void SwordHitBoxOn()
    {
        ResolveSwordHitBox();

        if (swordAttackHitBox == null)
        {
            return;
        }

        Player player = Player.instance;

        if (player == null)
        {
            return;
        }

        float dmg = player.Attack;

        if (dmg <= 0.0f)
        {
            return;
        }

        swordAttackHitBox.BeginHitBox(dmg);
    }

    public void SwordHitBoxOff()
    {
        ResolveSwordHitBox();

        if (swordAttackHitBox == null)
        {
            return;
        }

        swordAttackHitBox.EndHitBox();
    }

    // ============================================================
    // 7. Sword HitBox (Jump) - Animation Event
    // ============================================================
    private void ResolveSwordJumpHitBox()
    {
        if (swordJumpAttackHitBox != null)
        {
            return;
        }

        AttackHitBox[] all = GetComponentsInChildren<AttackHitBox>(true);

        if (all == null)
        {
            return;
        }

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) { continue; }

            string n = all[i].gameObject.name;

            if (n.Contains("Jump") == true || n.Contains("AttackJump") == true)
            {
                swordJumpAttackHitBox = all[i];
                return;
            }
        }
    }

    public void SwordJumpHitBoxOn()
    {
        ResolveSwordJumpHitBox();

        if (swordJumpAttackHitBox == null)
        {
            return;
        }

        Player player = Player.instance;

        if (player == null)
        {
            return;
        }

        float dmg = player.Attack;

        if (dmg <= 0.0f)
        {
            return;
        }

        swordJumpAttackHitBox.BeginHitBox(dmg);
    }

    public void SwordJumpHitBoxOff()
    {
        ResolveSwordJumpHitBox();

        if (swordJumpAttackHitBox == null)
        {
            return;
        }

        swordJumpAttackHitBox.EndHitBox();
    }

    public void SwordAttackEnd()
    {
        if (animationHandler != null)
        {
            animationHandler.SetAnimBool(PlayerAnimationHandler.IsAttack, false);
        }

        currentActionState = ActionState.None;
    }
}
