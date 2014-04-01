using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the way that every object in the game moves,
/// giving the illusion that the player is moving forwards
/// when in reality the world is moving backwards.
/// 
/// This helps the development in many ways, for example:
/// 	- Objects can be despawned when they reach an exact point behind the field of view.
/// 	- The player's position will never cause a stack overflow.
/// 	- The water planes need only be looped through positions rather than stepped along.
/// 
/// NOTE: There are two water textures at the moment. They are identical. The reason for this is
/// that the water stays still if its applied to more than one object and the objects are moved.
/// This is stupid and I'm blaming Unity!
/// </summary>
public class scrWorldScroll : MonoBehaviour
{
	public const float Z_INSTANTIATE = 500;	// The z value to instantiate objects at.
	public const float Z_DESTROY = -100;		// The z value to destroy objects at.

	public bool Loop = false;	// When behind the field of fiew: True = loop. False = destroy.

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Move this object backwards by the speed of the player.
		this.transform.Translate(0, 0, -scrPlayer.Speed * Time.deltaTime, Space.World);

		// Check if this object has gone entirely out of view.
		if (collider == null && this.transform.position.z < Z_DESTROY || collider != null && this.collider.bounds.max.z < Z_DESTROY)
		{
			// Check if the object should be destroyed or looped. Most frequent case: Loop = false.
			if (Loop == false)
			{
				// Destroy the object.
				Destroy (this.gameObject);
			}
			else
			{
				// Globally shift the object's z location to the Z_INSTANTIATE position, plus the length of the collider.
				this.transform.Translate (0, 0, Z_INSTANTIATE + this.collider.bounds.extents.z * 2, Space.World);
			}
		}
	}
}
