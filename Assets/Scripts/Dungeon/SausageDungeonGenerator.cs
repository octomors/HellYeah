using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SausageDungeonGenerator : IDungeonGenerator
{
    #region Settings
    private float SausageSizeSettings = 0.666f;
    private float MaxBranchSizeSettings = 0.333f;
    #endregion

    private Map map;

    private int roomsToGenerate;
    private int sausageSize;
    private int maxBranchSize;
    public SausageDungeonGenerator()
    {
        map = new Map();
    }
    public Map Generate(FloorConfig config)
    {
        CalculateIntegerSettings(config);

        GenerateSausage();

        Dictionary<(int x, int y), int> Branches = new Dictionary<(int x, int y), int>();
        var candidates = map.roomCoords
            .Except(new[] { map.EntranceRoomCoords, map.ExitRoomCoords })
            .OrderBy(coords => UnityEngine.Random.value)
            .ToList();
        foreach (var branchStart in candidates)
        {
            if (roomsToGenerate == 0)
            {
                break;
            }

            int branchLength = UnityEngine.Random.Range(1, Math.Min(roomsToGenerate, maxBranchSize) + 1);
            Branches[branchStart] = branchLength;
            roomsToGenerate -= branchLength;
        }

        foreach (var branch in Branches)
        {
            Debug.Log($"Generating branch at {branch.Key} with length {branch.Value}");
            var direction = UnityEngine.Random.value > 0.5f ? Direction.Left : Direction.Right;
            GenerateBranch(branch.Key, direction, branch.Value);
        }
        Debug.Log($"Sausage size: {sausageSize}, Branches count: {Branches.Count}, Total rooms: {map.roomCoords.Count}");
        return map;
    }

    /// <summary>
    /// Generates the main path of the dungeon, which is a "sausage" of rooms.
    /// </summary>
    private void GenerateSausage()
    {
        var walker = new Walker((0, 0), Direction.Up, 0.6f, 0.2f, 0.2f);

        map.EntranceRoomCoords = (0, 0);
        for (int i = 0; i < sausageSize; i++)
        {
            var nextPoint = walker.GetNextPoint();
            if (map.roomCoords.Contains(nextPoint))
            {
                i--;
                continue;
            }
            if(i == sausageSize - 1)
            {
                map.ExitRoomCoords = nextPoint;
            }
            map.roomCoords.Add(nextPoint);
        }

        roomsToGenerate -= sausageSize;
    }

    /// <summary>
    /// Generates a branch of rooms
    /// </summary>
    /// <param name="startCoords"></param>
    /// <param name="direction"></param>
    /// <param name="branchSize"></param>
    private void GenerateBranch((int y, int x) startCoords, Direction direction, int branchSize)
    {
        var walker = new Walker(startCoords, direction, 0.6f, 0.2f, 0.2f);

        for (int i = 0; i < branchSize; i++)
        {
            (int y, int x) nextPoint = walker.GetNextPoint();
            if (map.roomCoords.Contains(nextPoint))
            {
                i--;
                continue;
            }
            map.roomCoords.Add(nextPoint);
        }
    }

    private void CalculateIntegerSettings(FloorConfig config)
    {
        roomsToGenerate = UnityEngine.Random.Range(config.MinRoomCount, config.MaxRoomCount + 1);
        sausageSize = Mathf.CeilToInt(roomsToGenerate * SausageSizeSettings);
        maxBranchSize = Mathf.CeilToInt(roomsToGenerate * MaxBranchSizeSettings);
    }
}
