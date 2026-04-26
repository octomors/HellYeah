using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Spawns the player at the specified coordinates and rotation. Or Teleports existing player if one is already present in the scene.
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="rotation"></param>
    public void SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null)
        {
            playerInstance = existingPlayer;
            playerInstance.transform.SetPositionAndRotation(position, rotation);
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned in PlayerSpawner.");
            return;
        }

        playerInstance = Instantiate(playerPrefab, position, rotation);
    }
}

