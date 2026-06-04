using UnityEngine;
using System;


public class PowerManager : MonoBehaviour
{
    public static PowerManager Instance { get; private set; }

    public static event Action OnPowerDepleted;

    // ─ Inspector
    [Header("Power Settings")]
    [Tooltip("Starting power percentage (100 = full power).")]
    [SerializeField] float startingPower = 100f;

    [Header("Drain Rates  (% per second)")]
    [Tooltip("Constant passive drain even with nothing active.")]
    [SerializeField] float baseDrain = 0.15f;

    [Tooltip("Extra drain per closed door.")]
    [SerializeField] float doorDrain = 0.10f;

    [Tooltip("Extra drain per active hallway light.")]
    [SerializeField] float lightDrain = 0.05f;

    [Tooltip("Extra drain while camera monitor is open.")]
    [SerializeField] float cameraDrain = 0.08f;

    // ─ State
    public float CurrentPower { get; private set; }
    public bool IsPowerOut { get; private set; } = false;
    public int ActiveConsumers { get; private set; }

    int  _closedDoors = 0;
    int  _activeLights = 0;
    bool _camerasOpen = false;

    // ─ Lifecycle

    void Awake()
    {
        Instance = this;
        CurrentPower = startingPower;
    }

    void Update()
    {
        if (IsPowerOut) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        // Calculate total drain this frame
        float drain = baseDrain;
        drain += _closedDoors * doorDrain;
        drain += _activeLights * lightDrain;
        if (_camerasOpen) drain += cameraDrain;

        CurrentPower    -= drain * Time.deltaTime;
        ActiveConsumers  = _closedDoors + _activeLights + (_camerasOpen ? 1 : 0);

        if (CurrentPower <= 0f)
        {
            CurrentPower = 0f;
            TriggerPowerOut();
        }
    }

    // ─ Registration API

    public void RegisterDoorClosed() => _closedDoors++;
    public void UnregisterDoorClosed() => _closedDoors  = Mathf.Max(0, _closedDoors  - 1);
    public void RegisterLightOn() => _activeLights++;
    public void UnregisterLightOn() => _activeLights = Mathf.Max(0, _activeLights - 1);
    public void SetCamerasOpen(bool v) => _camerasOpen = v;

    // ─ Internal

    void TriggerPowerOut()
    {
        IsPowerOut = true;
        OnPowerDepleted?.Invoke();
        GameManager.Instance?.TriggerPowerOut();
        AudioManager.Instance?.PlayPowerOutAmbience();
    }
}
