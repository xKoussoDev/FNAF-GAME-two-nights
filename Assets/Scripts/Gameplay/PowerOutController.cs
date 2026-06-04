using UnityEngine;
using System.Collections;

public class PowerOutController : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds of darkness before Freddy attacks after power goes out.")]
    [SerializeField] float attackDelay = 20f;

    void OnEnable() => PowerManager.OnPowerDepleted += OnPowerOut;
    void OnDisable() => PowerManager.OnPowerDepleted -= OnPowerOut;

    void OnPowerOut() => StartCoroutine(PowerOutSequence());

    IEnumerator PowerOutSequence()
    {
        foreach (var door in FindObjectsByType<DoorController>())
            door.ForceOpen();

        foreach (var light in FindObjectsByType<LightController>())
            light.ForceOff();

        CameraSystem.Instance?.ForceClose();

        UIManager.Instance?.ShowPowerOutOverlay(true);
        yield return new WaitForSeconds(3f);
        UIManager.Instance?.ShowPowerOutOverlay(false);

        OfficeLookController.Instance?.SetEnabled(true);

        yield return new WaitForSeconds(attackDelay * 0.6f);

        AudioManager.Instance?.PlayFreddyLaugh();

        yield return new WaitForSeconds(attackDelay * 0.4f);

        JumpscareManager.Instance?.TriggerJumpscare(MonsterType.Freddy);
    }
}
