using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;

    [SerializeField] private float jumpForce; // сила прыжка
    [SerializeField] private float jumpHoldTime = 0.4f; // окно времени, в которое игрок может отпустить кнопку для низкого прыжка
    [SerializeField] private float jumpHoldMultiplier = 0.2f; // на сколько уменьшить скорость, если отпустить рано
    [SerializeField] private int maxJumps = 2; // макс. кол-во прыжков

    private PlayerActions playerActions;
    private InputAction move;
    private InputAction run;
    private InputAction jump;
    private InputAction crouch;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    private bool IsCrouching;

    private int jumpsRemaining; // сколько прыжков осталось
    private bool isJumpHolding; // зажата ли кнопка прыжка
    private float jumpHoldTimer; // сколько времени зажат прыжок



    void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out rb)) Debug.LogError("Rigidbody2D не найден у " + gameObject.name);
        if (!TryGetComponent<BoxCollider2D>(out boxCollider)) Debug.LogError("Collider2D не найден у " + gameObject.name);
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

        crouch = playerActions.PlayerController.Crouch;
        crouch.Enable();
    }

    void Update()
    {
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
        Vector2 axis = move.ReadValue<Vector2>();

        if (axis.x != 0)
        {
            if (run.IsPressed()) Run(axis);
            else Move(axis);
        }
        else if (crouch.IsPressed() && !IsCrouching) EnterCrouch(boxCollider);
        else if (!crouch.IsPressed() && IsCrouching) ExitCrouch(boxCollider);
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void Move(Vector2 axis) => rb.linearVelocity = new Vector2(axis.x * walkSpeed, rb.linearVelocity.y);

    private void Run(Vector2 axis) => rb.linearVelocity = new Vector2(axis.x * runSpeed, rb.linearVelocity.y);

    private void EnterCrouch(BoxCollider2D coll)
    {
        if (coll == null) return;
        else
        {
            IsCrouching = true;
            Vector2 newSize = coll.size;
            newSize.y = newSize.y / 2;
            coll.size = newSize;


        }
    }

    private void ExitCrouch(BoxCollider2D coll)
    {
        if(coll == null) return;
        else
        {
            IsCrouching = false;
            Vector2 newSize = coll.size;
            newSize.y = newSize.y * 2;
            coll.size = newSize;
        }
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpsRemaining = maxJumps;
            isJumpHolding = false;
        }
    }
}
