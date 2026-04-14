using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    private PlayerActions playerActions;
    private InputAction move;
    private InputAction run;
    private Rigidbody2D rb;


    void Start()
    {
        playerActions = new PlayerActions();
        move = playerActions.PlayerController.Move;
        move.Enable();
        run = playerActions.PlayerController.Run;
        run.Enable();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 axis = move.ReadValue<Vector2>();
        OnMove(axis);

        if (run.IsPressed()) Run(axis);
    }

    private void OnMove(Vector2 axis)
    {
        rb.linearVelocity = new Vector2(axis.x * walkSpeed, rb.linearVelocity.y);
    }

    private void Run(Vector2 axis)
    {
        rb.linearVelocity = new Vector2(axis.x * runSpeed, rb.linearVelocity.y);
        Debug.Log("Running");
    }
}
