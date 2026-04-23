using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [SerializeField] private string campSceneName = "Camp";
    [SerializeField] private string dungeonSceneName = "Dungeon";

    public string CampSceneName => campSceneName;
    public string DungeonSceneName => dungeonSceneName;

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

    public void LoadCamp()
    {
        Debug.Log($"Loading camp scene '{campSceneName}'");
        SceneManager.LoadScene(campSceneName);
    }

    public void LoadDungeon()
    {
        Debug.Log($"Loading dungeon scene '{dungeonSceneName}'");
        SceneManager.LoadScene(dungeonSceneName);
    }
}
