using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private string targetSpawnPointName;

    private void Awake()
    {
        // Singleton + DontDestroyOnLoad через GameManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LoadScene(string sceneName, string spawnPointName)
    {
        targetSpawnPointName = spawnPointName;

        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player not found in scene!");
            Cleanup();
            return;
        }

        GameObject spawnPoint = GameObject.Find(targetSpawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogError($"Spawn point '{targetSpawnPointName}' not found in scene!");
            Cleanup();
            return;
        }

        // Телепорт игрока
        player.transform.position = spawnPoint.transform.position;

        Cleanup();
    }

    private void Cleanup()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        targetSpawnPointName = null;
    }
}