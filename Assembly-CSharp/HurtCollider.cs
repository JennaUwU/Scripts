using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class HurtCollider : MonoBehaviour
{
	public enum BreakImpact
	{
		None = 0,
		Light = 1,
		Medium = 2,
		Heavy = 3
	}

	public enum TorqueAxis
	{
		up = 0,
		down = 1,
		left = 2,
		right = 3,
		forward = 4,
		back = 5
	}

	public enum HitType
	{
		Player = 0,
		PhysObject = 1,
		Enemy = 2
	}

	public class Hit
	{
		public HitType hitType;

		public GameObject hitObject;

		public float cooldown;
	}

	public bool playerLogic = true;

	[Space]
	public bool playerKill = true;

	public int playerDamage = 10;

	public float playerDamageCooldown = 0.25f;

	public float playerHitForce;

	public bool playerRayCast;

	public float playerTumbleForce;

	public float playerTumbleTorque;

	public TorqueAxis playerTumbleTorqueAxis = TorqueAxis.down;

	public float playerTumbleTime;

	public float playerTumbleImpactHurtTime;

	public int playerTumbleImpactHurtDamage;

	public bool physLogic = true;

	[Space]
	public bool physDestroy = true;

	public bool physHingeDestroy = true;

	public bool physHingeBreak;

	public BreakImpact physImpact = BreakImpact.Medium;

	public float physDamageCooldown = 0.25f;

	public float physHitForce;

	public float physHitTorque;

	public bool physRayCast;

	public bool enemyLogic = true;

	public Enemy enemyHost;

	[Space]
	[FormerlySerializedAs("enemyDespawn")]
	public bool enemyKill = true;

	public bool enemyStun = true;

	public float enemyStunTime = 2f;

	public EnemyType enemyStunType = EnemyType.Medium;

	public float enemyFreezeTime = 0.1f;

	[Space]
	public BreakImpact enemyImpact = BreakImpact.Medium;

	public int enemyDamage;

	public float enemyDamageCooldown = 0.25f;

	public float enemyHitForce;

	public float enemyHitTorque;

	public bool enemyRayCast;

	public bool enemyHitTriggers = true;

	[Range(0f, 180f)]
	public float hitSpread = 180f;

	public List<PhysGrabObject> ignoreObjects = new List<PhysGrabObject>();

	public UnityEvent onImpactAny;

	public UnityEvent onImpactPlayer;

	internal PlayerAvatar onImpactPlayerAvatar;

	public UnityEvent onImpactPhysObject;

	public UnityEvent onImpactEnemy;

	internal Enemy onImpactEnemyEnemy;

	private Collider Collider;

	private BoxCollider BoxCollider;

	private SphereCollider SphereCollider;

	private bool ColliderIsBox = true;

	private LayerMask LayerMask;

	private LayerMask RayMask;

	internal List<Hit> hits = new List<Hit>();

	private bool colliderCheckRunning;

	private bool cooldownLogicRunning;

	private Vector3 applyForce;

	private Vector3 applyTorque;

	private void Awake()
	{
		BoxCollider = GetComponent<BoxCollider>();
		if (!BoxCollider)
		{
			SphereCollider = GetComponent<SphereCollider>();
			Collider = SphereCollider;
			ColliderIsBox = false;
		}
		else
		{
			Collider = BoxCollider;
		}
		Collider.isTrigger = true;
		LayerMask = (int)SemiFunc.LayerMaskGetPhysGrabObject() + LayerMask.GetMask("Player") + LayerMask.GetMask("Default") + LayerMask.GetMask("Enemy");
		RayMask = LayerMask.GetMask("Default", "PhysGrabObjectHinge");
	}

	private void OnEnable()
	{
		if (!colliderCheckRunning)
		{
			colliderCheckRunning = true;
			StartCoroutine(ColliderCheck());
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		colliderCheckRunning = false;
		cooldownLogicRunning = false;
		hits.Clear();
	}

	private IEnumerator CooldownLogic()
	{
		while (hits.Count > 0)
		{
			for (int i = 0; i < hits.Count; i++)
			{
				Hit hit = hits[i];
				hit.cooldown -= Time.deltaTime;
				if (hit.cooldown <= 0f)
				{
					hits.RemoveAt(i);
					i--;
				}
			}
			yield return null;
		}
		cooldownLogicRunning = false;
	}

	private bool CanHit(GameObject hitObject, float cooldown, bool raycast, Vector3 hitPosition, HitType hitType)
	{
		foreach (Hit hit2 in hits)
		{
			if (hit2.hitObject == hitObject)
			{
				return false;
			}
		}
		Hit hit = new Hit();
		hit.hitObject = hitObject;
		hit.cooldown = cooldown;
		hit.hitType = hitType;
		hits.Add(hit);
		if (!cooldownLogicRunning)
		{
			StartCoroutine(CooldownLogic());
			cooldownLogicRunning = true;
		}
		if (raycast)
		{
			Vector3 normalized = (hitPosition - Collider.bounds.center).normalized;
			float maxDistance = Vector3.Distance(hitPosition, Collider.bounds.center);
			RaycastHit[] array = Physics.RaycastAll(Collider.bounds.center, normalized, maxDistance, RayMask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				if (raycastHit.collider.gameObject.CompareTag("Wall"))
				{
					PhysGrabObject componentInParent = hitObject.GetComponentInParent<PhysGrabObject>();
					PhysGrabObject componentInParent2 = raycastHit.collider.gameObject.GetComponentInParent<PhysGrabObject>();
					if (!componentInParent || componentInParent != componentInParent2)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private IEnumerator ColliderCheck()
	{
		yield return null;
		while (!LevelGenerator.Instance || !LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (true)
		{
			Vector3 center = Collider.bounds.center;
			Collider[] array;
			if (ColliderIsBox)
			{
				Vector3 halfExtents = BoxCollider.size * 0.5f;
				halfExtents.x *= Mathf.Abs(base.transform.lossyScale.x);
				halfExtents.y *= Mathf.Abs(base.transform.lossyScale.y);
				halfExtents.z *= Mathf.Abs(base.transform.lossyScale.z);
				array = Physics.OverlapBox(center, halfExtents, base.transform.rotation, LayerMask, QueryTriggerInteraction.Collide);
			}
			else
			{
				float radius = base.transform.lossyScale.x * SphereCollider.radius;
				array = Physics.OverlapSphere(center, radius, LayerMask, QueryTriggerInteraction.Collide);
			}
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (playerLogic && playerDamageCooldown > 0f && collider.gameObject.CompareTag("Player"))
					{
						PlayerAvatar playerAvatar = collider.gameObject.GetComponentInParent<PlayerAvatar>();
						if (!playerAvatar)
						{
							PlayerController componentInParent = collider.gameObject.GetComponentInParent<PlayerController>();
							if ((bool)componentInParent)
							{
								playerAvatar = componentInParent.playerAvatarScript;
							}
						}
						if ((bool)playerAvatar)
						{
							PlayerHurt(playerAvatar);
						}
					}
					if (!(enemyDamageCooldown > 0f) && !(physDamageCooldown > 0f) && !(playerDamageCooldown > 0f))
					{
						continue;
					}
					if (collider.gameObject.CompareTag("Phys Grab Object"))
					{
						PhysGrabObject componentInParent2 = collider.gameObject.GetComponentInParent<PhysGrabObject>();
						if (ignoreObjects.Contains(componentInParent2) || !componentInParent2)
						{
							continue;
						}
						bool flag = false;
						PlayerTumble componentInParent3 = collider.gameObject.GetComponentInParent<PlayerTumble>();
						if ((bool)componentInParent3)
						{
							flag = true;
						}
						if (playerLogic && playerDamageCooldown > 0f && flag)
						{
							PlayerHurt(componentInParent3.playerAvatar);
						}
						if (!SemiFunc.IsMasterClientOrSingleplayer())
						{
							continue;
						}
						EnemyRigidbody enemyRigidbody = null;
						if (enemyLogic && !flag)
						{
							enemyRigidbody = collider.gameObject.GetComponentInParent<EnemyRigidbody>();
							EnemyHurtRigidbody(enemyRigidbody, componentInParent2);
						}
						if (!physLogic || (bool)enemyRigidbody || flag || !(physDamageCooldown > 0f) || !CanHit(componentInParent2.gameObject, physDamageCooldown, physRayCast, componentInParent2.centerPoint, HitType.PhysObject))
						{
							continue;
						}
						bool flag2 = false;
						PhysGrabObjectImpactDetector componentInParent4 = collider.gameObject.GetComponentInParent<PhysGrabObjectImpactDetector>();
						if ((bool)componentInParent4)
						{
							if (physHingeDestroy)
							{
								PhysGrabHinge component = componentInParent2.GetComponent<PhysGrabHinge>();
								if ((bool)component)
								{
									component.DestroyHinge();
									flag2 = true;
								}
							}
							else if (physHingeBreak)
							{
								PhysGrabHinge component2 = componentInParent2.GetComponent<PhysGrabHinge>();
								if ((bool)component2 && (bool)component2.joint)
								{
									component2.joint.breakForce = 0f;
									component2.joint.breakTorque = 0f;
									flag2 = true;
								}
							}
							if (!flag2)
							{
								if (physDestroy)
								{
									if (!componentInParent4.destroyDisable)
									{
										PhysGrabHinge component3 = componentInParent2.GetComponent<PhysGrabHinge>();
										if ((bool)component3)
										{
											component3.DestroyHinge();
										}
										else
										{
											componentInParent4.DestroyObject();
										}
									}
									else
									{
										PhysObjectHurt(componentInParent2, BreakImpact.Heavy, 50f, 30f, apply: true, destroyLaunch: true);
									}
									flag2 = true;
								}
								else if ((bool)componentInParent2 && PhysObjectHurt(componentInParent2, physImpact, physHitForce, physHitTorque, apply: true, destroyLaunch: false))
								{
									flag2 = true;
								}
							}
						}
						if (flag2)
						{
							onImpactAny.Invoke();
							onImpactPhysObject.Invoke();
						}
					}
					else
					{
						if (!SemiFunc.IsMasterClientOrSingleplayer() || !enemyLogic)
						{
							continue;
						}
						Enemy componentInParent5 = collider.gameObject.GetComponentInParent<Enemy>();
						if ((bool)componentInParent5 && !componentInParent5.HasRigidbody && CanHit(componentInParent5.gameObject, enemyDamageCooldown, enemyRayCast, componentInParent5.transform.position, HitType.Enemy) && EnemyHurt(componentInParent5))
						{
							onImpactAny.Invoke();
							onImpactEnemyEnemy = componentInParent5;
							onImpactEnemy.Invoke();
						}
						if (!enemyHitTriggers)
						{
							continue;
						}
						EnemyParent componentInParent6 = collider.gameObject.GetComponentInParent<EnemyParent>();
						if ((bool)componentInParent6)
						{
							EnemyRigidbody componentInChildren = componentInParent6.GetComponentInChildren<EnemyRigidbody>();
							if ((bool)componentInChildren)
							{
								EnemyHurtRigidbody(componentInChildren, componentInChildren.physGrabObject);
							}
						}
					}
				}
			}
			yield return new WaitForSeconds(0.05f);
		}
	}

	private void EnemyHurtRigidbody(EnemyRigidbody _enemyRigidbody, PhysGrabObject _physGrabObject)
	{
		if (enemyDamageCooldown > 0f && (bool)_enemyRigidbody && CanHit(_physGrabObject.gameObject, enemyDamageCooldown, enemyRayCast, _physGrabObject.centerPoint, HitType.Enemy) && EnemyHurt(_enemyRigidbody.enemy))
		{
			onImpactAny.Invoke();
			onImpactEnemyEnemy = _enemyRigidbody.enemy;
			onImpactEnemy.Invoke();
		}
	}

	private bool EnemyHurt(Enemy _enemy)
	{
		if (_enemy == enemyHost)
		{
			return false;
		}
		if (!enemyLogic)
		{
			return false;
		}
		bool flag = false;
		if (enemyKill)
		{
			if (_enemy.HasHealth)
			{
				_enemy.Health.Hurt(_enemy.Health.healthCurrent, base.transform.forward);
			}
			else if (_enemy.HasStateDespawn)
			{
				_enemy.EnemyParent.SpawnedTimerSet(0f);
				_enemy.CurrentState = EnemyState.Despawn;
				flag = true;
			}
		}
		if (!flag)
		{
			if (enemyStun && _enemy.HasStateStunned && _enemy.Type <= enemyStunType)
			{
				_enemy.StateStunned.Set(enemyStunTime);
			}
			if (enemyFreezeTime > 0f)
			{
				_enemy.Freeze(enemyFreezeTime);
			}
			if (_enemy.HasRigidbody)
			{
				PhysObjectHurt(_enemy.Rigidbody.physGrabObject, enemyImpact, enemyHitForce, enemyHitTorque, apply: true, destroyLaunch: false);
				if (enemyFreezeTime > 0f)
				{
					_enemy.Rigidbody.FreezeForces(applyForce, applyTorque);
				}
			}
			if (enemyDamage > 0 && _enemy.HasHealth)
			{
				_enemy.Health.Hurt(enemyDamage, applyForce.normalized);
			}
		}
		return true;
	}

	private void PlayerHurt(PlayerAvatar _player)
	{
		if (GameManager.Multiplayer() && !_player.photonView.IsMine)
		{
			return;
		}
		int enemyIndex = SemiFunc.EnemyGetIndex(enemyHost);
		if (playerKill)
		{
			onImpactAny.Invoke();
			onImpactPlayer.Invoke();
			_player.playerHealth.Hurt(_player.playerHealth.health, savingGrace: true, enemyIndex);
		}
		else
		{
			if (!CanHit(_player.gameObject, playerDamageCooldown, playerRayCast, _player.PlayerVisionTarget.VisionTransform.position, HitType.Player))
			{
				return;
			}
			_player.playerHealth.Hurt(playerDamage, savingGrace: true, enemyIndex);
			bool flag = false;
			Vector3 center = Collider.bounds.center;
			Vector3 normalized = (_player.PlayerVisionTarget.VisionTransform.position - center).normalized;
			normalized = SemiFunc.ClampDirection(normalized, base.transform.forward, hitSpread);
			bool flag2 = _player.tumble.isTumbling;
			if (playerTumbleTime > 0f && _player.playerHealth.health > 0)
			{
				_player.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
				_player.tumble.TumbleOverrideTime(playerTumbleTime);
				if (playerTumbleImpactHurtTime > 0f)
				{
					_player.tumble.ImpactHurtSet(playerTumbleImpactHurtTime, playerTumbleImpactHurtDamage);
				}
				flag2 = true;
				flag = true;
			}
			if (flag2 && (playerTumbleForce > 0f || playerTumbleTorque > 0f))
			{
				flag = true;
				if (playerTumbleForce > 0f)
				{
					_player.tumble.TumbleForce(normalized * playerTumbleForce);
				}
				if (playerTumbleTorque > 0f)
				{
					Vector3 rhs = Vector3.zero;
					if (playerTumbleTorqueAxis == TorqueAxis.up)
					{
						rhs = _player.transform.up;
					}
					if (playerTumbleTorqueAxis == TorqueAxis.down)
					{
						rhs = -_player.transform.up;
					}
					if (playerTumbleTorqueAxis == TorqueAxis.right)
					{
						rhs = _player.transform.right;
					}
					if (playerTumbleTorqueAxis == TorqueAxis.left)
					{
						rhs = -_player.transform.right;
					}
					if (playerTumbleTorqueAxis == TorqueAxis.forward)
					{
						rhs = _player.transform.forward;
					}
					if (playerTumbleTorqueAxis == TorqueAxis.back)
					{
						rhs = -_player.transform.forward;
					}
					Vector3 torque = Vector3.Cross((_player.localCameraPosition - center).normalized, rhs) * playerTumbleTorque;
					_player.tumble.TumbleTorque(torque);
				}
			}
			if (!flag2 && playerHitForce > 0f)
			{
				PlayerController.instance.ForceImpulse(normalized * playerHitForce);
			}
			if (playerHitForce > 0f || playerDamage > 0 || flag)
			{
				onImpactPlayerAvatar = _player;
				onImpactAny.Invoke();
				onImpactPlayer.Invoke();
			}
		}
	}

	private bool PhysObjectHurt(PhysGrabObject physGrabObject, BreakImpact impact, float hitForce, float hitTorque, bool apply, bool destroyLaunch)
	{
		bool result = false;
		switch (impact)
		{
		case BreakImpact.Light:
			physGrabObject.lightBreakImpulse = true;
			result = true;
			break;
		case BreakImpact.Medium:
			physGrabObject.mediumBreakImpulse = true;
			result = true;
			break;
		case BreakImpact.Heavy:
			physGrabObject.heavyBreakImpulse = true;
			result = true;
			break;
		}
		if ((bool)enemyHost && impact != BreakImpact.None && physGrabObject.playerGrabbing.Count <= 0 && !physGrabObject.impactDetector.isEnemy)
		{
			physGrabObject.impactDetector.enemyInteractionTimer = 2f;
		}
		if (hitForce > 0f)
		{
			if (hitForce >= 5f && physGrabObject.playerGrabbing.Count > 0)
			{
				foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
				{
					if (!SemiFunc.IsMultiplayer())
					{
						item.ReleaseObjectRPC(physGrabEnded: true, 2f);
						continue;
					}
					item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 1f);
				}
			}
			Vector3 center = Collider.bounds.center;
			Vector3 normalized = (physGrabObject.centerPoint - center).normalized;
			normalized = SemiFunc.ClampDirection(normalized, base.transform.forward, hitSpread);
			applyForce = normalized * hitForce;
			Vector3 normalized2 = (physGrabObject.centerPoint - center).normalized;
			Vector3 rhs = -physGrabObject.transform.up;
			applyTorque = Vector3.Cross(normalized2, rhs) * hitTorque;
			if (apply)
			{
				if (destroyLaunch && !physGrabObject.rb.isKinematic)
				{
					physGrabObject.rb.velocity = Vector3.zero;
					physGrabObject.rb.angularVelocity = Vector3.zero;
					physGrabObject.impactDetector.destroyDisableLaunches++;
					physGrabObject.impactDetector.destroyDisableLaunchesTimer = 10f;
					Vector3 vector = Random.insideUnitSphere.normalized * 4f;
					if (physGrabObject.impactDetector.destroyDisableLaunches >= 3)
					{
						vector *= 20f;
						physGrabObject.impactDetector.destroyDisableLaunches = 0;
					}
					vector.y = 0f;
					applyForce = (Vector3.up * 20f + vector) * physGrabObject.rb.mass;
					applyTorque = Random.insideUnitSphere.normalized * 0.25f * physGrabObject.rb.mass;
				}
				physGrabObject.rb.AddForce(applyForce, ForceMode.Impulse);
				physGrabObject.rb.AddTorque(applyTorque, ForceMode.Impulse);
				result = true;
			}
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		SphereCollider component2 = GetComponent<SphereCollider>();
		if ((bool)component2 && (base.transform.localScale.z != base.transform.localScale.x || base.transform.localScale.z != base.transform.localScale.y))
		{
			Debug.LogError("Sphere Collider must be uniform scale");
		}
		Gizmos.color = new Color(1f, 0f, 0.39f, 6f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		if ((bool)component)
		{
			Gizmos.DrawWireCube(component.center, component.size);
		}
		if ((bool)component2)
		{
			Gizmos.DrawWireSphere(component2.center, component2.radius);
		}
		Gizmos.color = new Color(1f, 0f, 0.39f, 0.2f);
		if ((bool)component)
		{
			Gizmos.DrawCube(component.center, component.size);
		}
		if ((bool)component2)
		{
			Gizmos.DrawSphere(component2.center, component2.radius);
		}
		Gizmos.color = Color.white;
		Gizmos.matrix = Matrix4x4.identity;
		Vector3 vector = Vector3.zero;
		if ((bool)component)
		{
			vector = component.bounds.center;
		}
		if ((bool)component2)
		{
			vector = component2.bounds.center;
		}
		Vector3 vector2 = vector + base.transform.forward * 0.5f;
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector2 + Vector3.LerpUnclamped(-base.transform.forward, -base.transform.right, 0.5f) * 0.25f);
		Gizmos.DrawLine(vector2, vector2 + Vector3.LerpUnclamped(-base.transform.forward, base.transform.right, 0.5f) * 0.25f);
		if (hitSpread < 180f)
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
			Vector3 vector3 = (Quaternion.AngleAxis(hitSpread, base.transform.right) * base.transform.forward).normalized * 1.5f;
			Vector3 vector4 = (Quaternion.AngleAxis(0f - hitSpread, base.transform.right) * base.transform.forward).normalized * 1.5f;
			Vector3 vector5 = (Quaternion.AngleAxis(hitSpread, base.transform.up) * base.transform.forward).normalized * 1.5f;
			Vector3 vector6 = (Quaternion.AngleAxis(0f - hitSpread, base.transform.up) * base.transform.forward).normalized * 1.5f;
			Gizmos.DrawRay(vector, vector3);
			Gizmos.DrawRay(vector, vector4);
			Gizmos.DrawRay(vector, vector5);
			Gizmos.DrawRay(vector, vector6);
			Gizmos.DrawLineStrip(new Vector3[4]
			{
				vector + vector3,
				vector + vector5,
				vector + vector4,
				vector + vector6
			}, looped: true);
		}
		else if (hitSpread > 180f)
		{
			Debug.LogError("Hit Spread cannot be greater than 180 degrees");
		}
	}
}
