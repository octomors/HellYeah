using System.Collections.Generic;

public class Map
{
    public HashSet<(int y, int x)> roomCoords;
    public (int y, int x) EntranceRoomCoords;
    public (int y, int x) ExitRoomCoords;
    public Map()
    {
        roomCoords = new HashSet<(int y, int x)>();
    }
}