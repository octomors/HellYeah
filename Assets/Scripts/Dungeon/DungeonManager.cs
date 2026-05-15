using System.Collections.Generic;
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
        FloorConfig config = floorConfigs[floor];
        if (config == null)
        {
            Debug.LogError($"FloorConfig not found for floor {floor}.");
            return;
        }

        GenerateFloor(config);
    }

    [ContextMenu("Generate Floor")]
    public void GenerateFloor()
    {
        GenerateFloor(floorConfigs[0]);

    }

    public void GenerateFloor(FloorConfig config)
    {
        if (config == null)
        {
            Debug.LogError("FloorConfig is null.");
            return;
        }

        HashSet<string> prefabNames = new HashSet<string>();
        foreach (var group in config.RoomPrefabGroups)
        {
            if (group == null || group.prefabs == null)
            {
                continue;
            }

            foreach (var prefab in group.prefabs)
            {
                if (prefab != null)
                {
                    prefabNames.Add(prefab.name);
                }
            }
        }

        if (prefabNames.Count > 0)
        {
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                string name = obj.name;
                bool isGenerated = false;
                foreach (string prefabName in prefabNames)
                {
                    if (name == prefabName || name == prefabName + "(Clone)")
                    {
                        isGenerated = true;
                        break;
                    }
                }

                if (!isGenerated)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }

        IDungeonGenerator dungeonGenerator;
        switch (config.GenerationType)
        {
            case GenerationType.Sausage:
                dungeonGenerator = new SausageDungeonGenerator();
                break;
            case GenerationType.Waffle:
                dungeonGenerator = new WaffleDungeonGenerator();
                break;
            default:
                Debug.LogError($"Unsupported GenerationType.");
                return;
        }
        Map map = dungeonGenerator.Generate(config);
        RoomSpawner.SpawnRooms(config, map);

        LODGroup[] lodGroups = FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (LODGroup lodGroup in lodGroups)
        {
            if (lodGroup == null)
            {
                continue;
            }

            string rootName = lodGroup.transform.root.gameObject.name;
            bool isGenerated = false;
            foreach (string prefabName in prefabNames)
            {
                if (rootName == prefabName || rootName == prefabName + "(Clone)")
                {
                    isGenerated = true;
                    break;
                }
            }

            if (isGenerated)
            {
                lodGroup.enabled = false;
            }
        }
    }
}
