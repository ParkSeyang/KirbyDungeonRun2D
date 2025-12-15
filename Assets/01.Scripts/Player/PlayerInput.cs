using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float Horizontal { get; private set; }
    public float Vertical   { get; private set; }

    public bool JumpDown    { get; private set; }
    public bool JumpHeld    { get; private set; }

    public bool BreathDown  { get; private set; }
    public bool BreathUp    { get; private set; }

    public bool AttackDown  { get; private set; }
    public bool DropAbilityDown { get; private set; }
    
    public float MoveX { get; private set; }
    
    public bool RunHeld     { get; private set; }
    
    public bool InhaleHeld { get; private set; }
    
    private void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical   = Input.GetAxisRaw("Vertical");

        JumpDown   = Input.GetKeyDown(KeyCode.C);
        JumpHeld   = Input.GetKey(KeyCode.C);

        BreathDown = Input.GetKeyDown(KeyCode.Z);
        BreathUp   = Input.GetKeyUp(KeyCode.Z);

        AttackDown = Input.GetKeyDown(KeyCode.X);
        DropAbilityDown = Input.GetKeyDown(KeyCode.D);

        RunHeld    = Input.GetKey(KeyCode.LeftShift);
    }
}