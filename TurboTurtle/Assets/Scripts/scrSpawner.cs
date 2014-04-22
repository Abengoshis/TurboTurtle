using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrSpawner : MonoBehaviour
{
	public GameObject SurfaceDebrisPrefab;
	public GameObject UnderwaterDebrisPrefab;
	public GameObject OilPrefab;
	public GameObject FirePrefab;
	public GameObject OilRigPrefab;
	public GameObject OneBoatOneNetPrefab;
	public GameObject TwoBoatsOneNetPrefab;
	public GameObject SeaweedPrefab;
	public GameObject CleanerFishPrefab;
	public GameObject FlyingFishPrefab;
	public GameObject TurtlePrefab;
	
	public GameObject[] SceneryPrefabs;
	public GameObject[] UnderwaterPrefabs;
	private Transform[] underwaterPool = new Transform[400];
	
	private float spawnTimer = 0;
	private float spawnDelay = 0;
	private const float SPAWN_DELAY_MIN = 2.0f;
	private const float SPAWN_DELAY_MAX = 4.0f;
	private const float SPAWN_DELAY_VARIANCE = 1.0f;
	
	private int spawnsUntilSpecial;
	private const int SPECIAL_WAIT_MIN = 5;
	private const int SPECIAL_WAIT_MAX = 10;
	
	private int spawnsUntilScenery;
	private const int SCENERY_WAIT_MIN = 3;
	private const int SCENERY_WAIT_MAX = 8;
	
	private const int SPAWN_AMOUNT_MIN = 1;
	private const int SPAWN_AMOUNT_MAX = 4;
	private const int SPAWN_AMOUNT_VARIANCE = 2;
	
	public const int NUMBER_OF_LANES = 4;
	private static Vector3[] lanes = new Vector3[NUMBER_OF_LANES];
	private static Vector3[] boatLanes = new Vector3[2];
	private static Vector3[] rigLanes = new Vector3[4];

	public static float GetLaneX(int lane)
	{
		return lanes[lane].x;
	}

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
		spawnsUntilScenery = SCENERY_WAIT_MIN;

		// Generate a pool of underwater objects.
		for (int i = 0; i < underwaterPool.Length; ++i)
		{
			underwaterPool[i] = ((GameObject)Instantiate (UnderwaterPrefabs[Random.Range (0, UnderwaterPrefabs.Length)], new Vector3(Random.Range(-499, 500), 0, Random.Range(0, 1000)), Random.rotation)).transform;
			underwaterPool[i].localScale = Random.Range (0.2f, 1f) * Vector3.one;

			RaycastHit hit;
			Physics.Raycast(new Ray(new Vector3(underwaterPool[i].position.x, -30, underwaterPool[i].position.z), Vector3.down), out hit, 100, 1 << LayerMask.NameToLayer("Water"));
			underwaterPool[i].position = hit.point;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Increase the spawn timer by a factor of the player's speed in order to scale the distance between lanes appropriately with speed.
		spawnTimer += Time.deltaTime * scrPlayer.Speed / scrPlayer.SCROLL_SPEED_MIN;
		if (spawnTimer > spawnDelay)
		{
			StartCoroutine("SpawnStandard");
			
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

		ManageUnderwaterPool();
	}

	void ManageUnderwaterPool()
	{
		foreach (Transform u in underwaterPool)
		{
			u.Translate(0, 0, -scrPlayer.Speed * Time.deltaTime, Space.World);

			// Loop the object but also randomise its position on the sea floor and raycast down to put it on the ground.
			if (u.position.z < scrWorldScroll.Z_DESTROY)
			{
				u.position = new Vector3(Random.Range(-499, 500), -40, scrWorldScroll.Z_INSTANTIATE);
				u.rotation = Random.rotation;
				u.localScale = Random.Range (0.2f, 1f) * Vector3.one;
				RaycastHit hit;
				if (Physics.Raycast(new Ray(new Vector3(u.position.x, -30, u.position.z), Vector3.down), out hit, 100, 1 << LayerMask.NameToLayer("Water")))
					u.position = hit.point;
				else
					Debug.Log ("what");
			}
		}
	}
	
	IEnumerator SpawnStandard()
	{
		// Set the amount to spawn to a random amount between the amount dictated by the distance, with a random variance, clamped to the minimum and maximum amounts.
		int spawnAmount = Random.Range (SPAWN_AMOUNT_MIN, Mathf.Clamp(Mathf.RoundToInt (Mathf.Lerp (SPAWN_AMOUNT_MIN, SPAWN_AMOUNT_MAX, scrPlayer.DistanceFactor)) + Random.Range(-SPAWN_AMOUNT_VARIANCE, SPAWN_AMOUNT_VARIANCE + 1), SPAWN_AMOUNT_MIN, SPAWN_AMOUNT_MAX) + 1);
		
		Debug.Log ("Spawning " + spawnAmount + " objects.");
		
		// Copy the spawn positions to a list.
		List<Vector3> spawnPoints = new List<Vector3>();
		spawnPoints.AddRange(lanes);
		
		// Choose whether to spawn debris or other objects.
		bool spawnDebrisOnly = Random.Range (0, 2) == 0;
		
		// Loop until the spawn amount has been reduced to 0 or all lanes have had something spawned at their location (this latter case should never happen).
		for (int i = spawnPoints.Count - 1; i >= 0 && spawnAmount > 0; --i)
		{
			// Find a random spawn point within the spawn point list.
			int s = Random.Range (0, spawnPoints.Count);
			Vector3 offset = new Vector3(0, 0, Random.Range (-2.0f, 2.0f));
			Quaternion rotation = Quaternion.Euler(0, Random.Range (0f, 360f), 0);

			if (spawnDebrisOnly == true)
			{
				// Choose between surface and underwater debris.
				if (Random.Range (0, 2) == 0)
					Instantiate (SurfaceDebrisPrefab, spawnPoints [s] + offset, rotation);
				else
					Instantiate (UnderwaterDebrisPrefab, spawnPoints[s] + offset, rotation);
			}
			else
			{
				// More likely to spawn obstacle than powerup.
				if (Random.Range (0, 5) < 3)
				{
					if (Random.Range (0, 2) == 0)
					{
						Instantiate(FirePrefab, spawnPoints[s] + offset, rotation);
					}
					else
					{
						Instantiate(OilPrefab, spawnPoints[s] + offset, rotation);
					}
				}
				else
				{
					// Instantiate a powerup at the spawn point.
					int item = Random.Range (0, 5);
					switch (item)
					{
					case 0:
					case 1:
						Instantiate(CleanerFishPrefab, spawnPoints[s] + offset, rotation);
						break;
					case 2:
						Instantiate(SeaweedPrefab, spawnPoints[s] + offset, rotation);
						break;
					case 3:
						Instantiate(FlyingFishPrefab, spawnPoints[s] + offset, rotation);
						break;
					case 4:
						Instantiate(TurtlePrefab, spawnPoints[s] + offset, rotation);
						break;
					}
				}
			}
			
			// Remove the spawn point from the list so objects don't overlap.
			spawnPoints.RemoveAt (s);
			
			// Decrement the spawnAmount.
			--spawnAmount;

			yield return new WaitForSeconds(0.1f);
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
		int prefab = Random.Range (0, SceneryPrefabs.Length);
		GameObject scenery = (GameObject)Instantiate (SceneryPrefabs[prefab], new Vector3(Random.Range (-20f, 20f), SceneryPrefabs[prefab].transform.position.y, scrWorldScroll.Z_INSTANTIATE + 200 + Random.Range (0, 20f)), Quaternion.identity);
		
		// Chance to flip scenery.
		if (Random.Range (0, 2) == 0)
			scenery.transform.localScale = new Vector3(-1, 1, 1);
		
		Debug.Log ("Spawning underwater object.");
	}
}
