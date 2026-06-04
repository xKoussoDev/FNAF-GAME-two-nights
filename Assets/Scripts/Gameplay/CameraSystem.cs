using UnityEngine;
using System.Collections;


public class CameraSystem : MonoBehaviour
{
    public static CameraSystem Instance { get; private set; }

    // ─ Inspector
    [Header("Dependencies")]
    [Tooltip("Drag the CameraFeedManager component here.")]
    [SerializeField] CameraFeedManager feedManager;

    [Header("UI Panels")]
    [Tooltip("The entire camera monitor UI (RawImage feed, camera name, etc.).")]
    [SerializeField] GameObject cameraUI;

    [Tooltip("The normal office HUD (power, time, door buttons, etc.).")]
    [SerializeField] GameObject officeUI;

    // ─ State
    public bool CamerasOpen  { get; private set; } = false;
    public int CurrentCam { get; private set; } = 0;

    public int TotalCams => feedManager != null ? feedManager.CameraCount : 7;

    static readonly string[] CameraNames =
    {
        "CAM 1 – Stage",
        "CAM 2 – Dining Area",
        "CAM 3 – Backstage",
        "CAM 4 – Left Hall",
        "CAM 5 – Right Hall",
        "CAM 6 – Kitchen",
        "CAM 7 – Pirate Cove"
    };

    bool _inputBlocked;

    // ─ Lifecycle

    void Awake() => Instance = this;

    void Start() => SetCameraUI(false);

    void Update()
    {
        if (_inputBlocked) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (PowerManager.Instance != null && PowerManager.Instance.IsPowerOut) return;

        // Toggle monitor
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CamerasOpen) CloseCamera();
            else             OpenCamera();
        }

        if (CamerasOpen)
        {
            bool next = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            bool prev = Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A);
            if (next) StartCoroutine(SwitchFeed(+1));
            if (prev) StartCoroutine(SwitchFeed(-1));
        }
    }

    // ─ Public API

    public void OpenCamera()
    {
        CamerasOpen = true;
        SetCameraUI(true);
        OfficeLookController.Instance?.SetEnabled(false);
        PowerManager.Instance?.SetCamerasOpen(true);
        feedManager?.ShowFeed(CurrentCam);
        UIManager.Instance?.SetCameraName(CameraNames[CurrentCam]);
        AudioManager.Instance?.PlayCameraOpen();
        AudioManager.Instance?.StartCameraStatic();
    }

    public void CloseCamera()
    {
        CamerasOpen = false;
        SetCameraUI(false);
        OfficeLookController.Instance?.SetEnabled(true);
        PowerManager.Instance?.SetCamerasOpen(false);
        AudioManager.Instance?.StopCameraStatic();
    }

    public void ForceClose()
    {
        CamerasOpen = false;
        SetCameraUI(false);
        PowerManager.Instance?.SetCamerasOpen(false);
    }

    // ─ Internal

    IEnumerator SwitchFeed(int direction)
    {
        _inputBlocked = true;

        UIManager.Instance?.TriggerCameraGlitch();
        yield return new WaitForSeconds(0.18f);

        CurrentCam = (CurrentCam + direction + TotalCams) % TotalCams;
        feedManager?.ShowFeed(CurrentCam);
        UIManager.Instance?.SetCameraName(CameraNames[CurrentCam]);

        _inputBlocked = false;
    }

    void SetCameraUI(bool show)
    {
        if (cameraUI != null) cameraUI.SetActive(show);
        if (officeUI != null) officeUI.SetActive(!show);
    }
}
