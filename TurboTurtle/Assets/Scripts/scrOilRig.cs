using UnityEngine;
using System.Collections;

public class scrOilRig : MonoBehaviour
{
	public GameObject FireballPrefab;

	private float explodeDistance;
	private const float EXPLODE_DISTANCE_MIN = 0.75f;
	private const float EXPLODE_DISTANCE_MAX = 0.9f;

	private int fireballs;
	private const int FIREBALLS_MIN = 10;
	private const int FIREBALLS_MAX = 20;
	private const int FIREBALLS_PER_SPEW = 3;

	private float spewTimer = 0;
	private float spewDelay = 0;
	private const float SPEW_DELAY_MIN = 0.0f;
	private const float SPEW_DELAY_MAX = 1.0f;

	// Use this for initialization
	void Start ()
	{
		fireballs = Random.Range (FIREBALLS_MIN, FIREBALLS_MAX + 1);

		// Find a random distance to explode at from 50% to 75% the spawn distance.
		explodeDistance = Random.Range (EXPLODE_DISTANCE_MIN, EXPLODE_DISTANCE_MAX + 1) * scrWorldScroll.Z_INSTANTIATE;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (this.transform.position.z < explodeDistance)
		{
			// Display the destroyed parts of the rig.
			this.transform.Find ("Destroyed").gameObject.SetActive (true);

			// Spew fireballs.
			if (fireballs > 0)
			{
				spewTimer += Time.deltaTime;
				if (spewTimer >= spewDelay)
				{
					for (int i = 0; i < FIREBALLS_PER_SPEW; ++i)
					{
						Instantiate(FireballPrefab, this.transform.position + new Vector3(Random.Range (-this.transform.localScale.x, this.transform.localScale.x), 0, Random.Range(-this.transform.localScale.z, this.transform.localScale.z)) * 0.5f, Quaternion.identity);
						--fireballs;
						if (fireballs == 0)
							break;
					}

					spewDelay = Random.Range (SPEW_DELAY_MIN, SPEW_DELAY_MAX);
					spewTimer = 0;
				}
			}
		}
	}
}
