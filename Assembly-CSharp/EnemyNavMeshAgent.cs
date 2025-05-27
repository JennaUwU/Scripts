using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavMeshAgent : MonoBehaviour
{
	internal NavMeshAgent Agent;

	internal Vector3 AgentVelocity;

	public bool updateRotation;

	private float StopTimer;

	private float DisableTimer;

	internal float DefaultSpeed;

	internal float DefaultAcceleration;

	private float OverrideTimer;

	private float SetPathTimer;

	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
		if (!updateRotation)
		{
			Agent.updateRotation = false;
		}
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			Agent.enabled = true;
		}
		else
		{
			Agent.enabled = false;
		}
		DefaultSpeed = Agent.speed;
		DefaultAcceleration = Agent.acceleration;
	}

	private void Update()
	{
		AgentVelocity = Agent.velocity;
		if (SetPathTimer > 0f)
		{
			SetPathTimer -= Time.deltaTime;
		}
		if (DisableTimer > 0f)
		{
			Agent.enabled = false;
			DisableTimer -= Time.deltaTime;
			return;
		}
		if (!Agent.enabled)
		{
			Agent.enabled = true;
		}
		if (StopTimer > 0f)
		{
			Agent.isStopped = true;
			StopTimer -= Time.deltaTime;
		}
		else if (Agent.enabled && Agent.isStopped)
		{
			Agent.isStopped = false;
		}
		if (OverrideTimer > 0f)
		{
			OverrideTimer -= Time.deltaTime;
			if (OverrideTimer <= 0f)
			{
				Agent.speed = DefaultSpeed;
				Agent.acceleration = DefaultAcceleration;
			}
		}
	}

	public void OverrideAgent(float speed, float acceleration, float time)
	{
		Agent.speed = speed;
		Agent.acceleration = acceleration;
		OverrideTimer = time;
	}

	public void UpdateAgent(float speed, float acceleration)
	{
		Agent.speed = speed;
		Agent.acceleration = acceleration;
	}

	public void AgentMove(Vector3 position)
	{
		Vector3 velocity = Agent.velocity;
		Vector3 destination = Agent.destination;
		if (OnNavmesh(position))
		{
			Warp(position);
			SetDestination(destination);
			Agent.velocity = velocity;
		}
	}

	private bool OnNavmesh(Vector3 position)
	{
		NavMeshHit hit;
		return NavMesh.SamplePosition(position, out hit, 5f, -1);
	}

	public void Warp(Vector3 position)
	{
		if (Vector3.Distance(base.transform.position, position) < 1f)
		{
			return;
		}
		if (DisableTimer > 0f)
		{
			Agent.enabled = true;
		}
		if (OnNavmesh(position))
		{
			Agent.Warp(position);
			if (DisableTimer > 0f)
			{
				Agent.enabled = false;
			}
		}
	}

	public void ResetPath()
	{
		if (Agent.enabled && HasPath())
		{
			Agent.ResetPath();
		}
	}

	public bool CanReach(Vector3 _target, float _range)
	{
		if (!Agent.enabled)
		{
			return true;
		}
		if (!Agent.hasPath)
		{
			return true;
		}
		if (Vector3.Distance(GetPoint(), _target) > _range)
		{
			return false;
		}
		return true;
	}

	public void SetDestination(Vector3 position)
	{
		if (Agent.enabled)
		{
			if (!Agent.hasPath)
			{
				SetPathTimer = 0.1f;
			}
			Agent.SetDestination(position);
		}
	}

	public void Stop(float time)
	{
		if (Agent.enabled)
		{
			StopTimer = time;
			if (StopTimer == 0f)
			{
				Agent.isStopped = false;
			}
			else
			{
				Agent.isStopped = true;
			}
		}
	}

	public bool IsStopped()
	{
		if (StopTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public void Disable(float time)
	{
		Agent.enabled = false;
		DisableTimer = time;
	}

	public void Enable()
	{
		if (DisableTimer > 0f)
		{
			Agent.enabled = true;
			DisableTimer = 0f;
		}
	}

	public bool IsDisabled()
	{
		if (DisableTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public Vector3 GetPoint()
	{
		if (Agent.hasPath)
		{
			return Agent.path.corners[Agent.path.corners.Length - 1];
		}
		return new Vector3(-1000f, 1000f, 1000f);
	}

	public Vector3 GetDestination()
	{
		if (Agent.hasPath)
		{
			return Agent.destination;
		}
		return base.transform.position;
	}

	public bool HasPath()
	{
		if (SetPathTimer > 0f || Agent.hasPath)
		{
			return true;
		}
		return false;
	}

	public NavMeshPath CalculatePath(Vector3 position)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (!Agent.enabled)
		{
			return navMeshPath;
		}
		Agent.CalculatePath(position, navMeshPath);
		return navMeshPath;
	}
}
