using UnityEngine;
using System.Collections;

public class scrFlyingFish : MonoBehaviour
{
	private Transform[] fish;
	private float[] fishRotationOffsets;
	private int[] fishDirections;

	// Use this for initialization
	void Start ()
	{
		fish = this.GetComponentsInChildren<Transform> ();
		fishRotationOffsets = new float[fish.Length];
		fishDirections = new int[fish.Length];
		for (int i = 0; i < fish.Length; ++i)
		{
			fishRotationOffsets [i] = Random.Range (0, 360);
			fishDirections[i] = Random.Range (0, 2) == 0 ? 1 : -1;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < fish.Length; ++i)
		{
			if (fish[i].name != "Fish") continue;

			fish[i].rotation = Quaternion.Euler(fish[i].transform.eulerAngles.x, fish[i].transform.eulerAngles.y, Time.time * 100 * fishDirections[i] + fishRotationOffsets[i]);
		}
	}

	void OnDestroy()
	{
		for (int i = 0; i < fish.Length; ++i)
		{
			Destroy (fish[i].gameObject);
		}
	}
}
