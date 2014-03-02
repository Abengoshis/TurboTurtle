using UnityEngine;
using System.Collections;

public class scrPlayer : MonoBehaviour
{
	public float Speed;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		this.rigidbody.velocity = new Vector3(Input.GetAxis("Horizontal") * Speed, 0, 0);

	}
}
