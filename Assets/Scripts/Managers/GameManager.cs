using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // ─ Singleton 
    public static GameManager Instance { get; private set; }

    // ─ Events
    public static event Action<int> OnNightStart;
    public static event Action OnGameOver;
    public static event Action OnVictory;
    public static event Action OnPowerOut;

    // ─ Read-only State
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public int CurrentNight { get; private set; } = 1;
    public bool IsGameActive { get; private set; } = false;
    public bool IsAlive { get; private set; } = true;

    // ─ Scene Name Settings
    [Header("Scene Names – must match Build Settings exactly")]
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] string introScene = "IntroScene";
    [SerializeField] string night1Scene = "Night1";
    [SerializeField] string night2Scene = "Night2";
    [SerializeField] string gameOverScene = "GameOver";
    [SerializeField] string victoryScene = "Victory";

    // ─ Lifecycle
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─ Public API
    public void StartIntro()
    {
        CurrentState = GameState.Intro;
        SceneLoader.Instance?.FadeToScene(introScene);
    }

    public void StartNight(int nightNumber)
    {
        CurrentNight = nightNumber;
        IsAlive = true;
        IsGameActive = true;
        CurrentState = nightNumber == 1 ? GameState.Night1 : GameState.Night2;

        OnNightStart?.Invoke(nightNumber);
        AudioManager.Instance?.PlayOfficeAmbience();
        string scene = nightNumber == 1 ? night1Scene : night2Scene;
        SceneLoader.Instance?.FadeToScene(scene);
    }

    public void NightComplete()
    {
        IsGameActive = false;

        if (CurrentNight == 1)
            StartNight(2);
        else
            TriggerVictory();
    }

    public void TriggerGameOver()
    {
        if (!IsAlive) return;
        IsAlive = false;
        IsGameActive = false;
        CurrentState = GameState.GameOver;
        OnGameOver?.Invoke();
        AudioManager.Instance?.StopAllIncludingAmbience();
        SceneLoader.Instance?.FadeToScene(gameOverScene, 2.5f);
    }

    public void TriggerVictory()
    {
        IsAlive = false;
        IsGameActive = false;
        CurrentState = GameState.Victory;
        OnVictory?.Invoke();
        AudioManager.Instance?.StopAllIncludingAmbience();
        SceneLoader.Instance?.FadeToScene(victoryScene, 1.5f);
    }

    public void TriggerPowerOut() => OnPowerOut?.Invoke();

    public void GoToMainMenu()
    {
        IsGameActive = false;
        CurrentState = GameState.MainMenu;
        AudioManager.Instance?.StopAllIncludingAmbience();
        SceneLoader.Instance?.FadeToScene(mainMenuScene);
    }

    public void RetryNight() => StartNight(CurrentNight);
}
