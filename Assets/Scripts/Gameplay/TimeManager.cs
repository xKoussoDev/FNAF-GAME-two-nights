using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public static event Action<int> OnHourChanged;

    public static event Action OnDawnReached;

    // ─ Inspector
    [Header("Night Duration")]
    [Tooltip("Total real-time seconds for a full night. Default: 360 (6 minutes).")]
    [SerializeField] float nightDurationSeconds = 360f;

    // ─ State

    public int CurrentHour { get; private set; } = 0;

    public float NormalizedTime { get; private set; } = 0f;
    public string CurrentHourLabel => _hourLabels[Mathf.Clamp(CurrentHour, 0, 6)];

    static readonly string[] _hourLabels =
        { "12 AM", "1 AM", "2 AM", "3 AM", "4 AM", "5 AM", "6 AM" };

    float _elapsed;
    bool  _nightOver;

    // ─ Lifecycle

    void Awake() => Instance = this;

    void Update()
    {
        if (_nightOver) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (PowerManager.Instance != null && PowerManager.Instance.IsPowerOut) return;

        _elapsed += Time.deltaTime;
        NormalizedTime  = Mathf.Clamp01(_elapsed / nightDurationSeconds);

        int newHour = Mathf.FloorToInt(NormalizedTime * 6f);
        if (newHour != CurrentHour)
        {
            CurrentHour = newHour;
            OnHourChanged?.Invoke(CurrentHour);
        }

        // Reach 6 AM
        if (_elapsed >= nightDurationSeconds)
        {
            _nightOver = true;
            CurrentHour = 6;
            OnHourChanged?.Invoke(6);
            OnDawnReached?.Invoke();

            MonsterManager.Instance?.DeactivateAll();

            if (GameManager.Instance != null)
                GameManager.Instance.NightComplete();
            else
                Debug.Log("[TimeManager] 6 AM – GameManager no encontrado. Inicia desde MainMenu para cambiar de escena.");
        }
    }
}
