using UnityEngine;
using System.Collections;

public class scrBuoyancy : MonoBehaviour
{
	public bool BuoyantChildren;
	private Transform[] children;

	// Use this for initialization
	void Start ()
	{
		children = this.GetComponentsInChildren<Transform>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		RaycastHit hit;
		if (BuoyantChildren == true)
		{
			// Loop through all children.
			for (int i = 0; i < children.Length; ++i)
			{
				if (this.transform != null && children[i] != null && children[i].gameObject.layer != LayerMask.NameToLayer("Ignore Buoyancy") && (children[i] == this.transform || children[i].parent == this.transform))
				{
					// Cast a ray downwards from an area far above the water to get the top of the water's waves.
					if (Physics.Raycast(children[i].position + Vector3.up * 200, Vector3.down, out hit, 300, 1 << LayerMask.NameToLayer("Water")))
					{
						// Place the object's centre on top of the water.
						children[i].position = hit.point;
					}
				}
			}
		}
		else
		{
			// Cast a ray downwards from an area far above the water to get the top of the water's waves.
			if (Physics.Raycast(this.transform.position + Vector3.up * 200, Vector3.down, out hit, 300, 1 << LayerMask.NameToLayer("Water")))
			{
				// Place the object's centre on top of the water.
				this.transform.position = hit.point;
			}
		}
	}
}
