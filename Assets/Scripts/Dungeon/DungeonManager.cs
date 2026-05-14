using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private FloorConfig[] floorConfigs = new FloorConfig[3];

    private void Start()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager instance is not found.");
            return;
        }

        InitFloor();
    }

    public void InitFloor()
    {
        int floor = RunManager.Instance.CurrentFloor;
        FloorConfig config = ResolveConfigForFloor(floor);
        if (config == null)
        {
            Debug.LogError($"FloorConfig not found for floor {floor}.");
            return;
        }
        IDungeonGenerator dungeonGenerator;
        switch (config.GenerationType)
        {
            case GenerationType.Sausage:
                dungeonGenerator = new SausageDungeonGenerator();
                break;
            default:
                Debug.LogError($"Unsupported GenerationType.");
                return;
        }
        Map map = dungeonGenerator.Generate(config);
        RoomSpawner.SpawnRooms(config, map);
    }

    private FloorConfig ResolveConfigForFloor(int floor)
    {
        int index = Mathf.Clamp(floor - 1, 0, floorConfigs.Length - 1);
        FloorConfig config = floorConfigs[index];

        return config;
    }
}
