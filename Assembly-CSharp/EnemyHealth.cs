using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
	private PhotonView photonView;

	private Enemy enemy;

	public int health = 100;

	internal int healthCurrent;

	private bool deadImpulse;

	internal bool dead;

	private float deadImpulseTimer;

	public float deathFreezeTime = 0.1f;

	public bool impactHurt;

	public int impactLightDamage;

	public int impactMediumDamage;

	public int impactHeavyDamage;

	public bool objectHurt;

	public float objectHurtMultiplier = 1f;

	public bool objectHurtStun = true;

	internal float objectHurtStunTime = 2f;

	public Transform meshParent;

	private List<MeshRenderer> renderers;

	private List<Material> sharedMaterials = new List<Material>();

	internal List<Material> instancedMaterials = new List<Material>();

	public bool spawnValuable = true;

	public int spawnValuableMax = 3;

	internal int spawnValuableCurrent;

	internal Vector3 hurtDirection;

	private bool hurtEffect;

	private AnimationCurve hurtCurve;

	private float hurtLerp;

	public UnityEvent onHurt;

	private bool onHurtImpulse;

	public UnityEvent onDeathStart;

	public UnityEvent onDeath;

	public UnityEvent onObjectHurt;

	internal PlayerAvatar onObjectHurtPlayer;

	private int materialHurtColor;

	private int materialHurtAmount;

	internal float objectHurtDisableTimer;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		healthCurrent = health;
		hurtCurve = AssetManager.instance.animationCurveImpact;
		renderers = new List<MeshRenderer>();
		if ((bool)meshParent)
		{
			renderers.AddRange(meshParent.GetComponentsInChildren<MeshRenderer>(includeInactive: true));
		}
		foreach (MeshRenderer renderer in renderers)
		{
			Material material = null;
			foreach (Material sharedMaterial in sharedMaterials)
			{
				if (renderer.sharedMaterial.name == sharedMaterial.name)
				{
					material = sharedMaterial;
					renderer.sharedMaterial = instancedMaterials[sharedMaterials.IndexOf(sharedMaterial)];
				}
			}
			if (!material)
			{
				material = renderer.sharedMaterial;
				sharedMaterials.Add(material);
				instancedMaterials.Add(renderer.material);
			}
		}
		materialHurtColor = Shader.PropertyToID("_ColorOverlay");
		materialHurtAmount = Shader.PropertyToID("_ColorOverlayAmount");
		foreach (Material instancedMaterial in instancedMaterials)
		{
			instancedMaterial.SetColor(materialHurtColor, Color.red);
		}
	}

	private void Update()
	{
		if (hurtEffect)
		{
			hurtLerp += 2.5f * Time.deltaTime;
			hurtLerp = Mathf.Clamp01(hurtLerp);
			foreach (Material instancedMaterial in instancedMaterials)
			{
				instancedMaterial.SetFloat(materialHurtAmount, hurtCurve.Evaluate(hurtLerp));
			}
			if (hurtLerp > 1f)
			{
				hurtEffect = false;
				foreach (Material instancedMaterial2 in instancedMaterials)
				{
					instancedMaterial2.SetFloat(materialHurtAmount, 0f);
				}
			}
		}
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && deadImpulse)
		{
			deadImpulseTimer -= Time.deltaTime;
			if (deadImpulseTimer <= 0f)
			{
				if (!GameManager.Multiplayer())
				{
					DeathImpulseRPC();
				}
				else
				{
					photonView.RPC("DeathImpulseRPC", RpcTarget.All);
				}
			}
		}
		if (objectHurtDisableTimer > 0f)
		{
			objectHurtDisableTimer -= Time.deltaTime;
		}
		if (onHurtImpulse)
		{
			onHurt.Invoke();
			onHurtImpulse = false;
		}
	}

	public void OnSpawn()
	{
		if (hurtEffect)
		{
			hurtLerp = 1f;
			hurtEffect = false;
			foreach (Material instancedMaterial in instancedMaterials)
			{
				instancedMaterial.SetFloat(materialHurtAmount, 0f);
			}
		}
		healthCurrent = health;
		dead = false;
	}

	public void LightImpact()
	{
		if (impactHurt && enemy.IsStunned() && impactLightDamage > 0)
		{
			Hurt(impactLightDamage, -enemy.Rigidbody.impactDetector.previousPreviousVelocityRaw.normalized);
		}
	}

	public void MediumImpact()
	{
		if (impactHurt && enemy.IsStunned() && impactMediumDamage > 0)
		{
			Hurt(impactMediumDamage, -enemy.Rigidbody.impactDetector.previousPreviousVelocityRaw.normalized);
		}
	}

	public void HeavyImpact()
	{
		if (impactHurt && enemy.IsStunned() && impactHeavyDamage > 0)
		{
			Hurt(impactHeavyDamage, -enemy.Rigidbody.impactDetector.previousPreviousVelocityRaw.normalized);
		}
	}

	public void Hurt(int _damage, Vector3 _hurtDirection)
	{
		if (!dead)
		{
			healthCurrent -= _damage;
			if (healthCurrent <= 0)
			{
				healthCurrent = 0;
				Death(_hurtDirection);
			}
			else if (!GameManager.Multiplayer())
			{
				HurtRPC(_damage, _hurtDirection);
			}
			else
			{
				photonView.RPC("HurtRPC", RpcTarget.All, _damage, _hurtDirection);
			}
		}
	}

	[PunRPC]
	public void HurtRPC(int _damage, Vector3 _hurtDirection)
	{
		hurtDirection = _hurtDirection;
		hurtEffect = true;
		hurtLerp = 0f;
		if (hurtDirection == Vector3.zero)
		{
			hurtDirection = Random.insideUnitSphere;
		}
		onHurtImpulse = true;
	}

	private void Death(Vector3 _deathDirection)
	{
		if (!GameManager.Multiplayer())
		{
			DeathRPC(_deathDirection);
			return;
		}
		photonView.RPC("DeathRPC", RpcTarget.All, _deathDirection);
	}

	[PunRPC]
	public void DeathRPC(Vector3 _deathDirection)
	{
		hurtDirection = _deathDirection;
		hurtEffect = true;
		hurtLerp = 0f;
		deadImpulseTimer = deathFreezeTime;
		enemy.Freeze(deathFreezeTime);
		onDeathStart.Invoke();
		deadImpulse = true;
	}

	[PunRPC]
	public void DeathImpulseRPC()
	{
		deadImpulse = false;
		dead = true;
		if (hurtDirection == Vector3.zero)
		{
			hurtDirection = Random.insideUnitSphere;
		}
		onDeath.Invoke();
	}

	public void ObjectHurtDisable(float _time)
	{
		objectHurtDisableTimer = _time;
	}
}
