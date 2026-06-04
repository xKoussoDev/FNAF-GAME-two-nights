using UnityEngine;

public class FreddyAI : MonsterAI
{
    [Header("Freddy Settings")]
    [Tooltip("Seconds between laugh attempts.")]
    [SerializeField] float laughInterval = 30f;

    [Tooltip("0–1 probability that a laugh actually plays each interval.")]
    [SerializeField] float laughChance   = 0.5f;

    static readonly Room[] Path =
    {
        Room.Stage,
        Room.DiningArea,
        Room.Kitchen,
        Room.RightHall,
        Room.RightDoor
    };

    float _laughTimer;

    protected override Room[] GetPath() => Path;

    protected override void Start()
    {
        monsterType = MonsterType.Freddy;
        baseMoveInterval = 22f;
        baseMoveChance = 0.30f;
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (!IsActive) return;

        _laughTimer += Time.deltaTime;
        if (_laughTimer >= laughInterval)
        {
            _laughTimer = 0f;
            if (Random.value < laughChance)
                AudioManager.Instance?.PlayFreddyLaugh();
        }
    }

    protected override void OnRoomEntered(Room room)
    {
        SetBool("IsWalking", room != Room.Stage);
    }

    protected override void OnBlockedByDoor()
    {
        SetBool("IsWalking", false);
    }
}
