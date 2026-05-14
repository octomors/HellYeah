
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

        Dictionary<(int y, int x), DoorDir> specialDoors = BuildSpecialDoorMap(map, map.roomCoords);
        SpawnSpecialRooms(map, prefabMap, spacing, specialDoors);
        SpawnRoomPrefabs(map, prefabMap, spacing, specialDoors);
        SpawnCorridors(map, prefabMap, spacing, specialDoors);
    }

    private static void SpawnRoomPrefabs(
        Map map,
        Dictionary<RoomType, List<GameObject>> prefabMap,
        float spacing,
        Dictionary<(int y, int x), DoorDir> specialDoors)
    {
        foreach (var coord in map.roomCoords)
        {
            if (specialDoors != null && specialDoors.ContainsKey(coord))
            {
                continue;
            }

            HashSet<DoorDir> neighborDirs = GetNeighborDirs(coord, map.roomCoords, specialDoors, map);
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
        float spacing,
        Dictionary<(int y, int x), DoorDir> specialDoors)
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

            if (specialDoors != null && specialDoors.Count > 0)
            {
                TrySpawnSpecialCorridor(coord, specialDoors, corridorPrefab, spacing, DoorDir.PosZ);
                TrySpawnSpecialCorridor(coord, specialDoors, corridorPrefab, spacing, DoorDir.NegZ);
                TrySpawnSpecialCorridor(coord, specialDoors, corridorPrefab, spacing, DoorDir.PosX);
                TrySpawnSpecialCorridor(coord, specialDoors, corridorPrefab, spacing, DoorDir.NegX);
            }
        }
    }

    private static Vector3 ToWorldPosition((int y, int x) coord, float spacing)
    {
        return new Vector3(coord.x * spacing, 0f, coord.y * spacing);
    }

    private static HashSet<DoorDir> GetNeighborDirs(
        (int y, int x) coord,
        HashSet<(int y, int x)> coords,
        Dictionary<(int y, int x), DoorDir> specialDoors,
        Map map)
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

        if (specialDoors != null && map != null)
        {
            TryAddSpecialNeighbor(map.EntranceRoomCoords, coord, specialDoors, DoorDir.PosZ, dirs);
            TryAddSpecialNeighbor(map.EntranceRoomCoords, coord, specialDoors, DoorDir.NegZ, dirs);
            TryAddSpecialNeighbor(map.EntranceRoomCoords, coord, specialDoors, DoorDir.PosX, dirs);
            TryAddSpecialNeighbor(map.EntranceRoomCoords, coord, specialDoors, DoorDir.NegX, dirs);

            TryAddSpecialNeighbor(map.ExitRoomCoords, coord, specialDoors, DoorDir.PosZ, dirs);
            TryAddSpecialNeighbor(map.ExitRoomCoords, coord, specialDoors, DoorDir.NegZ, dirs);
            TryAddSpecialNeighbor(map.ExitRoomCoords, coord, specialDoors, DoorDir.PosX, dirs);
            TryAddSpecialNeighbor(map.ExitRoomCoords, coord, specialDoors, DoorDir.NegX, dirs);
        }

        return dirs;
    }

    private static Dictionary<(int y, int x), DoorDir> BuildSpecialDoorMap(
        Map map,
        HashSet<(int y, int x)> coords)
    {
        Dictionary<(int y, int x), DoorDir> result = new Dictionary<(int y, int x), DoorDir>();
        if (map == null || coords == null)
        {
            return result;
        }

        bool sameCoord = map.EntranceRoomCoords.Equals(map.ExitRoomCoords);
        if (TryChooseSpecialDoor(map.EntranceRoomCoords, coords, out DoorDir entranceDoor))
        {
            result[map.EntranceRoomCoords] = entranceDoor;
        }

        if (!sameCoord && TryChooseSpecialDoor(map.ExitRoomCoords, coords, out DoorDir exitDoor))
        {
            result[map.ExitRoomCoords] = exitDoor;
        }

        if (sameCoord && result.ContainsKey(map.EntranceRoomCoords))
        {
            Debug.LogWarning($"RoomSpawner: Entrance and exit share the same coord {map.EntranceRoomCoords}. Using entrance prefab.");
        }

        return result;
    }

    private static bool TryChooseSpecialDoor(
        (int y, int x) coord,
        HashSet<(int y, int x)> coords,
        out DoorDir door)
    {
        door = DoorDir.PosZ;
        HashSet<DoorDir> dirs = GetAdjacentDirs(coord, coords);
        DoorDir? chosen = ChooseSingleDoor(dirs);
        if (!chosen.HasValue)
        {
            return false;
        }

        door = chosen.Value;
        return true;
    }

    private static HashSet<DoorDir> GetAdjacentDirs(
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

    private static void SpawnSpecialRooms(
        Map map,
        Dictionary<RoomType, List<GameObject>> prefabMap,
        float spacing,
        Dictionary<(int y, int x), DoorDir> specialDoors)
    {
        if (map == null || specialDoors == null)
        {
            return;
        }

        foreach (var entry in specialDoors)
        {
            (int y, int x) coord = entry.Key;
            RoomType roomType = coord.Equals(map.EntranceRoomCoords) ? RoomType.EntranceRoom : RoomType.ExitRoom;
            HashSet<DoorDir> neighborDirs = new HashSet<DoorDir> { entry.Value };
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

        bool isEntrance = coord.Equals(map.EntranceRoomCoords);
        bool isExit = coord.Equals(map.ExitRoomCoords);
        if (!isEntrance && !isExit)
        {
            return dirs;
        }

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

        DoorDir? chosen = ChooseSingleDoor(dirs);
        HashSet<DoorDir> result = new HashSet<DoorDir>();
        if (chosen.HasValue)
        {
            result.Add(chosen.Value);
        }

        return result;
    }

    private static DoorDir? ChooseSingleDoor(HashSet<DoorDir> dirs)
    {
        if (dirs.Contains(DoorDir.PosZ))
        {
            return DoorDir.PosZ;
        }

        if (dirs.Contains(DoorDir.NegZ))
        {
            return DoorDir.NegZ;
        }

        if (dirs.Contains(DoorDir.PosX))
        {
            return DoorDir.PosX;
        }

        if (dirs.Contains(DoorDir.NegX))
        {
            return DoorDir.NegX;
        }

        return null;
    }

    private static void TryAddSpecialNeighbor(
        (int y, int x) specialCoord,
        (int y, int x) coord,
        Dictionary<(int y, int x), DoorDir> specialDoors,
        DoorDir dirFromCoord,
        HashSet<DoorDir> dirs)
    {
        if (specialDoors == null)
        {
            return;
        }

        (int y, int x) expected = AddDir(coord, dirFromCoord);
        if (!expected.Equals(specialCoord))
        {
            return;
        }

        if (!specialDoors.TryGetValue(specialCoord, out DoorDir specialDoor))
        {
            return;
        }

        if (specialDoor == Opposite(dirFromCoord))
        {
            dirs.Add(dirFromCoord);
        }
    }

    private static (int y, int x) AddDir((int y, int x) coord, DoorDir dir)
    {
        switch (dir)
        {
            case DoorDir.PosZ:
                return (coord.y + 1, coord.x);
            case DoorDir.NegZ:
                return (coord.y - 1, coord.x);
            case DoorDir.PosX:
                return (coord.y, coord.x + 1);
            case DoorDir.NegX:
                return (coord.y, coord.x - 1);
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), dir, "Invalid door direction");
        }
    }

    private static void TrySpawnSpecialCorridor(
        (int y, int x) coord,
        Dictionary<(int y, int x), DoorDir> specialDoors,
        GameObject corridorPrefab,
        float spacing,
        DoorDir dirFromCoord)
    {
        if (specialDoors == null)
        {
            return;
        }

        (int y, int x) specialCoord = AddDir(coord, dirFromCoord);
        if (!specialDoors.TryGetValue(specialCoord, out DoorDir specialDoor))
        {
            return;
        }

        if (specialDoor != Opposite(dirFromCoord))
        {
            return;
        }

        Vector3 a = ToWorldPosition(coord, spacing);
        Vector3 b = ToWorldPosition(specialCoord, spacing);
        Vector3 mid = (a + b) * 0.5f;
        Quaternion rot = (dirFromCoord == DoorDir.PosX || dirFromCoord == DoorDir.NegX)
            ? Quaternion.Euler(0f, 90f, 0f)
            : Quaternion.identity;
        UnityEngine.Object.Instantiate(corridorPrefab, mid, rot);
    }

    private static DoorDir Opposite(DoorDir dir)
    {
        switch (dir)
        {
            case DoorDir.PosZ:
                return DoorDir.NegZ;
            case DoorDir.NegZ:
                return DoorDir.PosZ;
            case DoorDir.PosX:
                return DoorDir.NegX;
            case DoorDir.NegX:
                return DoorDir.PosX;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), dir, "Invalid door direction");
        }
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
            case RoomType.EntranceRoom:
                return new HashSet<DoorDir> { DoorDir.PosZ };
            case RoomType.ExitRoom:
                return new HashSet<DoorDir> { DoorDir.NegZ };
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