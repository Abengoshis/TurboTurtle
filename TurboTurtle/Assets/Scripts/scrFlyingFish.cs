using UnityEngine;
using System.Collections;

public class scrFlyingFish : MonoBehaviour
{
	private Transform[] fish;

	// Use this for initialization
	void Start ()
	{
		fish = this.GetComponentsInChildren<Transform> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < fish.Length; ++i)
		{
			if (fish[i].name != "Fish") continue;

			fish[i].rotation = Quaternion.Euler(fish[i].transform.eulerAngles.x, 30 * i, Time.time * 100 + 30 * i);
		}
	}
}
