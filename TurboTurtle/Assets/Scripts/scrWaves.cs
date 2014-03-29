using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;

public class scrWaves : MonoBehaviour
{
	private static float waveSpeed = 2;
	private static float waveHeight = 1;
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
			float xWave = vertices[i].x / mesh.bounds.size.x;
			float zWave = vertices[i].z / mesh.bounds.size.z;

			vertices[i].y = waveHeight * Mathf.Sin (Time.time * waveSpeed + (xWave + zWave) * Mathf.PI * 2);
		}
		
		// Set the vertices back to the mesh and recalculate the normals.
		mesh.vertices = vertices;
		mesh.RecalculateNormals();

		// Reset and update the mesh of the collider.
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;	// Set the mesh after nullifying it so the vertices will be forced to update. (This is just how Unity works.)

		this.renderer.material.mainTextureOffset += new Vector2(0.02f, -0.01f) * Time.deltaTime;
	}
}
