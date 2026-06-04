using UnityEngine;
using System.Collections;

public class FoxyAI : MonsterAI
{
    // ── Inspector ───────────────────────────────────────────────
    [Header("Foxy Curtain State GameObjects")]
    [SerializeField] GameObject curtainClosed;
    [SerializeField] GameObject curtainPeeking;
    [SerializeField] GameObject curtainOpen;
    [SerializeField] GameObject foxyRunning;

    [Header("Foxy Timing")]
    [Tooltip("Seconds to advance one curtain stage.")]
    [SerializeField] float timePerCurtainStage = 35f;

    [Tooltip("Seconds Foxy stays in ReadyToRun state before sprinting (last warning).")]
    [SerializeField] float readyToRunDuration = 20f;

    [Tooltip("Seconds to reach the left door from Pirate Cove.")]
    [SerializeField] float sprintDuration = 2.5f;

    [Tooltip("Seconds Foxy waits inside the cove before becoming dangerous again.")]
    [SerializeField] float rechargeDelay = 45f;

    [Header("Camera Watch Resistance")]
    [Tooltip("Seconds the player must watch CAM 7 to push Foxy back one stage.")]
    [SerializeField] float watchTimeToRegress = 5f;

    // ─ State
    public FoxyCurtainState CurtainState { get; private set; } = FoxyCurtainState.Closed;

    float _curtainTimer;
    float _watchTimer;
    bool  _isSprinting;

    static readonly Room[] Path = { Room.PirateCove, Room.LeftHall, Room.LeftDoor };

    protected override Room[] GetPath() => Path;

    protected override Room GetStartRoom() => Room.PirateCove;

    // ─ Lifecycle

    protected override void Start()
    {
        monsterType = MonsterType.Foxy;
        baseMoveInterval = 9999f;
        baseMoveChance   = 0f;
        base.Start();

        CurrentRoom = Room.PirateCove;
        SetCurtainState(FoxyCurtainState.Closed);
        CameraFeedManager.Instance?.PlaceMonsterAtRoom(transform, Room.PirateCove, MonsterType.Foxy);
    }

    protected override void Update()
    {
        if (!IsActive) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (PowerManager.Instance != null && PowerManager.Instance.IsPowerOut) return;
        if (_isSprinting) return;

        TickCurtain();
        TickCameraWatch();
    }

    // ─ Curtain Tick

    void TickCurtain()
    {
        _curtainTimer += Time.deltaTime;
        float threshold = (CurtainState == FoxyCurtainState.ReadyToRun)
                          ? readyToRunDuration
                          : timePerCurtainStage;

        if (_curtainTimer >= threshold)
        {
            _curtainTimer = 0f;
            AdvanceCurtain();
        }
    }

    void AdvanceCurtain()
    {
        switch (CurtainState)
        {
            case FoxyCurtainState.Closed:
                SetCurtainState(FoxyCurtainState.Peeking);
                break;
            case FoxyCurtainState.Peeking:
                SetCurtainState(FoxyCurtainState.Open);
                break;
            case FoxyCurtainState.Open:
                SetCurtainState(FoxyCurtainState.ReadyToRun);
                _curtainTimer = 0f;
                break;
            case FoxyCurtainState.ReadyToRun:
                StartCoroutine(SprintToLeftDoor());
                break;
        }
    }

    // ─ Camera Watch Tick

    void TickCameraWatch()
    {
        bool watchingCove = CameraSystem.Instance != null
                            && CameraSystem.Instance.CamerasOpen
                            && CameraSystem.Instance.CurrentCam == 6;

        if (watchingCove)
        {
            _watchTimer += Time.deltaTime;
            if (_watchTimer >= watchTimeToRegress)
            {
                _watchTimer = 0f;
                RegressCurtain();
            }
        }
        else
        {
            _watchTimer = 0f;
        }
    }

    void RegressCurtain()
    {
        if (CurtainState == FoxyCurtainState.Running) return;

        switch (CurtainState)
        {
            case FoxyCurtainState.ReadyToRun:
                SetCurtainState(FoxyCurtainState.Open);
                break;
            case FoxyCurtainState.Open:
                SetCurtainState(FoxyCurtainState.Peeking);
                break;
            case FoxyCurtainState.Peeking:
                SetCurtainState(FoxyCurtainState.Closed);
                break;
        }
        _curtainTimer = 0f;
    }

    // ─ Sprint Attack

    IEnumerator SprintToLeftDoor()
    {
        _isSprinting = true;
        SetCurtainState(FoxyCurtainState.Running);
        AudioManager.Instance?.PlayFoxyRun();

        Room[] runPath = { Room.Backstage, Room.RightHall, Room.LeftHall };

        foreach (Room room in runPath)
        {
            CurrentRoom = room;
            CameraFeedManager.Instance?.PlaceMonsterAtRoom(transform, room, MonsterType.Foxy);
            yield return new WaitForSeconds(sprintDuration);
        }

        CurrentRoom = Room.LeftDoor;
        CameraFeedManager.Instance?.PlaceMonsterAtRoom(transform, Room.LeftDoor, MonsterType.Foxy);

        float elapsed = 0f;
        bool blocked = false;

        while (elapsed < doorLingerTime)
        {
            bool leftClosed = false;
            foreach (var door in FindObjectsByType<DoorController>())
                if (door.Side == DoorSide.Left) { leftClosed = door.IsClosed; break; }

            if (leftClosed)
            {
                blocked = true;
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (blocked)
        {
            AudioManager.Instance?.PlayMonsterBang();
            yield return new WaitForSeconds(0.5f);

            CurrentRoom = Room.PirateCove;
            CameraFeedManager.Instance?.PlaceMonsterAtRoom(transform, Room.PirateCove, MonsterType.Foxy);
            SetCurtainState(FoxyCurtainState.Closed);
            _curtainTimer = 0f;

            yield return new WaitForSeconds(rechargeDelay);
        }
        else
        {
            JumpscareManager.Instance?.TriggerJumpscare(MonsterType.Foxy);
        }

        _isSprinting = false;
    }

    // ─ State Visuals

    void SetCurtainState(FoxyCurtainState state)
    {
        CurtainState = state;

        if (curtainClosed != null) curtainClosed.SetActive(state == FoxyCurtainState.Closed);
        if (curtainPeeking != null) curtainPeeking.SetActive(state == FoxyCurtainState.Peeking);
        if (curtainOpen != null) curtainOpen.SetActive(state == FoxyCurtainState.Open || state == FoxyCurtainState.ReadyToRun);
        if (foxyRunning != null) foxyRunning.SetActive(state == FoxyCurtainState.Running);
    }
}
