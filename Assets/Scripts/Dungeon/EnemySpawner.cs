using System.Collections.Generic;
using UnityEngine;

public static class EnemySpawner
{
    private const string SpawnPointsRootName = "EnemySpawnPoints";

    public static void SpawnEnemies(FloorConfig config)
    {
        if (config == null)
        {
            Debug.LogError("EnemySpawner: FloorConfig is null.");
            return;
        }

        if (config.EnemyPrefabs == null || config.EnemyPrefabs.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefabs configured.");
            return;
        }

        List<GameObject> enemyPrefabs = BuildEnemyPrefabList(config.EnemyPrefabs);
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: All enemy prefabs are null.");
            return;
        }

        List<Transform> roomRoots = FindRoomSpawnRoots();
        if (roomRoots.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No EnemySpawnPoints objects found.");
            return;
        }

        int minEnemies = Mathf.Max(0, config.MinEnemiesPerRoom);
        int maxEnemies = Mathf.Max(minEnemies, config.MaxEnemiesPerRoom);

        foreach (Transform roomRoot in roomRoots)
        {
            List<Transform> spawnPoints = GetChildSpawnPoints(roomRoot);
            if (spawnPoints.Count == 0)
            {
                continue;
            }

            int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
            for (int i = 0; i < enemyCount; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
                if (prefab == null || spawnPoint == null)
                {
                    continue;
                }

                Object.Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }

    private static List<GameObject> BuildEnemyPrefabList(List<GameObject> prefabs)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                result.Add(prefab);
            }
        }

        return result;
    }

    private static List<Transform> FindRoomSpawnRoots()
    {
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        List<Transform> results = new List<Transform>();
        foreach (Transform transform in allTransforms)
        {
            if (transform != null && transform.name == SpawnPointsRootName)
            {
                results.Add(transform);
            }
        }

        return results;
    }

    private static List<Transform> GetChildSpawnPoints(Transform root)
    {
        List<Transform> spawnPoints = new List<Transform>();
        if (root == null)
        {
            return spawnPoints;
        }

        int count = root.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = root.GetChild(i);
            if (child != null)
            {
                spawnPoints.Add(child);
            }
        }

        return spawnPoints;
    }
}
