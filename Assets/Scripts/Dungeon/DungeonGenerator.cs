using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject exitDoorPrefab;

    [Header("Layout")]
    [SerializeField] private Transform generationRoot;
    [SerializeField] private float floorSize = 20f;
    [SerializeField] private float playerSpawnHeight = 1f;

    public void Generate(FloorConfig config, int seed, out Vector3 playerSpawnPosition, out Quaternion playerSpawnRotation)
    {
        playerSpawnPosition = Vector3.zero;
        playerSpawnRotation = Quaternion.identity;

        if (generationRoot == null)
        {
            generationRoot = transform;
        }

        ClearGenerated();

        Random.InitState(seed);

        SpawnFloor();
        SpawnExitDoor();

        playerSpawnPosition = generationRoot.TransformPoint(new Vector3(0f, playerSpawnHeight, -floorSize * 0.35f));
        playerSpawnRotation = generationRoot.rotation;
    }

    private void SpawnFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "GeneratedFloor";
        floor.transform.SetParent(generationRoot, false);
        floor.transform.localPosition = Vector3.zero;
        floor.transform.localScale = Vector3.one * (floorSize / 10f);
    }

    private void SpawnExitDoor()
    {
        Transform parent = generationRoot != null ? generationRoot : transform;
        Vector3 spawnPosition = generationRoot.TransformPoint(new Vector3(0f, 0f, floorSize * 0.35f));
        Quaternion spawnRotation = generationRoot.rotation;

        GameObject doorObject = Instantiate(exitDoorPrefab, spawnPosition, spawnRotation, parent);
        DungeonDoor door = doorObject.GetComponent<DungeonDoor>();
        if (door == null)
        {
            Debug.LogWarning("Exit door prefab does not contain DungeonDoor component.");
            return;
        }

        door.SetDoorType(DoorType.NextFloor);
    }

    private void ClearGenerated()
    {
        if (generationRoot == null)
        {
            return;
        }

        for (int i = generationRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(generationRoot.GetChild(i).gameObject);
        }
    }
}
