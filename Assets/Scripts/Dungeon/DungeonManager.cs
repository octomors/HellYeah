using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private IDungeonGenerator dungeonGenerator;
    [SerializeField] private FloorConfig[] floorConfigs = new FloorConfig[3];

    private void Start()
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

        InitFloor();
    }

    public void InitFloor()
    {
    }

    private FloorConfig ResolveConfigForFloor(int floor)
    {
        int index = Mathf.Clamp(floor - 1, 0, floorConfigs.Length - 1);
        FloorConfig config = floorConfigs[index];

        return config;
    }
}
