using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public bool IsRunActive { get; private set; }
    public int CurrentFloor { get; private set; }
    public int Seed { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Reset();
    }

    public void StartRun()
    {
        IsRunActive = true;
        CurrentFloor = 1;
        Seed = Random.Range(int.MinValue, int.MaxValue);
        Debug.Log($"Run started. Seed={Seed}, Floor={CurrentFloor}");
    }

    public int NextFloor()
    {
        if (!IsRunActive)
        {
            Debug.LogWarning("NextFloor called while run is not active.");
            return CurrentFloor;
        }

        CurrentFloor++;
        Debug.Log($"Moved to floor {CurrentFloor}");
        return CurrentFloor;
    }

    public void Reset()
    {
        IsRunActive = false;
        CurrentFloor = 0;
        Seed = 0;
    }
}
