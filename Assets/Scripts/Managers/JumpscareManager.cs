using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance { get; private set; }

    [Header("Jumpscare Sprites (order: Freddy, Bonnie, Chica, Foxy)")]
    [Tooltip("Assign 4 jumpscare sprites in the same order as MonsterType enum.")]
    [SerializeField] Sprite[] jumpscareSprites = new Sprite[4];

    [Header("UI References")]
    [Tooltip("The full-screen panel that contains the jumpscare image.")]
    [SerializeField] GameObject jumpscarePanel;

    [Tooltip("The Image component that displays the monster sprite.")]
    [SerializeField] Image jumpscareImage;

    [Tooltip("A solid white Image used for the initial flash effect.")]
    [SerializeField] Image whiteFlash;

    [Header("Camera Shake")]
    [Tooltip("Drag the player's Camera transform here for screen shake.")]
    [SerializeField] Transform cameraRoot;

    [SerializeField] float shakeIntensity = 0.08f;
    [SerializeField] float shakeDuration  = 0.6f;

    [Header("Timing")]
    [Tooltip("How long the jumpscare image is shown before Game Over loads.")]
    [SerializeField] float displayDuration = 2.0f;

    bool _isPlaying;

    // ─ Lifecycle

    void Awake()
    {
        Instance = this;
        if (jumpscarePanel != null) jumpscarePanel.SetActive(false);
        if (whiteFlash != null) whiteFlash.enabled = false;
    }

    // ─ Public API
    public void TriggerJumpscare(MonsterType monster)
    {
        if (_isPlaying) return;
        _isPlaying = true;
        StartCoroutine(JumpscareSequence(monster));
    }

    // ─ Internal

    IEnumerator JumpscareSequence(MonsterType monster)
    {
        OfficeLookController.Instance?.SetEnabled(false);
        CameraSystem.Instance?.ForceClose();

        if (whiteFlash != null)
        {
            whiteFlash.enabled = true;
            yield return new WaitForSecondsRealtime(0.05f);
            whiteFlash.enabled = false;
        }

        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(true);

            int idx = (int)monster;
            if (jumpscareImage != null &&
                idx >= 0 && idx < jumpscareSprites.Length &&
                jumpscareSprites[idx] != null)
            {
                jumpscareImage.sprite = jumpscareSprites[idx];
            }
        }

        AudioManager.Instance?.PlayJumpscare();

        if (cameraRoot != null)
            StartCoroutine(ShakeCamera(cameraRoot, shakeDuration, shakeIntensity));

        yield return new WaitForSecondsRealtime(displayDuration);

        GameManager.Instance?.TriggerGameOver();
    }

    IEnumerator ShakeCamera(Transform cam, float duration, float magnitude)
    {
        Vector3 origin  = cam.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cam.localPosition = origin + new Vector3(x, y, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cam.localPosition = origin;
    }
}
