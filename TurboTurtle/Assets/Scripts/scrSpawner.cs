using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrSpawner : MonoBehaviour
{
	public GameObject DebrisPrefab;

	private float spawnTimer = 0;
	private float spawnDelay = 0;
	private const float SPAWN_DELAY_MIN = 1.0f;
	private const float SPAWN_DELAY_MAX = 5.0f;
	private const float SPAWN_DELAY_VARIANCE = 2.0f;

	private const int SPAWN_AMOUNT_MIN = 1;
	private const int SPAWN_AMOUNT_MAX = 4;
	private const int SPAWN_AMOUNT_VARIANCE = 2;

	private Vector3[] spawns;

	// Use this for initialization
	void Start ()
	{
		this.transform.position = new Vector3(0, 0, scrWorldScroll.Z_INSTANTIATE);

		Transform[] children = GetComponentsInChildren<Transform>();

		// Populate the spawn positions array.
		spawns = new Vector3[children.Length - 1];
		for (int i = 0, s = 0; i < children.Length && s < spawns.Length; ++i)
		{
			if (children[i].gameObject != this.gameObject)
			{
				spawns[s] = children[i].position;
				spawns[s].z = scrWorldScroll.Z_INSTANTIATE;
				++s;
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		spawnTimer += Time.deltaTime;
		if (spawnTimer > spawnDelay)
		{
			SpawnStuff();

			// Set the spawn delay to the delay dictated by the distance, with a random variance, clamped to the minimum and maximum amounts. Note that the minimum delay is the final value reached, rather than the maximum delay, causing the frequency of spawns to become smaller.
			spawnDelay = Mathf.Clamp (Mathf.Lerp (SPAWN_DELAY_MAX, SPAWN_DELAY_MIN, scrPlayer.DistanceFactor) + Random.Range (-SPAWN_DELAY_VARIANCE, SPAWN_DELAY_VARIANCE), SPAWN_DELAY_MIN, SPAWN_DELAY_MAX);
			spawnTimer = 0;
		}
	}

	void SpawnStuff()
	{
		// Set the amount to spawn to a random amount between the amount dictated by the distance, with a random variance, clamped to the minimum and maximum amounts.
		int spawnAmount = Random.Range (SPAWN_AMOUNT_MIN, Mathf.Clamp(Mathf.RoundToInt (Mathf.Lerp (SPAWN_AMOUNT_MIN, SPAWN_AMOUNT_MAX, scrPlayer.DistanceFactor)) + Random.Range(-SPAWN_AMOUNT_VARIANCE, SPAWN_AMOUNT_VARIANCE + 1), SPAWN_AMOUNT_MIN, SPAWN_AMOUNT_MAX) + 1);

		Debug.Log ("Spawning " + spawnAmount + " objects.");

		// Copy the spawn positions to a list.
		List<Vector3> spawnPoints = new List<Vector3>();
		spawnPoints.AddRange(spawns);

		// Loop until the spawn amount has been reduced to 0 or all spawns have had something spawned at their location (this latter case should never happen).
		for (int i = spawnPoints.Count - 1; i >= 0 && spawnAmount > 0; --i)
		{
			// Find a random spawn point within the spawn point list.
			int s = Random.Range (0, spawnPoints.Count);

			// Instantiate an object at the spawn point.
			Instantiate (DebrisPrefab, spawnPoints[s], Quaternion.identity);

			// Remove the spawn point from the list so objects don't overlap.
			spawnPoints.RemoveAt (i);

			// Decrement the spawnAmount.
			--spawnAmount;
		}
	}
}
