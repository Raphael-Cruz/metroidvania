using UnityEngine;


public class InputManager : MonoBehaviour
{
    public static InputManager instance;
/*

TO ADD NEW ACTION:
[Header("Your New Action")]
public KeyCode yourActionKey = KeyCode.R;
public string yourActionButton = "YourAction";



*/
    [Header("Movement")]
    [Tooltip("Keyboard: A/D or Arrow Keys | Controller: Left Stick X-Axis")]
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;
    
    [Header("Jump")]
    [Tooltip("Keyboard: Space | Controller: Button 0 (A on Xbox, X on PlayStation)")]
    public KeyCode jumpKey = KeyCode.Space;
    public string jumpButton = "Jump"; // Controller button
    /*
    [Header("Dash")]
    [Tooltip("Keyboard: Left Shift | Controller: Button 1 (B on Xbox, Circle on PlayStation)")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public string dashButton = "Dash";
    */
    [Header("Shooting")]
    [Tooltip("Keyboard: Z or Left Click | Controller: Button 2 (X on Xbox, Square on PlayStation)")]
    public KeyCode shootKey = KeyCode.Z;
    public string shootButton = "Fire1";
    
    [Header("Skills (Missile/Shield)")]
    [Tooltip("Keyboard: X | Controller: Button 3 (Y on Xbox, Triangle on PlayStation)")]
    public KeyCode skillKey = KeyCode.X;
    public string skillButton = "Fire2";
    
    [Header("Hook/Grapple")]
    [Tooltip("Keyboard: C | Controller: Right Trigger (Button 5)")]
    public KeyCode hookKey = KeyCode.C;
    public string hookButton = "Hook";
    
    [Header("Potion Use")]
    [Tooltip("Keyboard: Q | Controller: Left Trigger (Button 4)")]
    public KeyCode potionKey = KeyCode.Q;
    public string potionButton = "Potion";
    
    [Header("Interact (Save Point/NPC)")]
    [Tooltip("Keyboard: E or F | Controller: Right Bumper (Button 5)")]
    public KeyCode interactKey = KeyCode.E;
    public string interactButton = "Interact";
    
    [Header("Pause/Menu")]
    [Tooltip("Keyboard: Escape | Controller: Start Button (Button 7)")]
    public KeyCode pauseKey = KeyCode.Escape;
    public string pauseButton = "Pause";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== MOVEMENT ==========
    public float GetHorizontalInput()
    {
        float input = 0f;
        
        // Keyboard input
        if (Input.GetKey(moveRightKey)) input += 1f;
        if (Input.GetKey(moveLeftKey)) input -= 1f;
        
        // Controller input (takes priority if detected)
        float axisInput = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(axisInput) > 0.1f) // Deadzone
        {
            input = axisInput;
        }
        
        return Mathf.Clamp(input, -1f, 1f);
    }

    // ========== JUMP ==========
    public bool GetJumpDown()
    {
        return Input.GetKeyDown(jumpKey) || Input.GetButtonDown(jumpButton);
    }

    public bool GetJump()
    {
        return Input.GetKey(jumpKey) || Input.GetButton(jumpButton);
    }
/*
    // ========== DASH ==========
    public bool GetDashDown()
    {
        return Input.GetKeyDown(dashKey) || Input.GetButtonDown(dashButton);
    }
*/
    // ========== SHOOTING ==========
    public bool GetShootDown()
    {
        return Input.GetKeyDown(shootKey) || Input.GetButtonDown(shootButton);
    }

    public bool GetShoot()
    {
        return Input.GetKey(shootKey) || Input.GetButton(shootButton);
    }

    // ========== SKILLS ==========
    public bool GetSkillDown()
    {
        return Input.GetKeyDown(skillKey) || Input.GetButtonDown(skillButton);
    }

    // ========== HOOK ==========
    public bool GetHookDown()
    {
        return Input.GetKeyDown(hookKey) || Input.GetButtonDown(hookButton);
    }

    public bool GetHook()
    {
        return Input.GetKey(hookKey) || Input.GetButton(hookButton);
    }

    // ========== POTION ==========
    public bool GetPotionDown()
    {
        return Input.GetKeyDown(potionKey) || Input.GetButtonDown(potionButton);
    }

    // ========== INTERACT ==========
    public bool GetInteractDown()
    {
        return Input.GetKeyDown(interactKey) || Input.GetButtonDown(interactButton);
    }

    // ========== PAUSE ==========
    public bool GetPauseDown()
    {
        return Input.GetKeyDown(pauseKey) || Input.GetButtonDown(pauseButton);
    }

    // ========== UTILITY ==========
    public bool IsUsingController()
    {
        // Check if any controller axis is being used
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || 
               Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f ||
               Input.GetJoystickNames().Length > 0;
    }

    // Display current control scheme in inspector
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(10, 10, 300, 20), 
                "Input: " + (IsUsingController() ? "Controller" : "Keyboard"), style);
        }
    }
}