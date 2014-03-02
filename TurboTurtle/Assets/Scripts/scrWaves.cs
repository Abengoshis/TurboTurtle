using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;

public class scrWaves : MonoBehaviour
{
	private float waveSpeed = 2;
	private float waveHeight = 5;
	private Mesh mesh;
	
	// Use this for initialization
	void Start ()
	{
		mesh = this.GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Copy the vertices to a standard array.
		Vector3[] vertices = mesh.vertices;
		
		// Undulate each vertex by an amount dependant on their x and z position (making the waves horizontal).
		for (int i = 0; i < vertices.Length; i++)
		{
			float xWave = Mathf.Sin (vertices[i].x / mesh.bounds.size.x * Mathf.PI * 2);
			float zWave = Mathf.Sin (vertices[i].z / mesh.bounds.size.z * Mathf.PI * 2);

			vertices[i].y = waveHeight * Mathf.Sin (Time.time * waveSpeed * xWave + zWave);
		}
		
		// Set the vertices back to the mesh and recalculate the normals.
		mesh.vertices = vertices;
		mesh.RecalculateNormals();

		// Reset and update the mesh of the collider.
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;	// Set the mesh after nullifying it so the vertices will be forced to update. (This is just how Unity works.)
	}
}
