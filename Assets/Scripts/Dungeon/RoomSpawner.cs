
using System;
using System.Collections.Generic;
using UnityEngine;

public static class RoomSpawner
{
    private enum DoorDir
    {
        PosZ,
        NegZ,
        PosX,
        NegX
    }

    public static void SpawnRooms(FloorConfig config, Map map)
    {
        if (config == null)
        {
            Debug.LogError("RoomSpawner: FloorConfig is null.");
            return;
        }

        if (map == null || map.roomCoords == null)
        {
            Debug.LogError("RoomSpawner: Map is null.");
            return;
        }

        if (map.roomCoords.Count == 0)
        {
            Debug.LogWarning("RoomSpawner: Map has no room coordinates.");
            return;
        }

        float spacing = (config.DoorsToRoomOriginLength * 2) + config.CorridorLength;
        Dictionary<RoomType, List<GameObject>> prefabMap = config.RoomPrefabs;

        SpawnRoomPrefabs(map.roomCoords, prefabMap, spacing);
        SpawnCorridors(map.roomCoords, prefabMap, spacing);
    }

    private static void SpawnRoomPrefabs(
        HashSet<(int y, int x)> coords,
        Dictionary<RoomType, List<GameObject>> prefabMap,
        float spacing)
    {
        foreach (var coord in coords)
        {
            HashSet<DoorDir> neighborDirs = GetNeighborDirs(coord, coords);
            if (!TryGetRoomType(neighborDirs, out RoomType roomType))
            {
                Debug.LogWarning($"RoomSpawner: Unsupported neighbor count at {coord}.");
                continue;
            }

            if (!TryGetRoomRotation(neighborDirs, roomType, out Quaternion rotation))
            {
                Debug.LogWarning($"RoomSpawner: No rotation match for {roomType} at {coord}.");
                continue;
            }

            if (!TryGetRandomPrefab(prefabMap, roomType, out GameObject prefab))
            {
                Debug.LogWarning($"RoomSpawner: No prefab for room type {roomType}.");
                continue;
            }

            Vector3 worldPos = ToWorldPosition(coord, spacing);
            UnityEngine.Object.Instantiate(prefab, worldPos, rotation);
        }
    }

    private static void SpawnCorridors(
        HashSet<(int y, int x)> coords,
        Dictionary<RoomType, List<GameObject>> prefabMap,
        float spacing)
    {
        if (!TryGetRandomPrefab(prefabMap, RoomType.Corridor, out GameObject corridorPrefab))
        {
            Debug.LogWarning("RoomSpawner: No corridor prefab found.");
            return;
        }

        foreach (var coord in coords)
        {
            var right = (coord.y, coord.x + 1);
            if (coords.Contains(right))
            {
                Vector3 a = ToWorldPosition(coord, spacing);
                Vector3 b = ToWorldPosition(right, spacing);
                Vector3 mid = (a + b) * 0.5f;
                Quaternion rot = Quaternion.Euler(0f, 90f, 0f);
                UnityEngine.Object.Instantiate(corridorPrefab, mid, rot);
            }

            var up = (coord.y + 1, coord.x);
            if (coords.Contains(up))
            {
                Vector3 a = ToWorldPosition(coord, spacing);
                Vector3 b = ToWorldPosition(up, spacing);
                Vector3 mid = (a + b) * 0.5f;
                Quaternion rot = Quaternion.identity;
                UnityEngine.Object.Instantiate(corridorPrefab, mid, rot);
            }
        }
    }

    private static Vector3 ToWorldPosition((int y, int x) coord, float spacing)
    {
        return new Vector3(coord.x * spacing, 0f, coord.y * spacing);
    }

    private static HashSet<DoorDir> GetNeighborDirs(
        (int y, int x) coord,
        HashSet<(int y, int x)> coords)
    {
        HashSet<DoorDir> dirs = new HashSet<DoorDir>();
        if (coords.Contains((coord.y + 1, coord.x)))
        {
            dirs.Add(DoorDir.PosZ);
        }

        if (coords.Contains((coord.y - 1, coord.x)))
        {
            dirs.Add(DoorDir.NegZ);
        }

        if (coords.Contains((coord.y, coord.x + 1)))
        {
            dirs.Add(DoorDir.PosX);
        }

        if (coords.Contains((coord.y, coord.x - 1)))
        {
            dirs.Add(DoorDir.NegX);
        }

        return dirs;
    }

    private static bool TryGetRoomType(HashSet<DoorDir> neighborDirs, out RoomType roomType)
    {
        roomType = RoomType.OneDoor;
        int count = neighborDirs.Count;
        if (count == 1)
        {
            roomType = RoomType.OneDoor;
            return true;
        }

        if (count == 2)
        {
            bool vertical = neighborDirs.Contains(DoorDir.PosZ) && neighborDirs.Contains(DoorDir.NegZ);
            bool horizontal = neighborDirs.Contains(DoorDir.PosX) && neighborDirs.Contains(DoorDir.NegX);
            roomType = (vertical || horizontal) ? RoomType.TwoDoorsLine : RoomType.TwoDoorsAngle;
            return true;
        }

        if (count == 3)
        {
            roomType = RoomType.ThreeDoors;
            return true;
        }

        if (count == 4)
        {
            roomType = RoomType.FourDoors;
            return true;
        }

        return false;
    }

    private static bool TryGetRoomRotation(
        HashSet<DoorDir> neighborDirs,
        RoomType roomType,
        out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        HashSet<DoorDir> baseDirs = GetDefaultDoorDirs(roomType);
        for (int steps = 0; steps < 4; steps++)
        {
            if (SetsEqual(neighborDirs, RotateDirs(baseDirs, steps)))
            {
                rotation = Quaternion.Euler(0f, steps * 90f, 0f);
                return true;
            }
        }

        return false;
    }

    private static HashSet<DoorDir> GetDefaultDoorDirs(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.OneDoor:
                return new HashSet<DoorDir> { DoorDir.PosZ };
            case RoomType.TwoDoorsAngle:
                return new HashSet<DoorDir> { DoorDir.PosZ, DoorDir.NegX };
            case RoomType.TwoDoorsLine:
                return new HashSet<DoorDir> { DoorDir.PosZ, DoorDir.NegZ };
            case RoomType.ThreeDoors:
                return new HashSet<DoorDir> { DoorDir.PosZ, DoorDir.NegZ, DoorDir.NegX };
            case RoomType.FourDoors:
                return new HashSet<DoorDir> { DoorDir.PosZ, DoorDir.NegZ, DoorDir.NegX, DoorDir.PosX };
            default:
                return new HashSet<DoorDir>();
        }
    }

    private static HashSet<DoorDir> RotateDirs(HashSet<DoorDir> dirs, int steps)
    {
        HashSet<DoorDir> rotated = new HashSet<DoorDir>();
        foreach (DoorDir dir in dirs)
        {
            rotated.Add(RotateDir(dir, steps));
        }
        return rotated;
    }

    private static DoorDir RotateDir(DoorDir dir, int steps)
    {
        int s = ((steps % 4) + 4) % 4;
        DoorDir result = dir;
        for (int i = 0; i < s; i++)
        {
            result = RotateRight(result);
        }
        return result;
    }

    private static DoorDir RotateRight(DoorDir dir)
    {
        switch (dir)
        {
            case DoorDir.PosZ:
                return DoorDir.PosX;
            case DoorDir.PosX:
                return DoorDir.NegZ;
            case DoorDir.NegZ:
                return DoorDir.NegX;
            case DoorDir.NegX:
                return DoorDir.PosZ;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), dir, "Invalid door direction");
        }
    }

    private static bool SetsEqual(HashSet<DoorDir> a, HashSet<DoorDir> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (DoorDir dir in a)
        {
            if (!b.Contains(dir))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryGetRandomPrefab(
        Dictionary<RoomType, List<GameObject>> prefabMap,
        RoomType roomType,
        out GameObject prefab)
    {
        prefab = null;
        if (prefabMap == null || !prefabMap.TryGetValue(roomType, out List<GameObject> list))
        {
            return false;
        }

        if (list == null || list.Count == 0)
        {
            return false;
        }

        int index = UnityEngine.Random.Range(0, list.Count);
        prefab = list[index];
        return prefab != null;
    }
}