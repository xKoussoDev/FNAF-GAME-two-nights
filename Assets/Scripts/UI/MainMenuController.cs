using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    // ─ Inspector
    [Header("Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button quitButton;

    [Header("Title Text")]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] string titleString = "NIGHT SHIFT";

    [Header("Ambience")]
    [Tooltip("AudioSource in this scene for menu background audio.")]
    [SerializeField] AudioSource ambienceSource;
    [SerializeField] AudioClip menuAmbienceClip;

    // ─ Lifecycle

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (titleText != null)
            titleText.text = titleString;

        playButton?.onClick.AddListener(OnPlay);
        quitButton?.onClick.AddListener(OnQuit);

        if (ambienceSource != null && menuAmbienceClip != null)
        {
            ambienceSource.clip = menuAmbienceClip;
            ambienceSource.loop = true;
            ambienceSource.volume = 0.5f;
            ambienceSource.Play();
        }
        else
        {
            AudioManager.Instance?.PlayMenuAmbience();
        }
    }

    // ─ Button handlers

    void OnPlay()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartIntro();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
