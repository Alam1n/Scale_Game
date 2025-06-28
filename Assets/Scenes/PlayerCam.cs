using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensX = 50f;
    public float sensY = 50f;

    [Header("Smoothing")]
    public float smoothTime = 0.05f;

    [Header("References")]
    public Transform orientation;

    private float targetXRotation;
    private float targetYRotation;
    private float currentXRotation;
    private float currentYRotation;
    private float xVelocity;
    private float yVelocity;

    private Vector2 mouseDelta;
    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Look.performed += ctx => mouseDelta = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => mouseDelta = Vector2.zero;
    }

    void OnDisable()
    {
        inputActions.Player.Look.performed -= ctx => mouseDelta = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled -= ctx => mouseDelta = Vector2.zero;
        inputActions.Player.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentXRotation = transform.eulerAngles.x;
        currentYRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        // Get target rotation based on raw input
        targetYRotation += mouseDelta.x * sensX * Time.deltaTime;
        targetXRotation -= mouseDelta.y * sensY * Time.deltaTime;
        targetXRotation = Mathf.Clamp(targetXRotation, -90f, 90f);

        // Smooth damp rotation over time
        currentXRotation = Mathf.SmoothDampAngle(currentXRotation, targetXRotation, ref xVelocity, smoothTime);
        currentYRotation = Mathf.SmoothDampAngle(currentYRotation, targetYRotation, ref yVelocity, smoothTime);

        // Apply rotation
        transform.rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }
}
