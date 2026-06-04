using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroController : MonoBehaviour
{
    [Header("Efecto de Parpadeo")]
    [SerializeField] Image blinkPanel;
    [SerializeField] int blinkCount = 3;
    [SerializeField] float blinkSpeed = 3f;
    [SerializeField] float eyeOpenDuration = 0.3f;

    [Header("Llamada Telefónica")]
    [SerializeField] AudioSource phoneCallSource;
    [SerializeField] float fallbackCallDuration = 201f;

    [Header("Subtítulos")]
    [SerializeField] GameObject subtitlesPanel;
    [SerializeField] TextMeshProUGUI subtitleText;
    [SerializeField] bool showSubtitles = true;
    [SerializeField] SubtitleLine[] subtitleLines;

    [Header("Botón de Saltar")]
    [SerializeField] Button skipButton;
    [SerializeField] GameObject skipButtonObject;

    [Header("Transición")]
    [SerializeField] float postCallDelay = 1.5f;

    bool _skipped = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OfficeLookController.Instance?.SetEnabled(false);

        if (subtitlesPanel != null) subtitlesPanel.SetActive(false);
        if (skipButtonObject != null) skipButtonObject.SetActive(false);

        skipButton?.onClick.AddListener(SkipIntro);

        SetBlinkAlpha(1f);
        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SkipIntro();
    }

    void SkipIntro()
    {
        if (_skipped) return;
        _skipped = true;

        StopAllCoroutines();

        if (phoneCallSource != null && phoneCallSource.isPlaying)
            phoneCallSource.Stop();

        if (subtitlesPanel != null) subtitlesPanel.SetActive(false);
        if (skipButtonObject != null) skipButtonObject.SetActive(false);

        StartCoroutine(GoToNight1());
    }

    IEnumerator GoToNight1()
    {
        yield return StartCoroutine(FadePanel(0f, 1f, 0.5f));

        if (GameManager.Instance != null)
            GameManager.Instance.StartNight(1);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Night1");
    }

    IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(BlinkSequence());
        yield return StartCoroutine(FadePanel(1f, 0f, 1.2f));

        float callLength = fallbackCallDuration;
        if (phoneCallSource != null && phoneCallSource.clip != null)
        {
            callLength = phoneCallSource.clip.length;
            phoneCallSource.Play();
        }

        if (skipButtonObject != null) skipButtonObject.SetActive(true);

        if (showSubtitles && subtitleLines != null && subtitleLines.Length > 0)
            StartCoroutine(PlaySubtitles());

        float elapsed    = 3f;
        float remaining  = Mathf.Max(0f, callLength - elapsed + postCallDelay);
        yield return new WaitForSeconds(remaining);

        if (skipButtonObject != null) skipButtonObject.SetActive(false);

        if (!_skipped)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartNight(1);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("Night1");
        }
    }

    IEnumerator BlinkSequence()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            yield return StartCoroutine(FadePanel(1f, 0f, 1f / blinkSpeed));
            yield return new WaitForSeconds(eyeOpenDuration + i * 0.15f);
            if (i < blinkCount - 1)
                yield return StartCoroutine(FadePanel(0f, 1f, 1f / blinkSpeed));
        }
    }

    IEnumerator FadePanel(float from, float to, float duration)
    {
        if (blinkPanel == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetBlinkAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetBlinkAlpha(to);
    }

    void SetBlinkAlpha(float alpha)
    {
        if (blinkPanel == null) return;
        var c = blinkPanel.color; c.a = alpha; blinkPanel.color = c;
    }

    IEnumerator PlaySubtitles()
    {
        if (subtitlesPanel == null || subtitleText == null) yield break;
        subtitlesPanel.SetActive(true);

        float start = Time.time;
        foreach (var line in subtitleLines)
        {
            float wait = line.startTime - (Time.time - start);
            if (wait > 0f) yield return new WaitForSeconds(wait);
            if (_skipped) yield break;
            subtitleText.text = line.text;
        }

        yield return new WaitForSeconds(2f);
        subtitlesPanel.SetActive(false);
    }
}

[System.Serializable]
public class SubtitleLine
{
    [Tooltip("Segundos desde el inicio del audio.")]
    public float startTime;
    [TextArea(1, 4)]
    public string text;
}
