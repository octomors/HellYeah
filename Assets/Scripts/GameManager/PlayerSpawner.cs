using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    public Transform targetSpawnPointTransform;
    private GameObject playerInstance;

    private void Awake()
    {
        playerInstance = GameObject.FindGameObjectWithTag("Player");

        if (playerInstance == null)
        {
            Debug.Log("Spawning player at " + targetSpawnPointTransform.position);
            playerInstance = Instantiate(playerPrefab);
        }

        // Reuse existing player across scenes and place at this scene's spawn.
        playerInstance.transform.position = targetSpawnPointTransform.position;
    }
}

