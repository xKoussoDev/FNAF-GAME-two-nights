using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VictoryController : MonoBehaviour
{
    // ─ Inspector
    [Header("Buttons")]
    [SerializeField] Button mainMenuButton;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI congratsText;
    [SerializeField] string congratsString = "YOU SURVIVED\nTWO NIGHTS";

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip   victoryClip;

    // ─ Lifecycle

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        mainMenuButton?.onClick.AddListener(OnMainMenu);

        if (congratsText != null)
            congratsText.text = congratsString;

        if (audioSource != null && victoryClip != null)
        {
            audioSource.clip = victoryClip;
            audioSource.loop = false;
            audioSource.volume = 0.8f;
            audioSource.Play();
        }
        else
        {
            AudioManager.Instance?.PlayVictoryAmbience();
        }
    }

    // ─ Button handler

    void OnMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
