using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBangDirector : MonoBehaviour, IPunObservable
{
	public enum State
	{
		Idle = 0,
		Leave = 1,
		ChangeDestination = 2,
		Investigate = 3,
		AttackSet = 4,
		AttackPlayer = 5,
		AttackCart = 6
	}

	public static EnemyBangDirector instance;

	public bool debugDraw;

	public bool debugOneOnly;

	public bool debugShortIdle;

	public bool debugLongIdle;

	public bool debugNoFuseProgress;

	[Space]
	public List<EnemyBang> units = new List<EnemyBang>();

	internal List<Vector3> destinations = new List<Vector3>();

	[Space]
	public State currentState = State.ChangeDestination;

	private bool stateImpulse = true;

	private float stateTimer;

	internal bool setup;

	internal PlayerAvatar playerTarget;

	internal bool playerTargetCrawling;

	internal Vector3 attackPosition;

	internal Vector3 attackVisionPosition;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			if (!Application.isEditor || (SemiFunc.IsMultiplayer() && !GameManager.instance.localTest))
			{
				debugDraw = false;
				debugOneOnly = false;
				debugShortIdle = false;
				debugLongIdle = false;
				debugNoFuseProgress = false;
			}
			base.transform.parent = LevelGenerator.Instance.EnemyParent.transform;
			StartCoroutine(Setup());
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private IEnumerator Setup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		int num = -1;
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			EnemyBang component = item.Enemy.GetComponent<EnemyBang>();
			if (!component)
			{
				continue;
			}
			if (debugOneOnly && units.Count > 0)
			{
				Object.Destroy(component.enemy.EnemyParent.gameObject);
				continue;
			}
			units.Add(component);
			destinations.Add(Vector3.zero);
			component.directorIndex = units.IndexOf(component);
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (num == -1)
				{
					num = Random.Range(0, component.headObjects.Length);
				}
				if (SemiFunc.IsMultiplayer())
				{
					component.photonView.RPC("SetHeadRPC", RpcTarget.All, num);
				}
				else
				{
					component.SetHeadRPC(num);
				}
				num++;
				if (num >= component.headObjects.Length)
				{
					num = 0;
				}
				float num2 = Random.Range(0.8f, 1.25f);
				if (SemiFunc.IsMultiplayer())
				{
					component.photonView.RPC("SetVoicePitchRPC", RpcTarget.All, num2);
				}
				else
				{
					component.SetVoicePitchRPC(num2);
				}
			}
		}
		foreach (EnemyBang unit in units)
		{
			EnemyBangFuse[] componentsInChildren = unit.enemy.EnemyParent.GetComponentsInChildren<EnemyBangFuse>(includeInactive: true);
			foreach (EnemyBangFuse obj in componentsInChildren)
			{
				obj.controller = unit;
				obj.particleParent.parent = unit.particleParent;
				obj.setup = true;
			}
		}
		setup = true;
	}

	private void Update()
	{
		if (!setup)
		{
			return;
		}
		if (debugDraw)
		{
			Debug.DrawRay(base.transform.position, Vector3.up * 2f, Color.green);
			foreach (EnemyBang unit in units)
			{
				Debug.DrawRay(destinations[units.IndexOf(unit)], Vector3.up * 2f, Color.yellow);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			switch (currentState)
			{
			case State.Idle:
				StateIdle();
				break;
			case State.ChangeDestination:
				StateChangeDestination();
				break;
			case State.Investigate:
				StateInvestigate();
				break;
			case State.AttackSet:
				StateAttackSet();
				break;
			case State.AttackPlayer:
				StateAttackPlayer();
				break;
			case State.AttackCart:
				StateAttackCart();
				break;
			case State.Leave:
				StateLeave();
				break;
			}
		}
	}

	private void StateIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(20f, 30f);
			if (debugShortIdle)
			{
				stateTimer *= 0.5f;
			}
			if (debugLongIdle)
			{
				stateTimer *= 2f;
			}
		}
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.ChangeDestination);
			}
			LeaveCheck();
		}
	}

	private void StateChangeDestination()
	{
		if (stateImpulse)
		{
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 10f, 25f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint)
			{
				flag = SetPosition(levelPoint.transform.position);
			}
			if (flag)
			{
				stateImpulse = false;
				UpdateState(State.Idle);
			}
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			UpdateState(State.Idle);
		}
	}

	private void StateAttackSet()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			UpdateState(State.AttackPlayer);
		}
	}

	private void StateAttackPlayer()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 3f;
		}
		PauseSpawnedTimers();
		if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			playerTargetCrawling = playerTarget.isCrawling;
			if (stateTimer > 0.5f)
			{
				attackPosition = playerTarget.transform.position;
				attackVisionPosition = playerTarget.PlayerVisionTarget.VisionTransform.position;
				if (!playerTargetCrawling)
				{
					attackVisionPosition += Vector3.up * 0.25f;
				}
			}
		}
		else
		{
			stateTimer = 0f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetPosition(attackPosition);
			UpdateState(State.Idle);
		}
	}

	private void StateAttackCart()
	{
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			if ((bool)levelPoint)
			{
				flag = SetPosition(levelPoint.transform.position);
			}
			if (flag)
			{
				stateImpulse = false;
				UpdateState(State.Idle);
			}
		}
	}

	private void UpdateState(State _state)
	{
		currentState = _state;
		stateImpulse = true;
		stateTimer = 0f;
	}

	private bool SetPosition(Vector3 _initialPosition)
	{
		if (NavMesh.SamplePosition(_initialPosition, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")) && !SemiFunc.EnemyPhysObjectSphereCheck(hit.position, 1f))
		{
			base.transform.position = hit.position;
			base.transform.rotation = Quaternion.identity;
			float num = 360f / (float)units.Count;
			foreach (EnemyBang unit in units)
			{
				float num2 = 0f;
				Vector3 value = base.transform.position;
				Vector3 vector = base.transform.position;
				for (; num2 < 2f; num2 += 0.1f)
				{
					value = vector;
					vector = hit.position + base.transform.forward * num2;
					if (!NavMesh.SamplePosition(vector, out var _, 5f, -1) || !Physics.Raycast(vector, Vector3.down, 5f, LayerMask.GetMask("Default")))
					{
						break;
					}
					Vector3 normalized = (vector + Vector3.up * 0.5f - (hit.position + Vector3.up * 0.5f)).normalized;
					if (Physics.Raycast(vector + Vector3.up * 0.5f, normalized, normalized.magnitude, LayerMask.GetMask("Default", "PhysGrabObjectHinge")) || (num2 > 0.5f && Random.Range(0, 100) < 15))
					{
						break;
					}
				}
				destinations[units.IndexOf(unit)] = value;
				base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y + num, 0f);
			}
			return true;
		}
		return false;
	}

	private void LeaveCheck()
	{
		bool flag = false;
		foreach (EnemyBang unit in units)
		{
			if (SemiFunc.EnemyForceLeave(unit.enemy))
			{
				flag = true;
			}
		}
		if (flag)
		{
			UpdateState(State.Leave);
		}
	}

	public void OnSpawn()
	{
		foreach (EnemyBang unit in units)
		{
			unit.enemy.EnemyParent.DespawnedTimerSet(unit.enemy.EnemyParent.DespawnedTimer - 30f);
		}
	}

	public void Investigate(Vector3 _position)
	{
		if (currentState != State.Investigate)
		{
			SetPosition(_position);
			UpdateState(State.Investigate);
		}
	}

	public void SetTarget(PlayerAvatar _player)
	{
		if (currentState != State.AttackSet && currentState != State.AttackPlayer && currentState != State.AttackCart)
		{
			playerTarget = _player;
			UpdateState(State.AttackSet);
		}
		else if (currentState == State.AttackPlayer && playerTarget == _player)
		{
			stateTimer = 2f;
		}
	}

	public void SeeTarget()
	{
		if (currentState == State.AttackPlayer)
		{
			stateTimer = 1f;
		}
	}

	public void TriggerNearby(Vector3 _position)
	{
		foreach (EnemyBang unit in units)
		{
			if (Vector3.Distance(unit.transform.position, _position) < 2f)
			{
				unit.OnVision();
			}
		}
	}

	private void PauseSpawnedTimers()
	{
		foreach (EnemyBang unit in units)
		{
			unit.enemy.EnemyParent.SpawnedTimerPause(0.1f);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(attackVisionPosition);
		}
		else
		{
			attackVisionPosition = (Vector3)stream.ReceiveNext();
		}
	}
}
