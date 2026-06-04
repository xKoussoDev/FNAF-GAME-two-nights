using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class VHSOverlayController : MonoBehaviour
{
    // ─ Inspector
    [Header("Overlay Layers")]
    [Tooltip("RawImage with a tiling horizontal-line texture.")]
    [SerializeField] RawImage scanlinesLayer;

    [Tooltip("RawImage with a tiling noise/static texture.")]
    [SerializeField] RawImage noiseLayer;

    [Tooltip("Image with a vignette (dark edges) sprite.")]
    [SerializeField] Image vignetteLayer;

    [Tooltip("Image used for the glitch flash. Normally invisible.")]
    [SerializeField] Image glitchLayer;

    [Header("Scanlines")]
    [SerializeField] float scanlinesScrollSpeed = 0.25f;
    [SerializeField] [Range(0f, 1f)] float scanlinesAlpha = 0.12f;

    [Header("Noise")]
    [SerializeField] float noiseScrollSpeed = 1.5f;
    [SerializeField] [Range(0f, 1f)] float noiseAlpha = 0.04f;

    [Header("Vignette")]
    [SerializeField] [Range(0f, 1f)] float vignetteAlpha = 0.55f;

    [Header("Glitch")]
    [SerializeField] float glitchMinInterval = 6f;
    [SerializeField] float glitchMaxInterval = 22f;
    [SerializeField] float glitchDuration = 0.08f;

    // ─ Internal
    float _glitchTimer;
    float _nextGlitch;
    bool  _isNight2;

    // ─ Lifecycle

    void Start()
    {
        _isNight2 = GameManager.Instance?.CurrentNight == 2;

        SetAlpha(scanlinesLayer, scanlinesAlpha);
        SetAlpha(noiseLayer, noiseAlpha);
        SetAlpha(vignetteLayer, vignetteAlpha);
        if (glitchLayer != null) glitchLayer.enabled = false;

        ScheduleNextGlitch();
    }

    void Update()
    {
        ScrollScanlines();
        DriftNoise();
        TickGlitch();
    }

    // ─ Scroll

    void ScrollScanlines()
    {
        if (scanlinesLayer == null) return;
        Rect r = scanlinesLayer.uvRect;
        r.y += scanlinesScrollSpeed * Time.deltaTime;
        scanlinesLayer.uvRect = r;
    }

    void DriftNoise()
    {
        if (noiseLayer == null) return;
        Rect r = noiseLayer.uvRect;
        r.x += noiseScrollSpeed * Time.deltaTime * Random.Range(0.9f, 1.1f);
        r.y += noiseScrollSpeed * Time.deltaTime * Random.Range(0.9f, 1.1f);
        noiseLayer.uvRect = r;
    }

    // ─ Glitch

    void TickGlitch()
    {
        _glitchTimer += Time.deltaTime;
        if (_glitchTimer >= _nextGlitch)
        {
            _glitchTimer = 0f;
            ScheduleNextGlitch();
            StartCoroutine(GlitchFlash());
        }
    }

    void ScheduleNextGlitch()
    {
        float mult = _isNight2 ? 0.45f : 1f;
        _nextGlitch = Random.Range(glitchMinInterval, glitchMaxInterval) * mult;
    }

    IEnumerator GlitchFlash()
    {
        if (glitchLayer == null) yield break;

        var rect = glitchLayer.rectTransform;
        Vector2 origin = rect.anchoredPosition;

        glitchLayer.enabled = true;
        rect.anchoredPosition = origin + new Vector2(Random.Range(-15f, 15f), Random.Range(-8f, 8f));

        yield return new WaitForSeconds(glitchDuration);

        rect.anchoredPosition = origin;
        glitchLayer.enabled   = false;
    }

    // ─ Helpers

    void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        var c = g.color; c.a = a; g.color = c;
    }

    void SetAlpha(RawImage g, float a)
    {
        if (g == null) return;
        var c = g.color; c.a = a; g.color = c;
    }
}
