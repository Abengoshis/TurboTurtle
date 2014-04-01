using UnityEngine;
using System.Collections;

public class scrFireball : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		// Set default upwards velocity.
		this.rigidbody.velocity = Vector3.up * 40.0f;

		// Add a randomness.
		this.rigidbody.AddForce(new Vector3(Random.Range (-this.transform.position.x * 15, this.transform.position.x * 7f) * 0.02f, Random.Range (0f, 30f), Random.Range(-10f, 11f)), ForceMode.Impulse);
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Stop the fireball when it reaches the water.
		if (this.rigidbody != null && this.transform.position.y < 0)
		{
			// Remove the rigidbody, it has no further use.
			Destroy (this.rigidbody);

			// Attach a buoyancy script.
			this.gameObject.AddComponent<scrBuoyancy>();
		}
	}
}
