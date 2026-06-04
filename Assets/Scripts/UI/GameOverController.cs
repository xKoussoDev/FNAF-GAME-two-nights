using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverController : MonoBehaviour
{
    // ─ Inspector
    [Header("Buttons")]
    [SerializeField] Button retryButton;
    [SerializeField] Button mainMenuButton;

    [Header("UI Text")]
    [Tooltip("Optional label showing which night the player failed.")]
    [SerializeField] TextMeshProUGUI nightLabel;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip   horrorSting;

    // ─ Lifecycle

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        retryButton?.onClick.AddListener(OnRetry);
        mainMenuButton?.onClick.AddListener(OnMainMenu);

        if (nightLabel != null && GameManager.Instance != null)
            nightLabel.text = $"Night {GameManager.Instance.CurrentNight}";

        if (audioSource != null && horrorSting != null)
            audioSource.PlayOneShot(horrorSting);
    }

    // ─ Button handlers

    void OnRetry()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RetryNight();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Night1");
    }

    void OnMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
