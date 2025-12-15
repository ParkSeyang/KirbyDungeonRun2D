using UnityEngine;

public class PlayerAnimEventBridge : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerAbilityController playerAbilityController;

    private void Awake()
    {
        // ===== 예외/가드 =====
        if (playerAbilityController == null)
        {
            playerAbilityController = GetComponentInParent<PlayerAbilityController>();
        }
    }

    // ===== Sword (Ground) =====
    public void SwordHitBoxOn()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.SwordHitBoxOn();
    }

    public void SwordHitBoxOff()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.SwordHitBoxOff();
    }

    // ✅ Sword (Jump) 추가
    public void SwordJumpHitBoxOn()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.SwordJumpHitBoxOn();
    }

    public void SwordJumpHitBoxOff()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.SwordJumpHitBoxOff();
    }

    // (호환용) 있으면 써도 되고, 없어도 됨(이제 Sword는 Tag 정규화로도 충분)
    public void SwordAttackEnd()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.SwordAttackEnd();
    }

    // ===== Cutter (Throw) =====
    public void CutterThrow()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.CutterThrow();
    }

    public void CutterAttackEnd()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.CutterAttackEnd();
    }

    // ===== Cutter (Dash Melee HitBox) =====
    public void CutterDashHitBoxOn()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.CutterDashHitBoxOn();
    }

    public void CutterDashHitBoxOff()
    {
        if (playerAbilityController == null) { return; }
        playerAbilityController.CutterDashHitBoxOff();
    }
}