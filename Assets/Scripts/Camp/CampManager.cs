using UnityEngine;

public class CampManager : MonoBehaviour
{
	[SerializeField] private Transform campSpawnPoint;

	private void Start()
	{
		SpawnPlayerInCamp();
	}
	

	public void SpawnPlayerInCamp()
	{
		PlayerSpawner.Instance.SpawnPlayer(campSpawnPoint.position, campSpawnPoint.rotation);
	}
}
