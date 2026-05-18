using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private FloorConfig[] floorConfigs = new FloorConfig[3];

    private void Start()
    {
        InitFloor();
    }

    public void InitFloor()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager instance is not found.");
            return;
        }

        if (dungeonGenerator == null)
        {
            Debug.LogError("DungeonGenerator is not assigned in DungeonManager.");
            return;
        }

        int floor = RunManager.Instance.CurrentFloor;

        FloorConfig config = ResolveConfigForFloor(floor);
        int floorSeed = RunManager.Instance.Seed + floor * 997;

        dungeonGenerator.Generate(config, floorSeed, out Vector3 playerSpawnPosition, out Quaternion playerSpawnRotation);

        SpawnPlayer(playerSpawnPosition, playerSpawnRotation);
    }

    private void SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        PlayerSpawner spawner = PlayerSpawner.Instance != null ? PlayerSpawner.Instance : FindAnyObjectByType<PlayerSpawner>();
        if (spawner == null)
        {
            Debug.LogError("PlayerSpawner not found in scene.");
            return;
        }

        spawner.SpawnPlayer(position, rotation);
    }

    private FloorConfig ResolveConfigForFloor(int floor)
    {
        int index = Mathf.Clamp(floor - 1, 0, floorConfigs.Length - 1);
        FloorConfig config = floorConfigs[index];

        return config;
    }
}
