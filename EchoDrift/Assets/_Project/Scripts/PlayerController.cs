using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [SerializeField] private float jumpForce; // сила прыжка
    [SerializeField] private float jumpHoldTime = 0.4f; // окно времени, в которое игрок может отпустить кнопку для низкого прыжка
    [SerializeField] private float jumpHoldMultiplier = 0.2f; // на сколько уменьшить скорость, если отпустить рано
    [SerializeField] private int maxJumps = 2; // макс. кол-во прыжков

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;



    private PlayerActions playerActions;
    private InputAction move;
    private InputAction run;
    private InputAction jump;
    private InputAction dash;
    private Rigidbody2D rb;

    private int jumpsRemaining; // сколько прыжков осталось
    private bool isJumpHolding; // зажата ли кнопка прыжка
    private float jumpHoldTimer; // сколько времени зажат прыжок

    private bool canDash = true;
    private bool isDashing = false;



    void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out rb)) Debug.LogError("Rigidbody2D не найден у " + gameObject.name);
    }


    void Start()
    {
        playerActions = new PlayerActions();

        move = playerActions.PlayerController.Move;
        move.Enable();

        run = playerActions.PlayerController.Run;
        run.Enable();

        jump = playerActions.PlayerController.Jump;
        jump.Enable();

        dash = playerActions.PlayerController.Dash;
        dash.Enable();
    }

    void Update()
    {
        if (dash.triggered && canDash && !isDashing) StartCoroutine(Dash());

        // НАЧАЛО ПРЫЖКА (когда игрок нажал кнопку)
        if (jump.triggered)
        {
            if(jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
                isJumpHolding = true;
                jumpHoldTimer = 0f;
            }
        }

        // УДЕРЖАНИЕ ПРЫЖКА (пока зажата кнопка)
        if(jump.IsPressed() && isJumpHolding)
        {
            jumpHoldTimer += Time.deltaTime;
            if(jumpHoldTimer >= jumpHoldTime)
            {
                isJumpHolding = false;
            }
        }

        // ОТПУСКАНИЕ ПРЫЖКА (если игрок отпустил кнопку раньше)
        if(jump.WasReleasedThisFrame()  && isJumpHolding)
        {
            // уменьшение вертикальной скорости
            Vector2 velocity = rb.linearVelocity;
            velocity.y *= jumpHoldMultiplier; //!
            rb.linearVelocity = velocity;
            isJumpHolding = false;
        }

        
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        Vector2 axis = move.ReadValue<Vector2>();

        if (axis.x != 0)
        {
            if (run.IsPressed()) Run(axis);
            else Move(axis);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Move(Vector2 axis) => rb.linearVelocity = new Vector2(axis.x * walkSpeed, rb.linearVelocity.y);

    private void Run(Vector2 axis) => rb.linearVelocity = new Vector2(axis.x * runSpeed, rb.linearVelocity.y);

    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpsRemaining = maxJumps;
            isJumpHolding = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            jumpsRemaining = maxJumps;
        }
    }

    private IEnumerator Dash()
    {
        Debug.Log("DASH");
        isDashing = true;
        canDash = false;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float direction = move.ReadValue<Vector2>().x;

        if(direction == 0)
        {
            direction = transform.localScale.x > 0 ? 1 : -1;
        }

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
