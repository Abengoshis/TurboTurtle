﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrPlayer : MonoBehaviour
{
	public static float TimeSinceStart { get; private set; }
	public const float TIME_CAP = 300.0f;
	public static float TimeFactor { get { return Mathf.Min (TimeSinceStart / TIME_CAP, 1.0f); } }
	
	public static float DistanceSinceStart { get; private set; }
	public const float DISTANCE_CAP = 2000.0f;
	public static float DistanceFactor { get { return Mathf.Min (DistanceSinceStart / DISTANCE_CAP, 1.0f); } }
	
	// Scroll speed properties.
	public static float Speed { get; private set; }		// The speed that the world scrolls backwards at.
	public static float ScrollSpeed { get; private set; }	// The target speed that the speed accelerates to.
	public const float SCROLL_SPEED_MIN = 20;
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
	private const float FLYING_HEIGHT = 5f;
	private const float FLYING_BOOST = 2;

	private const float SEAWEED_HEAL = 10.0f;
	
	// Debuffs
	private const float NET_SLOW = 0;
	
	private byte debrisDebuffs = 0;
	private const byte DEBRIS_STACKS_MAX = 3;
	private const float DEBRIS_SLOW = 0.97f;
	private const float DEBRIS_DOT = 0.025f;
	
	private bool oilDebuff = false;
	private float oilBurnTimer = 0;
	private const float OIL_BURN_DURATION = 5.0f;
	private const float OIL_BURN_MULTIPLIER = 2.0f;
	private const float OIL_SLOW = 0.7f;
	
	private bool fireDebuff = false;
	private const float FIRE_BOOST = 1.5f;
	private const float FIRE_DOT = 0.5f;
	
	// Turtle properties.
	private int lane = 1;
	private float laneX;
	private Transform wake;

	private const float HEALTH_MAX = 100;
	private float health = HEALTH_MAX;
	
	private bool diving = false;
	private bool diveInputPressed = false;
	private float diveTransitionTimer = 1;
	private const float DIVE_DEPTH = 10f;
	private const float DIVE_TRANSITION_DURATION = 1.0f;
	private const float DIVE_STRAFE_SLOW = 0.1f;

	public Texture DebrisImage;
	public Texture HealthImage;
	public Material[] AllCoralMaterials;
	private Color[] originalCoralColours;
	private float coralState = 1;

	void OnGUI()
	{
		GUI.skin.box.normal.textColor = Color.white;
		GUI.Box (new Rect(32, 32, 64, Screen.height - 64), "Health");
		GUI.DrawTexture(new Rect(36, Mathf.Lerp (Screen.height - 68, 64, health / HEALTH_MAX), 56, Mathf.Lerp (0, Screen.height - 102, health / HEALTH_MAX)), HealthImage);
		GUI.Box (new Rect(Screen.width / 2 - 128 , 16, 256, 40), "Distance Travelled\n" + (int)(DistanceSinceStart / 20) + "m");
		float speedFactor = (Speed - SCROLL_SPEED_MIN) / (SCROLL_SPEED_MAX - SCROLL_SPEED_MIN);
		GUI.Box (new Rect(Screen.width / 2 - 128 , Screen.height - 56, 256, 40), "Speed\n" + (speedFactor > 0.75f ? "VERY FAST" : speedFactor > 0.5f ? "FAST" : speedFactor > 0.25f ? "AVERAGE" : "SLOW"));


		for (int i = 0; i < debrisDebuffs; ++i)
		{
			GUI.DrawTexture(new Rect(Screen.width - 160, Screen.height / 2 + 96 - i * 160, i + 128, 128), DebrisImage);
		}
	}

	// Use this for initialization
	void Start ()
	{
		Speed = SCROLL_SPEED_MAX;
		ScrollSpeed = SCROLL_SPEED_MIN;
		Acceleration = ACCELERATION_MIN;
		TimeSinceStart = 0;
		laneX = scrSpawner.GetLaneX(lane);
		wake = transform.parent.Find ("Wake");
		originalCoralColours = new Color[AllCoralMaterials.Length];
		for (int i = 0; i < AllCoralMaterials.Length; ++i)
			originalCoralColours[i] = AllCoralMaterials[i].color;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Set the coral's state to become progressively worse with the lowering health, but to also never get better.
		float prevCoralState = coralState;
		coralState = Mathf.Min (coralState, health / HEALTH_MAX);
		// Check if the coral has changed.
		if (coralState != prevCoralState)
		{
			// Bleach the corals.
			for (int i = 0; i < AllCoralMaterials.Length; ++i)
			{
				AllCoralMaterials[i].color = Color.Lerp (new Color(1, 1, 1), originalCoralColours[i], coralState);
			}
 		}

		if (health <= 0)
			Application.LoadLevel("menu");

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
			diving = false;
			flyingTimer += Time.deltaTime;
			ScrollSpeed *= FLYING_BOOST;
			if (flyingTimer >= FLYING_DURATION)
			{
				flyingBuff = false;
				this.transform.Find ("FlyingBuff").gameObject.SetActive(false);
			}
		}
		
		if (debrisDebuffs > 0)
		{
			float slow = DEBRIS_SLOW / (1 + debrisDebuffs / 3);
			float dot = DEBRIS_DOT * debrisDebuffs;
			
			ScrollSpeed *= slow;
			Acceleration *= slow;
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
					audio.Stop ();

					this.transform.Find ("FireDebuff").gameObject.SetActive(false);
				}
				
				// Take damage over time.
				health -= FIRE_DOT * Time.deltaTime;
			}
			
			// Increase the speed.
			ScrollSpeed *= FIRE_BOOST;
			Acceleration *= FIRE_BOOST;
		}
		
		if (oilDebuff == true)
		{
			// Decrease the speed.
			ScrollSpeed *= OIL_SLOW;
			Acceleration *= OIL_SLOW;
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
		this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(laneX, this.transform.position.y, this.transform.position.z), 0.2f);
		wake.position = new Vector3(this.transform.position.x, wake.position.y, wake.position.z);

		if (diving == false)
		{
			if (Input.GetButtonDown("Horizontal"))
			{
				if (Input.GetAxis("Horizontal") < 0)
				{
					if (lane > 0)
					{
						--lane;
						laneX = scrSpawner.GetLaneX(lane);
					}
				}
				else
				{
					if (lane < scrSpawner.NUMBER_OF_LANES - 1)
					{
						++lane;
						laneX = scrSpawner.GetLaneX(lane);
					}
				}
			}
		}

		if (flyingBuff == false)
		{
			// Move the player up or down.
			this.transform.position = new Vector3(this.transform.position.x,
			                                      Mathf.SmoothStep(this.transform.position.y, -DIVE_DEPTH, diveTransitionTimer / DIVE_TRANSITION_DURATION),
			                                      this.transform.position.z);
		}
		else
		{
			// Move the player up or down.
			this.transform.position = new Vector3(this.transform.position.x,
			                                      Mathf.SmoothStep(this.transform.position.y, FLYING_HEIGHT, Mathf.Sin (Mathf.PI * flyingTimer / FLYING_DURATION)),
			                                      this.transform.position.z);
		}

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
		
		if (generic == "Obstacle" && flyingBuff == false)
		{
			wake.audio.pitch = Random.Range (0.8f, 1.2f);
			wake.audio.Play ();

			if (shellBuff == true)
			{
				shellBuff = false;
				this.transform.Find ("ShellBuff").gameObject.SetActive(false);
			}
			else
			{
				if (specific == "Fishing Boat" || specific == "Double Fishing Boat")
				{
					Speed *= NET_SLOW;
					Debug.Log ("boat");
				}
				else if (specific == "Oil" || specific == "Fire")
				{
					oilDebuff = true;
					oilBurnTimer = 0;
					
					this.transform.Find ("OilDebuff").gameObject.SetActive(true);
				}

				if (specific == "Fire" || specific == "Fireball")
				{
					fireDebuff = true;
					audio.Play ();
					
					this.transform.Find ("FireDebuff").gameObject.SetActive(true);
				}
				else if (specific == "SurfaceDebris" || specific == "UnderwaterDebris")
				{
					if (oilDebuff == true)
					{
						if (debrisDebuffs < DEBRIS_STACKS_MAX)
						{
							this.transform.Find ("DebrisDebuff_" + debrisDebuffs).gameObject.SetActive(true);


							Destroy (trigger.gameObject);

							++debrisDebuffs;
						}
					}

					Speed *= DEBRIS_SLOW / 4;
				}
				
				//Debug.Log ("+ Debuff: " + specific);
			}
		}
		else if (generic == "Powerup")
		{
			if (specific == "Turtle")
			{
				shellBuff = true;
				this.transform.Find ("ShellBuff").gameObject.SetActive(true);
				trigger.transform.Find ("Turtle6v19_AnimateDone").Find("Sphere").renderer.enabled = false;
			}
			else if (specific == "Seaweed")
			{
				health += SEAWEED_HEAL;
				if (health > 100)
					health = 100;
				
			}
			else if (specific == "Flying Fish")
			{
				if (flyingBuff == false)
				{
					flyingBuff = true;
					flyingTimer = 0;
					this.transform.Find ("FlyingBuff").gameObject.SetActive(true);
				}
			}
			else if (specific == "Cleaner Fish")
			{
				if (debrisDebuffs > 0)
				{
					Debug.Log ("DebrisDebuff_" + debrisDebuffs);
					--debrisDebuffs;
					this.transform.Find ("DebrisDebuff_" + debrisDebuffs).gameObject.SetActive(false);
				}
				else
				{
					oilDebuff = false;
					this.transform.Find ("OilDebuff").gameObject.SetActive(false);
				}
			}
			
			//Debug.Log ("+ Buff: " + specific);
		}
		
		// Ensure the other object does not loop when it goes out of the world.
		trigger.transform.root.GetComponent<scrWorldScroll>().Loop = false;
		
		// Set the colliders of all children to the Ignore Player layer.
		foreach (Transform t in trigger.transform.root.GetComponentsInChildren<Transform>())
			t.gameObject.layer = LayerMask.NameToLayer("Ignore Player");
		
	}

	void OnDestroy()
	{
		for (int i = 0; i < AllCoralMaterials.Length; ++i)
			AllCoralMaterials[i].color = originalCoralColours[i];
	}
}
