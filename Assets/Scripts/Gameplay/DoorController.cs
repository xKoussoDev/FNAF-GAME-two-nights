using UnityEngine;
using UnityEngine.UI;


public class DoorController : MonoBehaviour
{
    // ─ Inspector
    [Header("Identity")]
    [SerializeField] DoorSide side;

    [Header("Keyboard Control")]
    [Tooltip("Q for left door, E for right door.")]
    [SerializeField] KeyCode toggleKey = KeyCode.Q;

    [Header("Door Models")]
    [Tooltip("GameObject visible when the door is OPEN (active by default).")]
    [SerializeField] GameObject doorOpenModel;

    [Tooltip("GameObject visible when the door is CLOSED (inactive by default).")]
    [SerializeField] GameObject doorClosedModel;

    [Header("Optional UI")]
    [Tooltip("On-screen button that also toggles the door.")]
    [SerializeField] Button doorButton;

    [Tooltip("Small indicator light Image that changes colour.")]
    [SerializeField] Image indicatorLight;
    [SerializeField] Color closedColor = new Color(0.9f, 0.1f, 0.1f);
    [SerializeField] Color openColor = new Color(0.1f, 0.5f, 0.1f);

    // ─ State
    public bool IsClosed { get; private set; } = false;
    public DoorSide Side => side;

    // ─ Lifecycle 

    void Start()
    {
        doorButton?.onClick.AddListener(Toggle);
        UpdateVisuals();
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

    public void Toggle() => SetDoor(!IsClosed);

    public void SetDoor(bool closed)
    {
        bool wasClose = IsClosed;
        IsClosed = closed;

        if (IsClosed && !wasClose)
        {
            PowerManager.Instance?.RegisterDoorClosed();
            AudioManager.Instance?.PlayDoorClose();
        }
        else if (!IsClosed && wasClose)
        {
            PowerManager.Instance?.UnregisterDoorClosed();
            AudioManager.Instance?.PlayDoorOpen();
        }

        UpdateVisuals();
    }

    public void ForceOpen() => SetDoor(false);

    // ─ Internal

    void UpdateVisuals()
    {
        if (doorOpenModel != null) doorOpenModel.SetActive(!IsClosed);
        if (doorClosedModel != null) doorClosedModel.SetActive(IsClosed);

        if (indicatorLight != null)
            indicatorLight.color = IsClosed ? closedColor : openColor;
    }
}
