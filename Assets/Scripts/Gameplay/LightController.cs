using UnityEngine;
using UnityEngine.UI;

public class LightController : MonoBehaviour
{
    // ─ Inspector
    [Header("Identity")]
    [SerializeField] DoorSide side;

    [Header("Keyboard Control")]
    [Tooltip("Z for left light, C for right light.")]
    [SerializeField] KeyCode toggleKey = KeyCode.Z;

    [Header("Hallway Light")]
    [Tooltip("The 3D Light component placed in the hallway.")]
    [SerializeField] Light hallwayLight;

    [Header("Optional UI")]
    [Tooltip("On-screen button that also toggles the light.")]
    [SerializeField] Button lightButton;

    [Tooltip("Small indicator Image that shows light state.")]
    [SerializeField] Image indicatorImage;
    [SerializeField] Color onColor = new Color(1f, 0.85f, 0f);
    [SerializeField] Color offColor = new Color(0.3f, 0.3f, 0.3f);

    // ─ State
    public bool     IsOn { get; private set; } = false;
    public DoorSide Side => side;

    // ─ Lifecycle

    void Start()
    {
        lightButton?.onClick.AddListener(Toggle);
        SetLight(false);
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (PowerManager.Instance != null && PowerManager.Instance.IsPowerOut) return;
        if (CameraSystem.Instance != null && CameraSystem.Instance.CamerasOpen) return;

        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    // ─ Public API

    public void Toggle() => SetLight(!IsOn);

    public void SetLight(bool on)
    {
        bool wasOn = IsOn;
        IsOn = on;

        if (hallwayLight != null)
            hallwayLight.enabled = IsOn;

        if (IsOn && !wasOn)
        {
            PowerManager.Instance?.RegisterLightOn();
            AudioManager.Instance?.PlayLightSwitch();
        }
        else if (!IsOn && wasOn)
        {
            PowerManager.Instance?.UnregisterLightOn();
            AudioManager.Instance?.PlayLightSwitch();
        }

        UpdateVisuals();
    }

    public void ForceOff() => SetLight(false);

    // ─ Internal

    void UpdateVisuals()
    {
        if (indicatorImage != null)
            indicatorImage.color = IsOn ? onColor : offColor;
    }
}
