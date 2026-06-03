using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [SerializeField] private string campSceneName = "Camp";
    [SerializeField] private string dungeonSceneName = "Dungeon";
    [SerializeField] private string bossSceneName = "Boss";

    public string CampSceneName => campSceneName;
    public string DungeonSceneName => dungeonSceneName;
    public string BossSceneName => bossSceneName;

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
        Debug.Log($"Loading camp scene");
        SceneManager.LoadScene(campSceneName);
    }

    public void LoadDungeon()
    {
        Debug.Log($"Loading dungeon scene");
        SceneManager.LoadScene(dungeonSceneName);
    }

    public void LoadBoss()
    {
        Debug.Log("Loading boss scene");
        SceneManager.LoadScene(bossSceneName);
    }
}
