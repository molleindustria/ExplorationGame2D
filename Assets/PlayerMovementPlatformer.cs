using UnityEngine;

public class PlayerMovementPlatformer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("2D physics body")]
    public Rigidbody2D rb;
    [Tooltip("What counts as ground for IsTouching")]
    public ContactFilter2D GroundFilter;

    [Header("Horizontal (ground/air)")]
    [Tooltip("Max running speed (units/sec)")]
    public float maxRunSpeed = 6f;
    [Tooltip("Ground acceleration toward target speed")]
    public float runAccel = 60f;
    [Tooltip("Ground deceleration toward zero/target")]
    public float runDecel = 80f;
    [Tooltip("Air acceleration toward target speed")]
    public float airAccel = 40f;
    [Tooltip("Air deceleration (let go mid-air)")]
    public float airDecel = 40f;

    [Header("Jump (SMB-like)")]
    [Tooltip("Desired full jump height (units)")]
    public float jumpHeight = 3f;
    [Tooltip("Time to apex for full jump (seconds)")]
    public float timeToApex = 0.40f;
    [Tooltip("Extra gravity multiplier when rising & jump released early")]
    public float lowJumpGravityMult = 2.0f;
    [Tooltip("Extra gravity multiplier when falling")]
    public float fallGravityMult = 2.5f;
    [Tooltip("Max downward speed (terminal velocity)")]
    public float terminalFallSpeed = 25f;

    [Header("Jump forgiveness")]
    [Tooltip("Coyote time after leaving ground (sec)")]
    public float coyoteTime = 0.10f;
    [Tooltip("Jump buffer before landing (sec)")]
    public float jumpBufferTime = 0.10f;

    [Header("Ladder")]
    [Tooltip("When on ladder: vertical move speed (units/sec)")]
    public float ladderSpeed = 4f;
    [Tooltip("Acceleration on ladder (to reach ladderSpeed)")]
    public float ladderAccel = 40f;
    [Tooltip("Deceleration on ladder (when no vertical input)")]
    public float ladderDecel = 60f;

    [Header("Input")]
    [Tooltip("Name of the horizontal axis")]
    public string horizontalAxis = "Horizontal";
    [Tooltip("Name of the vertical axis")]
    public string verticalAxis = "Vertical";
    [Tooltip("Buttons considered as Jump")]
    public string[] jumpButtons = new[] { "Jump", "Fire2" };
    [Tooltip("Treat UP input as Jump when not on ladder")]
    public bool upIsJump = true;

    [Header("Mode/Options")]
    [Tooltip("When on moves vertically instead of jumping")]
    public bool ladderMode = false;
    [Tooltip("Disable all movement & input")]
    public bool frozen = false;

    [Header("State (read-only)")]
    public bool isGrounded = false;
    public Vector2 movementInput; // x = run input; y = vertical input (used only on ladder)

    // --- internals ---
    float baseGravity;         // required gravity magnitude for desired apex
    float baseJumpSpeed;       // required initial jump speed for desired apex
    float gravityScaleBase;    // rb.gravityScale that achieves baseGravity with current project gravity
    float coyoteTimer = 0f;
    float bufferTimer = 0f;

    // For detecting UP "button down" edge when treating UP as jump
    private bool prevUpHeld = false;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        RecomputeJumpPhysics();
    }

    void OnValidate()
    {
        timeToApex = Mathf.Max(0.05f, timeToApex);
        jumpHeight = Mathf.Max(0.05f, jumpHeight);
        fallGravityMult = Mathf.Max(1f, fallGravityMult);
        lowJumpGravityMult = Mathf.Max(1f, lowJumpGravityMult);
        ladderSpeed = Mathf.Max(0f, ladderSpeed);
        ladderAccel = Mathf.Max(0f, ladderAccel);
        ladderDecel = Mathf.Max(0f, ladderDecel);
        if (!Application.isPlaying) RecomputeJumpPhysics();
    }

    void RecomputeJumpPhysics()
    {
        // Kinematics: g = 2h / t^2 ; v0 = g * t
        baseGravity = (2f * jumpHeight) / (timeToApex * timeToApex); // positive magnitude
        baseJumpSpeed = baseGravity * timeToApex;                       // upward initial speed
        float projG = Mathf.Abs(Physics2D.gravity.y);                 // project gravity magnitude
        gravityScaleBase = baseGravity / Mathf.Max(0.0001f, projG);
    }

    void Update()
    {
        if (frozen) return;

        // 1) Input
        float h = Input.GetAxisRaw(horizontalAxis);
        movementInput.x = Mathf.Clamp(h, -1f, 1f);

        float vIn = Input.GetAxisRaw(verticalAxis);
        movementInput.y = ladderMode ? Mathf.Clamp(vIn, -1f, 1f) : 0f; // vertical input only used on ladder

        // Unified jump input (buttons + optional Up axis)
        bool jumpPressed = false;
        bool jumpHeld = false;

        // Buttons
        for (int i = 0; i < jumpButtons.Length; i++)
        {
            if (Input.GetButtonDown(jumpButtons[i])) jumpPressed = true;
            if (Input.GetButton(jumpButtons[i])) jumpHeld = true;
        }

        // UP as jump (only when not on ladder)
        if (upIsJump && !ladderMode)
        {
            bool upHeldNow = vIn > 0.1f;
            if (upHeldNow && !prevUpHeld) jumpPressed = true; // edge = button down
            if (upHeldNow) jumpHeld = true;
            prevUpHeld = upHeldNow;
        }
        else
        {
            prevUpHeld = false; // reset edge tracker when not using up-as-jump or on ladder
        }

        // 2) Grounded & timers
        isGrounded = rb.IsTouching(GroundFilter);

        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (jumpPressed) bufferTimer = jumpBufferTime;
        else bufferTimer -= Time.deltaTime;

        // 3) Ladder vs Jump/Gravity logic
        if (ladderMode)
        {
            // On ladder → no gravity
            rb.gravityScale = 0f;
            // Clear jump buffers while on ladder so we don't auto-jump on exit
            bufferTimer = 0f;
            coyoteTimer = 0f;
        }
        else
        {
            // Not on ladder → normal jump start (buffered + coyote)
            if (bufferTimer > 0f && coyoteTimer > 0f)
            {
                bufferTimer = 0f;
                coyoteTimer = 0f;

                var v = rb.linearVelocity;
                v.y = baseJumpSpeed;
                rb.linearVelocity = v;
            }

            // Gravity scaling (variable height + stronger fall)
            float gScale = gravityScaleBase;
            if (rb.linearVelocity.y > 0.01f)
            {
                if (!jumpHeld) gScale *= lowJumpGravityMult; // cut jump when released early
            }
            else if (rb.linearVelocity.y < -0.01f)
            {
                gScale *= fallGravityMult;
            }
            rb.gravityScale = gScale;
        }
    }

    void FixedUpdate()
    {
        if (frozen) return;

        Vector2 v = rb.linearVelocity;

        // --- Horizontal movement (same on ground/air/ladder) ---
        float targetX = movementInput.x * maxRunSpeed;
        bool onGround = isGrounded;

        float ax;
        if (Mathf.Abs(targetX) > 0.01f)
            ax = onGround ? runAccel : airAccel;
        else
            ax = onGround ? runDecel : airDecel;

        v.x = Mathf.MoveTowards(v.x, targetX, ax * Time.fixedDeltaTime);
        v.x = Mathf.Clamp(v.x, -maxRunSpeed, maxRunSpeed);

        // --- Vertical handling ---
        if (ladderMode)
        {
            // Move at fixed ladderSpeed, with accel/decel, and ignore gravity
            float targetY = movementInput.y * ladderSpeed;
            float ay = (Mathf.Abs(targetY) > 0.01f) ? ladderAccel : ladderDecel;
            v.y = Mathf.MoveTowards(v.y, targetY, ay * Time.fixedDeltaTime);
        }
        else
        {
            // Terminal fall speed in normal mode
            if (v.y < -terminalFallSpeed)
                v.y = -terminalFallSpeed;
        }

        rb.linearVelocity = v;
    }

    // External controls (optional)
    public void Freeze()
    {
        frozen = true;
        rb.linearVelocity = Vector2.zero;
    }

    public void UnFreeze()
    {
        frozen = false;
    }

    // Ladder triggers (as you had)
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ladder")
        {
            ladderMode = true;
            // Optional: damp vertical when latching onto ladder
            var v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ladder")
        {
            ladderMode = false;
            // Restore base gravity immediately when leaving ladder
            rb.gravityScale = gravityScaleBase;
        }
    }
}
