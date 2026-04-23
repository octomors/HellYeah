using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IDoorHandler
{
    public static GameManager Instance { get; private set; }

    private RunManager runManager;
    private SceneLoader sceneLoader;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        runManager = FindAnyObjectByType<RunManager>();
        sceneLoader = FindAnyObjectByType<SceneLoader>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartRun()
    {
        runManager.StartRun();
        sceneLoader.LoadDungeon();
    }

    public void CompleteFloor()
    {
        int nextFloor = runManager.NextFloor();
        if (nextFloor <= 3)
        {
            sceneLoader.LoadDungeon();
        }
        else
        {
            EndRun();
        }
    }

    public void EndRun()
    {
        sceneLoader.LoadCamp();
        runManager.Reset();
    }

    public void HandleDoor(DoorType type)
    {
        switch (type)
        {
            case DoorType.StartRun:
                StartRun();
                break;
            case DoorType.NextFloor:
                CompleteFloor();
                break;
            default:
                Debug.LogWarning($"Unhandled door type: {type}");
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DungeonDoor[] doors = FindObjectsByType<DungeonDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (DungeonDoor door in doors)
        {
            door.Init(this);
        }
    }
}
