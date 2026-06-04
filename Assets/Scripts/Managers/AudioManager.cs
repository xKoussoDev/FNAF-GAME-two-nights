using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ─ Ambience clips
    [Header("Ambience")]
    [SerializeField] AudioClip officeAmbienceClip;
    [SerializeField] AudioClip powerOutAmbienceClip;
    [SerializeField] AudioClip victoryAmbienceClip;
    [SerializeField] AudioClip menuAmbienceClip;

    // ─ Office SFX
    [Header("Office SFX")]
    [SerializeField] AudioClip cameraOpenClip;
    [SerializeField] AudioClip cameraStaticClip;
    [SerializeField] AudioClip doorCloseClip;
    [SerializeField] AudioClip doorOpenClip;
    [SerializeField] AudioClip lightSwitchClip;

    // ─ Monster SFX
    [Header("Monster SFX")]
    [SerializeField] AudioClip monsterBangClip; // blocked by closed door
    [SerializeField] AudioClip jumpscareScreamClip; // loud jumpscare scream
    [SerializeField] AudioClip foxyRunClip; // Foxy sprint
    [SerializeField] AudioClip freddyLaughClip; // Freddy's laugh
    [SerializeField] AudioClip phoneRingClip;

    // ─ Volume controls
    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] float ambienceVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] float officeAmbienceVolume = 0.15f;
    [Range(0f, 1f)] [SerializeField] float sfxVolume = 1.0f;
    [Range(0f, 1f)] [SerializeField] float doorVolume = 0.3f;
    [Range(0f, 1f)] [SerializeField] float musicVolume = 0.4f;

    // ─ Sources (created at runtime)
    AudioSource _ambience;
    AudioSource _sfx;
    AudioSource _music;
    AudioSource _cameraStatic;

    // ─ Lifecycle

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _ambience = MakeSource("Ambience", loop: true,  volume: ambienceVolume);
        _sfx = MakeSource("SFX", loop: false, volume: sfxVolume);
        _music = MakeSource("Music", loop: true,  volume: musicVolume);
        _cameraStatic = MakeSource("CameraStatic", loop: true,  volume: 0.3f);
    }

    AudioSource MakeSource(string label, bool loop, float volume)
    {
        var go = new GameObject(label + "Source");
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.volume = volume;
        src.playOnAwake = false;
        return src;
    }

    // ─ Ambience API

    public void PlayOfficeAmbience()
    {
        if (officeAmbienceClip == null || _ambience == null) return;
        if (_ambience.clip == officeAmbienceClip && _ambience.isPlaying) return;
        _ambience.clip = officeAmbienceClip;
        _ambience.volume = officeAmbienceVolume;
        _ambience.Play();
    }
    public void PlayPowerOutAmbience() => PlayAmbience(powerOutAmbienceClip);
    public void PlayVictoryAmbience() => PlayAmbience(victoryAmbienceClip);
    public void PlayMenuAmbience() => PlayAmbience(menuAmbienceClip);
    public void StopAmbience() => _ambience.Stop();

    void PlayAmbience(AudioClip clip)
    {
        if (clip == null || _ambience == null) return;
        if (_ambience.clip == clip && _ambience.isPlaying) return; // already playing
        _ambience.clip = clip;
        _ambience.Play();
    }

    // ─ SFX API

    public void PlayCameraOpen() => PlaySFX(cameraOpenClip);
    public void PlayDoorClose() => PlaySFX(doorCloseClip, doorVolume);
    public void PlayDoorOpen() => PlaySFX(doorOpenClip,  doorVolume);
    public void PlayLightSwitch() => PlaySFX(lightSwitchClip);
    public void PlayMonsterBang() => PlaySFX(monsterBangClip);
    public void PlayJumpscare() => PlaySFX(jumpscareScreamClip, volume: 1f);
    public void PlayFoxyRun() => PlaySFX(foxyRunClip);
    public void PlayFreddyLaugh() => PlaySFX(freddyLaughClip);
    public void PlayPhoneRing() => PlaySFX(phoneRingClip);


    public void StartCameraStatic()
    {
        if (cameraStaticClip == null || _cameraStatic == null) return;
        if (_cameraStatic.isPlaying) return;
        _cameraStatic.clip = cameraStaticClip;
        _cameraStatic.Play();
    }

    public void StopCameraStatic()
    {
        _cameraStatic?.Stop();
    }

    public void PlayCameraStatic() => PlaySFX(cameraStaticClip);

    public void StopSFX() => _sfx?.Stop();

    void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _sfx == null) return;
        _sfx.PlayOneShot(clip, volume);
    }

    // ─ Scene change
    public void StopAll()
    {
        _sfx?.Stop();
        _music?.Stop();
        _cameraStatic?.Stop();
    }

    public void StopAllIncludingAmbience()
    {
        _ambience?.Stop();
        _sfx?.Stop();
        _music?.Stop();
        _cameraStatic?.Stop();
    }

    // ─ Volume Setters

    public void SetAmbienceVolume(float v) { ambienceVolume = v; if (_ambience) _ambience.volume = v; }
    public void SetSFXVolume(float v) { sfxVolume = v; if (_sfx) _sfx.volume = v; }
}
