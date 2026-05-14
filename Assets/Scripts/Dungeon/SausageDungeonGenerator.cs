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
            var direction = UnityEngine.Random.value > 0.5f ? Direction.Left : Direction.Right;
            GenerateBranch(branch.Key, direction, branch.Value);
        }

        GenerateEntranceAndExit();
        
        return map;
    }

    /// <summary>
    /// Generates the main path of the dungeon, which is a "sausage" of rooms.
    /// </summary>
    private void GenerateSausage()
    {
        var walker = new Walker((0, 0), Direction.Up, 0.6f, 0.2f, 0.2f);

        for (int i = 0; i < sausageSize; i++)
        {
            var nextPoint = walker.GetNextPoint();
            if (map.roomCoords.Contains(nextPoint))
            {
                i--;
                continue;
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

    private void GenerateEntranceAndExit()
    {
        if (map.roomCoords.Count < 2)
        {
            return;
        }

        List<(int y, int x)> rooms = map.roomCoords.ToList();
        (int y, int x) a = rooms[0];
        (int y, int x) b = rooms[1];
        int bestDistSq = -1;

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                int dy = rooms[j].y - rooms[i].y;
                int dx = rooms[j].x - rooms[i].x;
                int distSq = (dy * dy) + (dx * dx);
                if (distSq > bestDistSq)
                {
                    bestDistSq = distSq;
                    a = rooms[i];
                    b = rooms[j];
                }
            }
        }

        (int y, int x) stepA = StepAway(a, b);
        (int y, int x) stepB = StepAway(b, a);

        map.EntranceRoomCoords = (a.y + stepA.y, a.x + stepA.x);
        map.ExitRoomCoords = (b.y + stepB.y, b.x + stepB.x);
    }

    private static (int y, int x) StepAway((int y, int x) from, (int y, int x) to)
    {
        int dy = from.y - to.y;
        int dx = from.x - to.x;

        int absDy = Math.Abs(dy);
        int absDx = Math.Abs(dx);

        if (absDy >= absDx)
        {
            return (Sign(dy), 0);
        }

        return (0, Sign(dx));
    }

    private static int Sign(int value)
    {
        if (value > 0)
        {
            return 1;
        }

        if (value < 0)
        {
            return -1;
        }

        return 0;
    }

    private void CalculateIntegerSettings(FloorConfig config)
    {
        roomsToGenerate = UnityEngine.Random.Range(config.MinRoomCount - 2, config.MaxRoomCount - 2 + 1);
        sausageSize = Mathf.CeilToInt(roomsToGenerate * SausageSizeSettings);
        maxBranchSize = Mathf.CeilToInt(roomsToGenerate * MaxBranchSizeSettings);
    }
}
