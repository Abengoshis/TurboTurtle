using UnityEngine;
using System.Collections;

public class scrBuoyancy : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Cast a ray downwards from an area far above the water to get the top of the water's waves.
		RaycastHit hit;
		if (Physics.Raycast(this.transform.position + Vector3.up * 10, Vector3.down, out hit, 100, 1 << LayerMask.NameToLayer("Water")))
		{
			// Place the object's centre on top of the water.
			this.transform.position = hit.point;
		}
	}
}
