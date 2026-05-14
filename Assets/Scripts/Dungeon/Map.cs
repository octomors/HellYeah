using System.Collections.Generic;

public class Map
{
    public HashSet<(int y, int x)> roomCoords;

    public Map()
    {
        roomCoords = new HashSet<(int y, int x)>();
    }
}