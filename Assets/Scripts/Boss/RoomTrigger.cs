using UnityEngine;

/// <summary>
/// Tells the boss when the player enters/leaves.
/// </summary>
public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private BossController boss;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (boss != null)
            boss.OnPlayerEnterRoom();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (boss != null)
            boss.OnPlayerExitRoom();
    }
}