using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Tooltip("How fast it accelerates (used as force or velocity depending on settings)")]
    public float movementForce = 1f;

    [Tooltip("Limit the velocity")]
    public Vector2 maxVelocity = new Vector2(10, 10);

    [Tooltip("Make it into a side scroller")]
    public bool twoDirection = false;

    [Tooltip("Only for two direction: jump force on the Y axis (0 for no jump)")]
    public float jumpForce = 0;

    [Tooltip("Set true to reach full speed instantly (raw input smoothing off)")]
    public bool analogSpeed = true;

    [Tooltip("Zero the velocity when direction is unpressed")]
    public bool noInertia = false;

    [Tooltip("The component that manages the 2D physics")]
    public Rigidbody2D rb;

    [Tooltip("Series of settings to determine if the collision is ground")]
    public ContactFilter2D GroundFilter;

    [Tooltip("For side view check if hitting collider below the sprite")]
    public bool isGrounded = false;

    [Tooltip("If set to true prevents any movements")]
    public bool frozen = false;

    [Tooltip("Minimum time between jumps")]
    public float jumpWait = 0.5f;
    private float jumpTimer = 0;

    [Tooltip("Gravity scale when moving up")]
    public float jumpGravity = 0;
    [Tooltip("Gravity scale when falling (tune for less floaty movement)")]
    public float fallGravity = 0;

    public Vector2 movementInput;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (frozen) return;

        // Read movement input
        if (analogSpeed)
        {
            movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        else
        {
            movementInput = Vector2.zero;

            if (Input.GetAxisRaw("Horizontal") > 0) movementInput.x = 1;
            if (Input.GetAxisRaw("Horizontal") < 0) movementInput.x = -1;
            if (Input.GetAxisRaw("Vertical") > 0) movementInput.y = 1;
            if (Input.GetAxisRaw("Vertical") < 0) movementInput.y = -1;
        }

        // Two direction = side scroller (ignore vertical input)
        if (twoDirection)
        {
            movementInput = new Vector2(movementInput.x, 0);
        }

        // Handle jumping
        if (twoDirection && jumpForce > 0)
        {
            jumpTimer -= Time.deltaTime;
            isGrounded = rb.IsTouching(GroundFilter);

            if ((Input.GetButtonDown("Fire2") || Input.GetAxisRaw("Vertical") > 0) && isGrounded && jumpTimer <= 0)
            {
                jumpTimer = jumpWait;
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            }

            // Adjust gravity
            if (rb.linearVelocity.y > 0)
                rb.gravityScale = jumpGravity;
            else
                rb.gravityScale = fallGravity;
        }
    }

    void FixedUpdate()
    {
        if (frozen) return;

        // Apply inertia cancellation
        if (noInertia)
        {
            Vector2 newVelocity = rb.linearVelocity;
            if (Input.GetAxisRaw("Horizontal") == 0) newVelocity.x = 0;
            if (Input.GetAxisRaw("Vertical") == 0 && !twoDirection) newVelocity.y = 0;
            rb.linearVelocity = newVelocity;
        }

        // Apply movement as force
        Vector2 movement = new Vector2(movementInput.x * movementForce, movementInput.y * movementForce);
        rb.AddForce(movement);

        // Clamp velocity
        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -maxVelocity.x, maxVelocity.x),
            Mathf.Clamp(rb.linearVelocity.y, -maxVelocity.y, maxVelocity.y)
        );

        if (maxVelocity.x == maxVelocity.y)
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity.x);
    }

    // Freeze controls externally
    public void Freeze()
    {
        frozen = true;
        rb.linearVelocity = Vector2.zero;
    }

    public void UnFreeze()
    {
        frozen = false;
    }

    // Example of custom function to toggle two direction mode
    public void LadderMode(bool ladderOn)
    {
        if (ladderOn)
        {
            twoDirection = false;
            rb.gravityScale = 0;
        }
        else
        {
            twoDirection = true;

            if (!isGrounded)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
