using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrSpawner : MonoBehaviour
{
	public GameObject DebrisPrefab;
	public GameObject OilPrefab;
	public GameObject FirePrefab;
	public GameObject OilRigPrefab;
	public GameObject OneBoatOneNetPrefab;
	public GameObject TwoBoatsOneNetPrefab;

	private float spawnTimer = 0;
	private float spawnDelay = 0;
	private const float SPAWN_DELAY_MIN = 1.0f;
	private const float SPAWN_DELAY_MAX = 10.0f;
	private const float SPAWN_DELAY_VARIANCE = 1.0f;

	private int spawnsUntilSpecial;
	private const int SPECIAL_WAIT_MIN = 5;
	private const int SPECIAL_WAIT_MAX = 10;

	private int spawnsUntilScenery;
	private const int SCENERY_WAIT_MIN = 3;
	private const int SCENERY_WAIT_MAX = 6;

	private const int SPAWN_AMOUNT_MIN = 1;
	private const int SPAWN_AMOUNT_MAX = 4;
	private const int SPAWN_AMOUNT_VARIANCE = 2;

	public const int NUMBER_OF_LANES = 5;
	private static Vector3[] lanes = new Vector3[NUMBER_OF_LANES];
	private static Vector3[] boatLanes = new Vector3[2];
	private static Vector3[] rigLanes = new Vector3[4];

	// Use this for initialization
	void Start ()
	{
		this.transform.position = new Vector3(0, 0, scrWorldScroll.Z_INSTANTIATE);

		Transform[] children = GetComponentsInChildren<Transform>();

		// Populate the spawn positions array.
		for (int i = 0, l = 0, b = 0, r = 0; i < children.Length; ++i)
		{
			if (children[i].name == "Spawner")
			{
				lanes[l] = children[i].position;
				lanes[l].z = scrWorldScroll.Z_INSTANTIATE;
				++l;
			}
			else if (children[i].name == "BoatLane")
			{
				boatLanes[b] = children[i].position;
				boatLanes[b].z = scrWorldScroll.Z_INSTANTIATE;
				++b;
			}
			else if (children[i].name == "RigLane")
			{
				rigLanes[r] = children[i].position;
				rigLanes[r].z = scrWorldScroll.Z_INSTANTIATE;
				++r;
			}
		}

		spawnsUntilSpecial = SPECIAL_WAIT_MIN;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Increase the spawn timer by a factor of the player's speed in order to scale the distance between lanes appropriately with speed.
		spawnTimer += Time.deltaTime * scrPlayer.Speed / scrPlayer.SCROLL_SPEED_MIN;
		if (spawnTimer > spawnDelay)
		{
			SpawnStandard();

			// Set the spawn delay to the delay dictated by the distance, with a random variance, clamped to the minimum and maximum amounts. Note that the minimum delay is the final value reached, rather than the maximum delay, causing the frequency of lanes to become smaller.
			spawnDelay = Mathf.Clamp (Mathf.Lerp (SPAWN_DELAY_MAX, SPAWN_DELAY_MIN, scrPlayer.DistanceFactor) + Random.Range (-SPAWN_DELAY_VARIANCE, SPAWN_DELAY_VARIANCE), SPAWN_DELAY_MIN, SPAWN_DELAY_MAX);
			spawnTimer = 0;

			// Reduce the number of lanes until a special object spawns.
			--spawnsUntilSpecial;
			if (spawnsUntilSpecial == 0)
			{
				SpawnSpecial();
				spawnsUntilSpecial = Random.Range (SPECIAL_WAIT_MIN, SPECIAL_WAIT_MAX + 1);
			}
			else
			{
				// Reduce the number of lanes until a scenery object spawns.
				--spawnsUntilScenery;
				if (spawnsUntilScenery == 0)
				{
					SpawnScenery();
					spawnsUntilScenery = Random.Range (SCENERY_WAIT_MIN, SCENERY_WAIT_MAX + 1);
				}
			}
		}
	}

	void SpawnStandard()
	{
		// Set the amount to spawn to a random amount between the amount dictated by the distance, with a random variance, clamped to the minimum and maximum amounts.
		int spawnAmount = Random.Range (SPAWN_AMOUNT_MIN, Mathf.Clamp(Mathf.RoundToInt (Mathf.Lerp (SPAWN_AMOUNT_MIN, SPAWN_AMOUNT_MAX, scrPlayer.DistanceFactor)) + Random.Range(-SPAWN_AMOUNT_VARIANCE, SPAWN_AMOUNT_VARIANCE + 1), SPAWN_AMOUNT_MIN, SPAWN_AMOUNT_MAX) + 1);

		Debug.Log ("Spawning " + spawnAmount + " objects.");

		// Copy the spawn positions to a list.
		List<Vector3> spawnPoints = new List<Vector3>();
		spawnPoints.AddRange(lanes);

		// Loop until the spawn amount has been reduced to 0 or all lanes have had something spawned at their location (this latter case should never happen).
		for (int i = spawnPoints.Count - 1; i >= 0 && spawnAmount > 0; --i)
		{
			// Find a random spawn point within the spawn point list.
			int s = Random.Range (0, spawnPoints.Count);

			// Instantiate an object at the spawn point.
			Instantiate (DebrisPrefab, spawnPoints[s] + new Vector3(0, 0, Random.Range (-2f, 2f)), Quaternion.identity);

			// Remove the spawn point from the list so objects don't overlap.
			spawnPoints.RemoveAt (i);

			// Decrement the spawnAmount.
			--spawnAmount;
		}
	}

	void SpawnSpecial()
	{
		int choice = Random.Range (0, 3);

		// Choice between oil rig, solo fishing boat and dual fishing boat.
		if (choice == 0)
		{
			// Choose a random spawn point.
			Vector3 spawnPoint = rigLanes[Random.Range (0, boatLanes.Length)];

			Instantiate (OilRigPrefab, spawnPoint, Quaternion.identity);
		}
		else if (choice == 1)
		{
			// Choose a random special spawn point from the centre two special spawn points.
			Vector3 spawnPoint = boatLanes[Random.Range (1, boatLanes.Length - 1)];

			Instantiate (OneBoatOneNetPrefab, spawnPoint, Quaternion.identity);

		}
		else if (choice == 2)
		{
			// Spawn the prefab at x = 0 so the boats are on both sides equally.
			Instantiate (TwoBoatsOneNetPrefab, new Vector3(0, 0, scrWorldScroll.Z_INSTANTIATE), Quaternion.identity);
		}

		Debug.Log ("Spawning special object.");
	}

	void SpawnScenery()
	{

	}
}
