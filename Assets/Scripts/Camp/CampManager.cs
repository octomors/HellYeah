using UnityEngine;

public class CampManager : MonoBehaviour
{
	[SerializeField] private Transform campSpawnPoint;

	private void Start()
	{
		//SpawnPlayerInCamp();
		PlayerCombatManager combatManager = FindAnyObjectByType<PlayerCombatManager>();
		if (combatManager != null)
		{
			combatManager.RestoreHealth();
		}
	}
	

	public void SpawnPlayerInCamp()
	{
		PlayerSpawner.Instance.SpawnPlayer(campSpawnPoint.position, campSpawnPoint.rotation);
	}
}
