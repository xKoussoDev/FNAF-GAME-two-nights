using UnityEngine;

public class OfficeLookController : MonoBehaviour
{
    public static OfficeLookController Instance { get; private set; }

    // ─ Inspector
    [Header("Sensitivity")]
    [SerializeField] float mouseSensitivity = 2.5f;

    [Header("Rotation Limits  (degrees)")]
    [SerializeField] float horizontalLimit = 60f;
    [Tooltip("How far down the player can look.")]
    [SerializeField] float verticalLimitDown = 10f;
    [Tooltip("How far up the player can look. Set to 0 to block looking up entirely.")]
    [SerializeField] float verticalLimitUp = 0f;

    [Header("Smoothing")]
    [Tooltip("Lower = snappier. Higher = floatier.")]
    [SerializeField] float smoothTime = 0.06f;

    // ── State
    public bool IsEnabled { get; private set; } = true;

    float _targetYaw;
    float _targetPitch;
    float _currentYaw;
    float _currentPitch;
    float _yawVelocity;
    float _pitchVelocity;
    float _baseYaw;
    float _basePitch;

    // ─ Lifecycle

    void Awake()
    {
        Instance = this;
        _baseYaw = transform.localEulerAngles.y;
        _basePitch = transform.localEulerAngles.x;
        LockCursor();
    }

    void Update()
    {
        if (!IsEnabled) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        _targetYaw = Mathf.Clamp(_targetYaw + mouseX, -horizontalLimit, horizontalLimit);
        _targetPitch = Mathf.Clamp(_targetPitch + mouseY, -verticalLimitUp, verticalLimitDown);

        _currentYaw = Mathf.SmoothDamp(_currentYaw, _targetYaw, ref _yawVelocity, smoothTime);
        _currentPitch = Mathf.SmoothDamp(_currentPitch, _targetPitch, ref _pitchVelocity, smoothTime);

        transform.localRotation = Quaternion.Euler(_basePitch + _currentPitch, _baseYaw + _currentYaw, 0f);
    }

    // ─ Public API

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        if (enabled)
            LockCursor();
        else
            FreeCursor();
    }

    // ─ Internal

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FreeCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
