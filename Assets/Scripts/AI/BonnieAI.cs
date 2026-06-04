using UnityEngine;

public class BonnieAI : MonsterAI
{
    static readonly Room[] Path =
    {
        Room.Stage,
        Room.Backstage,
        Room.RightHall,
        Room.RightDoor
    };

    protected override Room[] GetPath() => Path;

    protected override void Start()
    {
        monsterType = MonsterType.Bonnie;
        baseMoveInterval = 12f;
        baseMoveChance = 0.50f;
        base.Start();
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
