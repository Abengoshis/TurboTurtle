using UnityEngine;
using System.Collections;

public class scrCleanerFish : MonoBehaviour
{
	struct FishAgent
	{
		private Transform transform;
		private Vector3 origin, target;
		private const float TARGET_RADIUS = 4.0f;
		private float moveTimer, moveDelay;
		private const float DELAY_MIN = 2.0f, DELAY_MAX = 4.0f;

		public FishAgent(Transform transform)
		{
			this.transform = transform;
			origin = Vector3.zero;
			target = origin;
			moveTimer = 0;
			moveDelay = 0;

			// Set the colour to a random hue.
			float h = Random.Range (0, 360);
			float x = 1 - Mathf.Abs ((h / 60f) % 2 - 1);
			Color c = Color.black;
			if (h < 60)
			{
				c.r = 1;
				c.g = x;
			}
			else if (h < 120)
			{
				c.r = x;
				c.g = 1;
			}
			else if (h < 180)
			{
				c.g = 1;
				c.b = x;
			}
			else if (h < 240)
			{
				c.g = x;
				c.b = 1;
			}
			else if (h < 300)
			{
				c.r = x;
				c.b = 1;
			}
			else
			{
				c.r = 1;
				c.b = x;
			}

			// Apply the colour to the fish by generating a new material.
			Material temp = new Material(transform.renderer.material);
			temp.color = c;
			transform.renderer.material = temp;
		}

		public void Update()
		{
			moveTimer += Time.deltaTime;
			if (moveTimer >= moveDelay)
			{
				// Set the origin to the target position.
				origin = target;

				// Set the target to a new position.
				target = Random.insideUnitSphere * TARGET_RADIUS;

				// Set a random delay and reset the timer.
				moveDelay = Random.Range (DELAY_MIN, DELAY_MAX);
				moveTimer = 0;
			}

			transform.localPosition = Vector3.Lerp (origin, target, Mathf.SmoothStep(0f, 1f, moveTimer / moveDelay));
			Quaternion currentRotation = transform.rotation;
			transform.LookAt(transform.parent.position + target);
			transform.eulerAngles = new Vector3(90, transform.eulerAngles.y + 270, transform.eulerAngles.z);
			Quaternion lookRotation = transform.rotation;
			transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.RotateTowards(currentRotation, lookRotation, 45), 0.5f / moveDelay);
		}
	}

	private FishAgent[] fish;

	// Use this for initialization
	void Start ()
	{
		Transform[] children = this.GetComponentsInChildren<Transform> ();
		int count = 0;
		for (int i = 0; i < children.Length; ++i)
			if (children[i].name == "Fish")
				++count;

		fish = new FishAgent[count];
		count = 0;
		foreach (Transform child in children)
		{
			if (child.name == "Fish")
				fish[count++] = new FishAgent(child);

			if (count == fish.Length)
				break;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < fish.Length; ++i)
		{
			fish[i].Update();
		}
	}
}
