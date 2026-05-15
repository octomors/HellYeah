using System;
using System.Collections.Generic;
using System.Linq;

public class WaffleDungeonGenerator : IDungeonGenerator
{
    Map map = new Map();
    public Map Generate(FloorConfig config)
    {

        int roomCount = UnityEngine.Random.Range(config.MinRoomCount - 2, config.MaxRoomCount - 2 + 1);

        int width = (int)Math.Sqrt(roomCount) + 1;
        int height = (int)Math.Sqrt(roomCount) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map.roomCoords.Add((y, x));
            }
        }

        while (map.roomCoords.Count > roomCount)
        {
            var coords = map.roomCoords.OrderBy(c => UnityEngine.Random.value).First();
            map.roomCoords.Remove(coords);
        }

        GenerateEntranceAndExit();
        return map;
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
}