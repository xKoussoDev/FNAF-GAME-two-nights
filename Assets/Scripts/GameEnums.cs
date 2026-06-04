public enum GameState { MainMenu, Intro, Night1, Night2, GameOver, Victory }


public enum Room
{
    Stage  = 0, // CAM 1
    DiningArea = 1, // CAM 2
    Backstage = 2, // CAM 3
    LeftHall = 3, // CAM 4
    RightHall = 4, // CAM 5
    Kitchen = 5, // CAM 6
    PirateCove = 6, // CAM 7
    LeftDoor = 7, // off-camera left door
    RightDoor = 8, // off-camera right door
    Office = 9 // player
}


public enum MonsterType { Freddy = 0, Bonnie = 1, Chica = 2, Foxy = 3 }

public enum DoorSide { Left, Right }

public enum FoxyCurtainState { Closed = 0, Peeking = 1, Open = 2, ReadyToRun = 3, Running = 4 }
