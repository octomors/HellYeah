using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[System.Serializable]
public class RoomPrefabGroup
{
    public RoomType type;
    public List<GameObject> prefabs = new List<GameObject>();
}

[CreateAssetMenu(fileName = "FloorConfig", menuName = "Dungeon/Floor Config")]
public class FloorConfig : ScriptableObject
{
    public GenerationType GenerationType;
    public int MinRoomCount;
    public int MaxRoomCount;
    public List<RoomPrefabGroup> RoomPrefabGroups;

    public int CorridorLength;
    public int DoorsToRoomOriginLength;

    private bool cacheBuilded;
    private Dictionary<RoomType, List<GameObject>> prefabs;
    /// <summary>
    /// O(1) runtime cache for PrefabGroups
    /// </summary>
    public Dictionary<RoomType, List<GameObject>> RoomPrefabs
    {
        get
        {
            if (!cacheBuilded)
            {
                prefabs = RoomPrefabGroups.ToDictionary(g => g.type, g => g.prefabs);
                cacheBuilded = true;
            }
            return prefabs;
        }
    }
}
