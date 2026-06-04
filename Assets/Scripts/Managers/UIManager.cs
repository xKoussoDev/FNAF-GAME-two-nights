using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ─ Power HUD
    [Header("Power HUD")]
    [Tooltip("TextMeshPro showing 'Power: 75%'.")]
    [SerializeField] TextMeshProUGUI powerText;

    [Tooltip("Small bar images (5 bars). Enable/disable based on usage.")]
    [SerializeField] Image[] usageBars;

    // ─ Time / Night HUD
    [Header("Time HUD")]
    [Tooltip("TextMeshPro showing '3 AM'.")]
    [SerializeField] TextMeshProUGUI hourText;

    [Tooltip("TextMeshPro showing 'Night 1'.")]
    [SerializeField] TextMeshProUGUI nightText;

    // ─ Camera HUD
    [Header("Camera HUD")]
    [Tooltip("TextMeshPro showing the current camera name.")]
    [SerializeField] TextMeshProUGUI cameraNameText;

    [Tooltip("Brief white/static flash when switching cameras.")]
    [SerializeField] Image cameraGlitchOverlay;

    // ─ Power-Out Overlay
    [Header("Power-Out Overlay")]
    [Tooltip("Dark red overlay panel shown when power runs out.")]
    [SerializeField] GameObject powerOutOverlay;

    // ─ Settings
    [Header("Settings")]
    [SerializeField] float glitchFlashDuration = 0.15f;

    // ─ Lifecycle

    void Awake() => Instance = this;

    void OnEnable()
    {
        TimeManager.OnHourChanged += OnHourChanged;
        PowerManager.OnPowerDepleted += OnPowerDepleted;
    }

    void OnDisable()
    {
        TimeManager.OnHourChanged -= OnHourChanged;
        PowerManager.OnPowerDepleted -= OnPowerDepleted;
    }

    void Start()
    {
        if (nightText != null)
            nightText.text = $"Night {GameManager.Instance?.CurrentNight ?? 1}";

        if (hourText != null)
            hourText.text = "12 AM";

        if (cameraGlitchOverlay != null)
            cameraGlitchOverlay.enabled = false;

        if (powerOutOverlay != null)
            powerOutOverlay.SetActive(false);
    }

    void Update()
    {
        if (PowerManager.Instance == null) return;

        float pwr = Mathf.CeilToInt(PowerManager.Instance.CurrentPower);
        if (powerText != null)
            powerText.text = $"Power: {pwr:0}%";

        UpdateUsageBars(PowerManager.Instance.ActiveConsumers);
    }

    // ─ Public API

    public void SetCameraName(string name)
    {
        if (cameraNameText != null)
            cameraNameText.text = name;
    }

    public void TriggerCameraGlitch()
    {
        StartCoroutine(GlitchFlash());
    }

    public void ShowPowerOutOverlay(bool show)
    {
        if (powerOutOverlay != null)
            powerOutOverlay.SetActive(show);
    }

    // ─ Event Handlers

    void OnHourChanged(int hour)
    {
        if (hourText != null)
            hourText.text = TimeManager.Instance?.CurrentHourLabel ?? "";
    }

    void OnPowerDepleted()
    {
        if (powerOutOverlay != null)
            powerOutOverlay.SetActive(true);
    }

    // ─ Internal

    void UpdateUsageBars(int active)
    {
        if (usageBars == null) return;
        for (int i = 0; i < usageBars.Length; i++)
        {
            if (usageBars[i] != null)
                usageBars[i].enabled = i < active;
        }
    }

    IEnumerator GlitchFlash()
    {
        if (cameraGlitchOverlay == null) yield break;
        cameraGlitchOverlay.enabled = true;
        yield return new WaitForSeconds(glitchFlashDuration);
        cameraGlitchOverlay.enabled = false;
    }
}
