using UnityEngine;
using UnityEngine.InputSystem;

public class Simple3DInput : MonoBehaviour
{
    public InputAction MoveAction;
    private Rigidbody rb;


    public float moveSpeed = 5f;

    private void Start()
    {
        // Freeze X and Z rotation so it can't fall over
        rb.freezeRotation = true;
        MoveAction.Enable();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 moveInput = MoveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 newPosition = rb.position + move * moveSpeed * Time.deltaTime;

        rb.MovePosition(newPosition);
    }

    private void OnDisable()
    {
        MoveAction.Disable();
    }
}