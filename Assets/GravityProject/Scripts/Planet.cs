using UnityEngine;

public class Planet : MonoBehaviour
{
	[SerializeField] private int _priority;
	[SerializeField] private bool _blackHole = false;

	private void OnTriggerEnter(Collider other)
	{
		PlayerCharacterGalaxy player = other.GetComponent<PlayerCharacterGalaxy>();
		if (player != null)
		{
			if (player.PlanetPrio > _priority)
			{
				
				player.PlanetPrio = _priority;
				player.PlanetPuller = transform;
			}
		}
	}
	private void OnTriggerStay(Collider other)
	{
		
		PlayerCharacterGalaxy player = other.GetComponent<PlayerCharacterGalaxy>();
		if (player != null)
		{
			
			if (player.PlanetPrio > _priority)
			{
				player.PlanetPrio = _priority;
				player.PlanetPuller = transform;
			}
		}
	}
	private void OnTriggerExit(Collider other)
	{
		PlayerCharacterGalaxy player = other.GetComponent<PlayerCharacterGalaxy>();
		if (player != null)
		{
			player.PlanetPrio = int.MaxValue;
			player.PlanetPuller = null;
		}
	}
	private void OnCollisionEnter(Collision collision)
	{
		PlayerCharacterGalaxy player = collision.transform.GetComponent<PlayerCharacterGalaxy>();
		if (player != null)
		{
			player.Grounded = true;
		}
	}
	private void OnCollisionExit(Collision collision)
	{
		PlayerCharacterGalaxy player = collision.transform.GetComponent<PlayerCharacterGalaxy>();
		if (player != null)
		{
			player.Grounded = false;
		}
	}
}
