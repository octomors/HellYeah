using UnityEngine;

public class CampManager : MonoBehaviour
{
	[SerializeField] private Transform campSpawnPoint;

	private void Awake()
	{
		SpawnPlayerInCamp();
	}

	public void SpawnPlayerInCamp()
	{
		PlayerSpawner.Instance.SpawnPlayer(campSpawnPoint.position, campSpawnPoint.rotation);
	}
}
