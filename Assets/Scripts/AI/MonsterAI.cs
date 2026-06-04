using UnityEngine;
using System.Collections;


public abstract class MonsterAI : MonoBehaviour
{
    // ─ Inspector
    [Header("Identity")]
    [SerializeField] protected MonsterType monsterType;

    [Header("Movement  (tune these per monster)")]
    [Tooltip("Base seconds between movement attempts.")]
    [SerializeField] protected float baseMoveInterval = 15f;

    [Tooltip("0–1 probability the monster actually moves each interval.")]
    [SerializeField] protected float baseMoveChance = 0.40f;

    [Tooltip("After 3 AM, move chance is multiplied by this value.")]
    [SerializeField] protected float aggressionMultiplier = 1.6f;

    [Tooltip("On Night 2, the move interval is multiplied by this (< 1 = faster).")]
    [SerializeField] protected float night2SpeedMultiplier = 0.55f;

    [Header("Door Behaviour")]
    [Tooltip("Seconds the monster waits at the door before attacking (if open).")]
    [SerializeField] protected float doorLingerTime = 4f;

    [Tooltip("Seconds the monster stays in the hall after being blocked before retreating.")]
    [SerializeField] float retreatDelay = 2f;

    [Header("Optional Animator")]
    [Tooltip("Drag the FBX model's Animator component here if animations are set up.")]
    [SerializeField] protected Animator monsterAnimator;

    // ─ State
    public Room CurrentRoom { get; protected set; } = Room.Stage;
    public bool IsActive { get; private set; } = false;
    public bool IsAttacking { get; protected set; } = false;

    protected int _pathIndex = 0;

    float _moveTimer;
    float _moveInterval;
    float _moveChance;

    // ─ Abstract Interface

    protected abstract Room[] GetPath();

    protected virtual void OnRoomEntered(Room newRoom) { }

    protected virtual void OnBlockedByDoor() { }

    // ─ Lifecycle

    protected virtual void Start()
    {
        CurrentRoom = Room.Stage;
        _pathIndex = 0;
        CameraFeedManager.Instance?.HideMonster(transform);
    }

    protected virtual void Update()
    {
        if (!IsActive) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (PowerManager.Instance != null && PowerManager.Instance.IsPowerOut) return;

        _moveTimer += Time.deltaTime;
        if (_moveTimer >= _moveInterval)
        {
            _moveTimer = 0f;
            TryMove();
        }
    }

    // ─ Public API

    public void Activate(int nightNumber)
    {
        IsActive = true;
        _pathIndex = 0;
        CurrentRoom = GetStartRoom();

        float speedMult = (nightNumber == 2) ? night2SpeedMultiplier : 1f;
        _moveInterval = baseMoveInterval * speedMult;
        _moveChance = baseMoveChance;
        _moveTimer = 0f;

        CameraFeedManager.Instance?.PlaceMonsterAtRoom(transform, CurrentRoom, monsterType);
        OnRoomEntered(CurrentRoom);
    }

    protected virtual Room GetStartRoom() => Room.Stage;

    public void Deactivate()
    {
        IsActive = false;
        CameraFeedManager.Instance?.HideMonster(transform);
    }

    // ─ Internal Movement

    void TryMove()
    {
        float chance = _moveChance;

        if (TimeManager.Instance != null && TimeManager.Instance.CurrentHour >= 3)
            chance *= aggressionMultiplier;

        if (Random.value > chance) return;

        Room[] path = GetPath();
        if (path == null || path.Length == 0) return;

        int nextIndex = _pathIndex + 1;
        if (nextIndex >= path.Length) return;

        Room nextRoom = path[nextIndex];

        if (nextRoom == Room.LeftDoor || nextRoom == Room.RightDoor)
            HandleDoorAttempt(nextRoom, nextIndex);
        else
            MoveToRoom(nextRoom, nextIndex);
    }

    void HandleDoorAttempt(Room doorRoom, int targetPathIndex)
    {
        if (IsAttacking) return;
        MoveToRoom(doorRoom, targetPathIndex);
        StartCoroutine(LingerAtDoor(doorRoom, targetPathIndex));
    }

    IEnumerator LingerAtDoor(Room doorRoom, int targetPathIndex)
    {
        IsAttacking = true;
        DoorSide side = (doorRoom == Room.LeftDoor) ? DoorSide.Left : DoorSide.Right;

        float elapsed = 0f;

        while (elapsed < doorLingerTime)
        {
            if (FindDoorState(side))
            {
                IsAttacking = false;
                AudioManager.Instance?.PlayMonsterBang();
                OnBlockedByDoor();

                yield return new WaitForSeconds(retreatDelay);
                RetreatToRandomRoom();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        JumpscareManager.Instance?.TriggerJumpscare(monsterType);
    }

    void RetreatToRandomRoom()
    {
        Room[] path = GetPath();
        if (path == null || path.Length == 0) return;

        int maxIdx  = Mathf.Max(0, _pathIndex - 1);
        int randIdx = Random.Range(0, maxIdx + 1);

        MoveToRoom(path[randIdx], randIdx);
        IsAttacking = false;
    }

    protected void MoveToRoom(Room room, int newPathIndex)
    {
        CurrentRoom = room;
        _pathIndex  = newPathIndex;
        CameraFeedManager.Instance?.PlaceMonsterAtRoom(transform, room, monsterType);
        OnRoomEntered(room);
    }

    bool FindDoorState(DoorSide side)
    {
        foreach (var door in FindObjectsByType<DoorController>())
            if (door.Side == side) return door.IsClosed;
        return false;
    }

    // ─ Animator Helpers

    protected void SetBool(string param, bool value)
    {
        if (monsterAnimator != null) monsterAnimator.SetBool(param, value);
    }

    protected void SetTrigger(string param)
    {
        if (monsterAnimator != null) monsterAnimator.SetTrigger(param);
    }
}
