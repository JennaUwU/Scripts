using UnityEngine;

public class EnemyChecklist : MonoBehaviour
{
	private Color colorPositive = Color.green;

	private Color colorNegative = new Color(1f, 0.74f, 0.61f);

	[Space]
	public bool hasRigidbody;

	public new bool name;

	public bool difficulty;

	public bool type;

	public bool center;

	public bool killLookAt;

	public bool sightingStinger;

	public bool enemyNearMusic;

	public bool healthMax;

	public bool healthMeshParent;

	public bool healthOnHurt;

	public bool healthOnDeath;

	public bool healthImpact;

	public bool healthObject;

	public bool rigidbodyPhysAttribute;

	public bool rigidbodyAudioPreset;

	public bool rigidbodyColliders;

	public bool rigidbodyFollow;

	public bool rigidbodyCustomGravity;

	public bool rigidbodyGrab;

	public bool rigidbodyPositionFollow;

	public bool rigidbodyRotationFollow;

	private void ResetChecklist()
	{
		difficulty = false;
		type = false;
		center = false;
		killLookAt = false;
		sightingStinger = false;
		enemyNearMusic = false;
		healthMax = false;
		healthMeshParent = false;
		healthOnHurt = false;
		healthOnDeath = false;
		healthImpact = false;
		healthObject = false;
		rigidbodyPhysAttribute = false;
		rigidbodyAudioPreset = false;
		rigidbodyColliders = false;
		rigidbodyFollow = false;
		rigidbodyCustomGravity = false;
		rigidbodyGrab = false;
		rigidbodyPositionFollow = false;
		rigidbodyRotationFollow = false;
	}

	private void SetAllChecklist()
	{
		difficulty = true;
		type = true;
		center = true;
		killLookAt = true;
		sightingStinger = true;
		enemyNearMusic = true;
		healthMax = true;
		healthMeshParent = true;
		healthOnHurt = true;
		healthOnDeath = true;
		healthImpact = true;
		healthObject = true;
		rigidbodyPhysAttribute = true;
		rigidbodyAudioPreset = true;
		rigidbodyColliders = true;
		rigidbodyFollow = true;
		rigidbodyCustomGravity = true;
		rigidbodyGrab = true;
		rigidbodyPositionFollow = true;
		rigidbodyRotationFollow = true;
	}
}
