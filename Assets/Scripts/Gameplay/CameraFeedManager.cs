using UnityEngine;
using UnityEngine.UI;

public class CameraFeedManager : MonoBehaviour
{
    public static CameraFeedManager Instance { get; private set; }

    // ─ Inspector
    [Header("Security Cameras  (index 0–6: Stage, Dining, Backstage, LeftHall, RightHall, Kitchen, PirateCove)")]
    [SerializeField] Camera[] securityCameras = new Camera[7];

    [Header("Render Textures (one per camera – create in Assets/RenderTextures/)")]
    [SerializeField] RenderTexture[] renderTextures = new RenderTexture[7];

    [Header("Feed Display")]
    [Tooltip("The RawImage in the camera UI panel that shows the live feed.")]
    [SerializeField] RawImage feedDisplay;

    [Header("Monster Room Anchors (same index order as Cameras above)")]
    [Tooltip("Empty GameObjects placed where a monster stands in each room.")]
    [SerializeField] Transform[] roomAnchors = new Transform[7];

    [Header("Door Anchors  (visible from office, not on cameras)")]
    [Tooltip("Where the monster stands at the LEFT door (player sees it looking left).")]
    [SerializeField] Transform leftDoorAnchor;

    [Tooltip("Where the monster stands at the RIGHT door (player sees it looking right).")]
    [SerializeField] Transform rightDoorAnchor;

    // ─ State
    public int CameraCount => securityCameras.Length;

    // ─ Lifecycle

    void Awake()
    {
        Instance = this;
        AssignRenderTextures();
    }

    // ─ Public API

    public void ShowFeed(int index)
    {
        if (feedDisplay == null) return;
        if (index < 0 || index >= renderTextures.Length) return;
        if (renderTextures[index] == null) return;

        feedDisplay.texture = renderTextures[index];
    }

    public void PlaceMonsterAtRoom(Transform monsterRoot, Room room, MonsterType type = MonsterType.Freddy)
    {
        int typeIdx = (int)type;
        float offsetX = (typeIdx < MonsterOffsets.Length) ? MonsterOffsets[typeIdx] : 0f;

        if (room == Room.LeftDoor || room == Room.RightDoor)
        {
            Transform doorAnchor = GetDoorAnchor(room);
            if (doorAnchor != null)
            {
                monsterRoot.position = doorAnchor.position + doorAnchor.right * offsetX;
                monsterRoot.rotation = doorAnchor.rotation;
            }
            else
            {
                HideMonster(monsterRoot);
            }
            return;
        }

        int idx = RoomToAnchorIndex(room);
        if (idx < 0) { HideMonster(monsterRoot); return; }

        Transform anchor = (idx < roomAnchors.Length) ? roomAnchors[idx] : null;
        if (anchor == null) { HideMonster(monsterRoot); return; }

        monsterRoot.position = anchor.position + anchor.right * offsetX;
        monsterRoot.rotation = anchor.rotation;
    }

    Vector3 GetRoomOffset(Transform anchor, Room room)
    {
        return Vector3.zero; // offset se aplica en PlaceMonsterAtRoom con el tipo
    }

    static readonly float[] MonsterOffsets = { -80f, 80f, -160f, 160f };

    public void HideMonster(Transform monsterRoot)
    {
        monsterRoot.position = new Vector3(monsterRoot.position.x, -5000f, monsterRoot.position.z);
    }

    // ─ Internal

    void AssignRenderTextures()
    {
        for (int i = 0; i < securityCameras.Length; i++)
        {
            if (securityCameras[i] != null && i < renderTextures.Length && renderTextures[i] != null)
            {
                securityCameras[i].targetTexture = renderTextures[i];
                securityCameras[i].enabled = true;
            }
        }
    }

    int RoomToAnchorIndex(Room room)
    {
        switch (room)
        {
            case Room.Stage:  return 0;
            case Room.DiningArea: return 1;
            case Room.Backstage: return 2;
            case Room.LeftHall: return 3;
            case Room.RightHall: return 4;
            case Room.Kitchen: return 5;
            case Room.PirateCove: return 6;
            default: return -1;
        }
    }

    Transform GetDoorAnchor(Room room)
    {
        if (room == Room.LeftDoor)  return leftDoorAnchor;
        if (room == Room.RightDoor) return rightDoorAnchor;
        return null;
    }
}
