using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrPlayer : MonoBehaviour
{
	public static float TimeSinceStart { get; private set; }
	public const float TIME_CAP = 300.0f;
	public static float TimeFactor { get { return Mathf.Min (TimeSinceStart / TIME_CAP, 1.0f); } }

	public static float DistanceSinceStart { get; private set; }
	public const float DISTANCE_CAP = 300.0f;
	public static float DistanceFactor { get { return Mathf.Min (DistanceSinceStart / DISTANCE_CAP, 1.0f); } }

	// Scroll speed properties.
	public static float Speed { get; private set; }		// The speed that the world scrolls backwards at.
	public static float ScrollSpeed { get; private set; }	// The target speed that the speed accelerates to.
	public const float SCROLL_SPEED_MIN = 10;
	public const float SCROLL_SPEED_MAX = 40;

	// Acceleration properties.
	public static float Acceleration { get; private set; }
	private const float ACCELERATION_MIN = 1;
	private const float ACCELERATION_MAX = 6;

	// Buffs
	private bool shellBuff = false;

	private bool flyingBuff = false;
	private float flyingTimer = 0;
	private const float FLYING_DURATION = 5.0f;

	private const float SEAWEED_HEAL = 10.0f;

	// Debuffs
	private const float NET_SLOW = 0.1f;

	private byte debrisDebuffs = 0;
	private const byte DEBRIS_STACKS_MAX = 3;
	private const float DEBRIS_SLOW = 0.8f;
	private const float DEBRIS_DOT = 0.2f;

	private bool oilDebuff = false;
	private float oilBurnTimer = 0;
	private const float OIL_BURN_DURATION = 5.0f;
	private const float OIL_BURN_MULTIPLIER = 2.0f;
	private const float OIL_SLOW = 0.5f;

	private bool fireDebuff = false;
	private const float FIRE_BOOST = 2.0f;
	private const float FIRE_DOT = 0.5f;

	// Turtle properties.
	private float strafe = 0;	// Horizontal movement.
	private const float STRAFE_NORMAL = 10;

	private float health = 100;

	private bool diving = false;
	private bool diveInputPressed = false;
	private float diveTransitionTimer = 1;
	private const float DIVE_DEPTH = 10f;
	private const float DIVE_TRANSITION_DURATION = 2.0f;
	private const float DIVE_STRAFE_SLOW = 0.1f;

	// Use this for initialization
	void Start ()
	{
		strafe = STRAFE_NORMAL;
		ScrollSpeed = SCROLL_SPEED_MIN;
		Acceleration = ACCELERATION_MIN;
		TimeSinceStart = 0;
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetAxis ("Dive") != 0)
		{
			if (diveInputPressed == false && diveTransitionTimer == 0 || diveTransitionTimer == DIVE_TRANSITION_DURATION)
			{
				diving = !diving;
				diveInputPressed = true;
			}
		}
		else
		{
			diveInputPressed = false;
		}

		if (flyingBuff == true)
		{
			flyingTimer += Time.deltaTime;
			if (flyingTimer >= FLYING_DURATION)
				flyingBuff = false;
		}

		if (debrisDebuffs > 0)
		{
			float slow = DEBRIS_SLOW / debrisDebuffs;
			float dot = DEBRIS_DOT * debrisDebuffs;

			ScrollSpeed *= slow;
			Acceleration *= slow;
			strafe *= slow;
			health -= dot;
		}

		if (fireDebuff == true)
		{
			if (oilDebuff == true)
			{
				// Take extra damage over time.
				health -= FIRE_DOT * OIL_BURN_MULTIPLIER * Time.deltaTime;

				// Burn away the oil over time.
				oilBurnTimer += Time.deltaTime;
				if (oilBurnTimer >= OIL_BURN_DURATION)
				{
					oilDebuff = false;

					this.transform.Find ("OilDebuff").gameObject.SetActive(false);
				}

			}
			else
			{
				if (diveTransitionTimer == DIVE_TRANSITION_DURATION)
				{
					fireDebuff = false;

					this.transform.Find ("FireDebuff").gameObject.SetActive(false);
				}

				// Take damage over time.
				health -= FIRE_DOT * Time.deltaTime;
			}

			// Increase the speed.
			ScrollSpeed *= FIRE_BOOST;
			Acceleration *= FIRE_BOOST;
			strafe *= FIRE_BOOST;
		}

		if (oilDebuff == true)
		{
			// Decrease the speed.
			ScrollSpeed *= OIL_SLOW;
			Acceleration *= OIL_SLOW;
			strafe *= OIL_SLOW;
		}

		if (diving == true)
		{
			diveTransitionTimer += Time.deltaTime;
			if (diveTransitionTimer > DIVE_TRANSITION_DURATION)
				diveTransitionTimer = DIVE_TRANSITION_DURATION;
		}
		else
		{
			diveTransitionTimer -= Time.deltaTime;
			if (diveTransitionTimer < 0)
				diveTransitionTimer = 0;
		}

		// Accelerate the speed to the scroll speed.
		if (Speed < ScrollSpeed)
		{
			Speed += Acceleration * Time.deltaTime;
			if (Speed > ScrollSpeed)
				Speed = ScrollSpeed;
		}
		else if (Speed > ScrollSpeed)	// Decelerate the speed to the scroll speed at the maximum acceleration. This will happen after being on fire.
		{
			Speed -= ACCELERATION_MAX * Time.deltaTime;
			if (Speed < ScrollSpeed)
				Speed = ScrollSpeed;
		}
		
		// Move the player left or right.
		this.transform.root.rigidbody.velocity = new Vector3(Input.GetAxis("Horizontal") * Mathf.Lerp (strafe, strafe * DIVE_STRAFE_SLOW, diveTransitionTimer / DIVE_TRANSITION_DURATION), 0, 0);

		// Move the player up or down.
		this.transform.position = new Vector3(this.transform.position.x,
		                                      Mathf.SmoothStep(this.transform.position.y, -DIVE_DEPTH, diveTransitionTimer / DIVE_TRANSITION_DURATION),
		                                      this.transform.position.z);
		
		// Move the camera to keep the player at the bottom of the screen.
		Camera.main.transform.position = new Vector3(Camera.main.transform.position.x,
		                                             Mathf.Lerp (Camera.main.transform.position.y, this.transform.root.position.y + 7.5f, 0.2f),
		                                             Mathf.SmoothStep (-7f, -16.5f, diveTransitionTimer / DIVE_TRANSITION_DURATION));

		// Count the time since the creation of this object.
		TimeSinceStart += Time.deltaTime;

		// Increase the distance travelled by the speed.
		DistanceSinceStart += Speed * Time.deltaTime;

		// Interpolate the scroll speed and acceleration of the player between the minimum and the maximum over 5 minutes.
		Acceleration = Mathf.Lerp (ACCELERATION_MIN, ACCELERATION_MAX, DistanceFactor);
		ScrollSpeed = Mathf.Lerp (SCROLL_SPEED_MIN, SCROLL_SPEED_MAX, DistanceFactor);
		strafe = STRAFE_NORMAL;
	}

	void OnCollisionEnter(Collision collision)
	{
		// Force the trigger enter function.
		//OnTriggerEnter(collision.collider);
	}

	void OnTriggerEnter(Collider trigger)
	{
		// Get the name of the object, minus the annoying (Clone) part.
		string specific = trigger.transform.root.name.Remove(trigger.transform.root.name.IndexOf('('));
		string generic = trigger.transform.root.tag;

		if (generic == "Obstacle")
		{
			if (specific == "Net")
			{
				Speed *= NET_SLOW;
				
			}
			else if (specific == "Oil")
			{
				oilDebuff = true;
				oilBurnTimer = 0;

				this.transform.Find ("OilDebuff").gameObject.SetActive(true);
			}
			else if (specific == "Fire" || specific == "Fireball")
			{
				fireDebuff = true;

				this.transform.Find ("FireDebuff").gameObject.SetActive(true);
			}
			else if (specific == "SurfaceDebris" || specific == "UnderwaterDebris")
			{
				if (debrisDebuffs < DEBRIS_STACKS_MAX)
				{
					this.transform.Find ("DebrisDebuff_" + debrisDebuffs).gameObject.SetActive(true);

					++debrisDebuffs;
				}
			}

			Debug.Log ("+ Debuff: " + specific);
		}
		else if (generic == "Powerup")
		{
			if (specific == "Turtle")
			{
				shellBuff = true;

			}
			else if (specific == "Seaweed")
			{
				health += SEAWEED_HEAL;
				if (health > 100)
					health = 100;

			}
			else if (specific == "Flying Fish")
			{
				flyingBuff = true;
				flyingTimer = 0;

			}
			else if (specific == "Cleaner Fish")
			{
				if (debrisDebuffs > 0)
					--debrisDebuffs;
			}

			Debug.Log ("+ Buff: " + specific);
		}

		// Ensure the other object does not loop when it goes out of the world.
		trigger.transform.root.GetComponent<scrWorldScroll>().Loop = false;

		// Set the colliders of all children to the Ignore Player layer.
		foreach (Transform t in trigger.transform.root.GetComponentsInChildren<Transform>())
			t.gameObject.layer = LayerMask.NameToLayer("Ignore Player");

	}
}
