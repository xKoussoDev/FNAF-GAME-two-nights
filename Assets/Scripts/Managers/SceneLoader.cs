using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    // ─ Singleton
    public static SceneLoader Instance { get; private set; }

    [Header("Transition")]
    [SerializeField] float fadeDuration = 0.6f;

    // ─ Runtime state
    Image _fadeImage;
    bool _isFading;

    // ─ Lifecycle
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildFadeCanvas();
    }

    void Start()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    // ─ Canvas builder

    void BuildFadeCanvas()
    {
        var canvasGO = new GameObject("[FadeCanvas]");
        canvasGO.transform.SetParent(transform);
        DontDestroyOnLoad(canvasGO);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        _fadeImage = panelGO.AddComponent<Image>();
        _fadeImage.color = Color.black;
        _fadeImage.raycastTarget = false;

        var rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        SetAlpha(1f);
    }

    // ─ Public API

    public void FadeToScene(string sceneName, float preDelay = 0f)
    {
        if (!_isFading)
            StartCoroutine(TransitionRoutine(sceneName, preDelay));
    }

    // ─ Internal

    IEnumerator TransitionRoutine(string sceneName, float preDelay)
    {
        _isFading = true;

        if (preDelay > 0f)
            yield return new WaitForSecondsRealtime(preDelay);

        yield return StartCoroutine(Fade(0f, 1f));
        AudioManager.Instance?.StopAll();
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return StartCoroutine(Fade(1f, 0f));
        _isFading = false;
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / fadeDuration));
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        if (_fadeImage == null) return;
        var c = _fadeImage.color;
        c.a = a;
        _fadeImage.color = c;
    }
}
