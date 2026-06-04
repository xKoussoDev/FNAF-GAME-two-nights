using UnityEngine;

public class ChicaAI : MonsterAI
{
    static readonly Room[] Path =
    {
        Room.Stage,
        Room.DiningArea,
        Room.Backstage,
        Room.LeftHall,
        Room.LeftDoor
    };

    protected override Room[] GetPath() => Path;

    protected override void Start()
    {
        monsterType = MonsterType.Chica;
        baseMoveInterval = 14f;
        baseMoveChance = 0.45f;
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
