using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(EnemyGrounded))]
public class EnemyJump : MonoBehaviour
{
	public Enemy enemy;

	internal bool jumping;

	internal bool jumpingDelay;

	internal bool landDelay;

	internal float jumpCooldown;

	internal float timeSinceJumped;

	[Space]
	public bool warpAgentOnLand;

	[Space]
	public bool surfaceJump = true;

	public float surfaceJumpForceUp = 5f;

	public float surfaceJumpForceSide = 2f;

	private bool surfaceJumpImpulse;

	private Vector3 surfaceJumpDirection;

	private float surfaceJumpDisableTimer;

	[Space]
	public bool stuckJump;

	private float stuckJumpDisableTimer;

	private float cartJumpTimer;

	private float cartJumpCooldown;

	public int stuckJumpCount = 5;

	public float stuckJumpForceUp = 5f;

	public float stuckJumpForceSide = 2f;

	private bool stuckJumpImpulse;

	private Vector3 stuckJumpImpulseDirection;

	[Space]
	public bool gapJump;

	public float gapJumpForceUp = 5f;

	public float gapJumpForceForward = 5f;

	internal bool gapJumpImpulse;

	private float gapJumpOverrideTimer;

	private float gapJumpOverrideUp;

	private float gapJumpOverrideForward;

	public float gapJumpDelay;

	private float gapJumpDelayTimer;

	public float gapLandDelay;

	private float gapLandDelayTimer;

	private bool gapCheckerActive;

	private void Awake()
	{
		enemy.Jump = this;
		enemy.HasJump = true;
		if (gapJump && !gapCheckerActive)
		{
			StartCoroutine(GapChecker());
			gapCheckerActive = true;
		}
	}

	private void Start()
	{
		if (!enemy.HasRigidbody)
		{
			Debug.LogError("EnemyJump: No Rigidbody found on " + enemy.name);
			stuckJump = false;
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		gapCheckerActive = false;
	}

	private void OnEnable()
	{
		if (gapJump && !gapCheckerActive)
		{
			StartCoroutine(GapChecker());
			gapCheckerActive = true;
		}
	}

	public void StuckReset()
	{
		stuckJumpImpulse = false;
	}

	public void SurfaceJumpTrigger(Vector3 _direction)
	{
		if (!jumping)
		{
			surfaceJumpImpulse = true;
			surfaceJumpDirection = _direction;
		}
	}

	public void SurfaceJumpDisable(float _time)
	{
		surfaceJumpImpulse = false;
		surfaceJumpDisableTimer = _time;
	}

	public void StuckTrigger(Vector3 _direction)
	{
		if (!jumping)
		{
			stuckJumpImpulse = true;
			stuckJumpImpulseDirection = _direction;
		}
	}

	public void StuckDisable(float _time)
	{
		stuckJumpDisableTimer = _time;
	}

	private IEnumerator GapChecker()
	{
		gapCheckerActive = true;
		while (true)
		{
			if (enemy.Grounded.grounded && enemy.NavMeshAgent.HasPath())
			{
				int num = 8;
				float num2 = 0.5f;
				float maxDistance = 2f;
				Vector3 forward = enemy.Rigidbody.transform.forward;
				forward.y = 0f;
				Vector3 origin = enemy.Rigidbody.physGrabObject.centerPoint + forward * num2;
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					if (Physics.Raycast(origin, Vector3.down * 0.25f, maxDistance, SemiFunc.LayerMaskGetVisionObstruct()))
					{
						if (flag)
						{
							gapJumpImpulse = true;
						}
					}
					else if (i < 2)
					{
						flag = true;
					}
					origin += forward * num2;
				}
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	private void FixedUpdate()
	{
		if (!jumping)
		{
			timeSinceJumped += Time.fixedDeltaTime;
		}
		else
		{
			timeSinceJumped = 0f;
		}
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		bool flag = false;
		if (enemy.Rigidbody.grabbed || enemy.IsStunned() || enemy.Rigidbody.teleportedTimer > 0f)
		{
			stuckJumpImpulse = false;
			gapJumpImpulse = false;
			return;
		}
		float num = gapJumpForceUp;
		float num2 = gapJumpForceForward;
		if (gapJumpOverrideTimer > 0f)
		{
			num = gapJumpOverrideUp;
			num2 = gapJumpOverrideForward;
			gapJumpOverrideTimer -= Time.fixedDeltaTime;
		}
		if (gapJumpImpulse && !jumping && jumpCooldown <= 0f)
		{
			if (gapJumpDelayTimer > 0f)
			{
				JumpingDelaySet(_jumpingDelay: true);
				enemy.NavMeshAgent.Stop(0.1f);
				enemy.Rigidbody.OverrideFollowPosition(0.1f, 0f);
				enemy.Rigidbody.OverrideColliderMaterialStunned(0.1f);
				gapJumpDelayTimer -= Time.fixedDeltaTime;
			}
			else
			{
				enemy.Rigidbody.DisableFollowPosition(0.5f, 10f);
				Vector3 force = enemy.Rigidbody.transform.forward * num2;
				force.y = 0f;
				force += Vector3.up * num;
				enemy.Rigidbody.JumpImpulse();
				enemy.Rigidbody.rb.AddForce(force, ForceMode.Impulse);
				enemy.NavMeshAgent.OverrideAgent(10f, 999f, 0.5f);
				gapJumpImpulse = false;
				stuckJumpImpulse = false;
				flag = true;
			}
		}
		else
		{
			gapJumpDelayTimer = gapJumpDelay;
		}
		if (enemy.TeleportedTimer > 0f)
		{
			StuckDisable(0.5f);
		}
		if (stuckJumpDisableTimer > 0f)
		{
			stuckJumpDisableTimer -= Time.fixedDeltaTime;
			stuckJumpImpulse = false;
		}
		else if (stuckJump)
		{
			if (cartJumpTimer > 0f && enemy.Rigidbody.touchingCartTimer > 0f)
			{
				if (cartJumpCooldown > 0f)
				{
					cartJumpCooldown -= Time.fixedDeltaTime;
				}
				else
				{
					stuckJumpImpulse = true;
					cartJumpCooldown = 2f;
				}
			}
			if (enemy.StuckCount >= stuckJumpCount)
			{
				stuckJumpImpulse = true;
				enemy.StuckCount = 0;
			}
			if (!flag && stuckJumpImpulse && enemy.Grounded.grounded && !jumping && jumpCooldown <= 0f)
			{
				if (stuckJumpImpulseDirection == Vector3.zero)
				{
					stuckJumpImpulseDirection = enemy.transform.position - enemy.Rigidbody.transform.position;
				}
				Vector3 force2 = stuckJumpImpulseDirection.normalized * stuckJumpForceSide;
				force2.y = 0f;
				force2 += Vector3.up * stuckJumpForceUp;
				stuckJumpImpulseDirection = Vector3.zero;
				enemy.Rigidbody.JumpImpulse();
				enemy.Rigidbody.rb.AddForce(force2, ForceMode.Impulse);
				stuckJumpImpulse = false;
				flag = true;
			}
		}
		if (cartJumpTimer > 0f)
		{
			cartJumpTimer -= Time.fixedDeltaTime;
		}
		if (surfaceJump)
		{
			if (surfaceJumpDisableTimer > 0f)
			{
				surfaceJumpDisableTimer -= Time.fixedDeltaTime;
			}
			else if (!flag && surfaceJumpImpulse && enemy.Grounded.grounded && !jumping && jumpCooldown <= 0f)
			{
				enemy.Rigidbody.DisableFollowPosition(0.2f, 20f);
				enemy.NavMeshAgent.Stop(0.3f);
				Vector3 vector = surfaceJumpDirection * surfaceJumpForceSide;
				vector.y = 0f;
				enemy.Rigidbody.JumpImpulse();
				enemy.Rigidbody.rb.AddForce(vector + Vector3.up * surfaceJumpForceUp, ForceMode.Impulse);
				surfaceJumpImpulse = false;
				flag = true;
			}
		}
		if (!jumping)
		{
			if (flag)
			{
				JumpingDelaySet(_jumpingDelay: false);
				JumpingSet(_jumping: true);
				LandDelaySet(_landDelay: false);
				enemy.Grounded.GroundedDisable(0.1f);
			}
		}
		else if (enemy.Grounded.grounded)
		{
			if (warpAgentOnLand && !enemy.NavMeshAgent.IsDisabled())
			{
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			}
			JumpingDelaySet(_jumpingDelay: false);
			JumpingSet(_jumping: false);
			if (gapLandDelay > 0f)
			{
				LandDelaySet(_landDelay: true);
				gapLandDelayTimer = gapLandDelay;
			}
			jumpCooldown = 0.25f;
		}
		if (jumpCooldown > 0f)
		{
			jumpCooldown -= Time.fixedDeltaTime;
			jumpCooldown = Mathf.Max(jumpCooldown, 0f);
			enemy.StuckCount = 0;
			surfaceJumpImpulse = false;
			stuckJumpImpulse = false;
			gapJumpImpulse = false;
		}
		if (gapLandDelayTimer > 0f)
		{
			enemy.NavMeshAgent.Stop(0.1f);
			enemy.Rigidbody.OverrideFollowPosition(0.1f, 0f);
			enemy.Rigidbody.OverrideColliderMaterialStunned(0.1f);
			gapLandDelayTimer -= Time.fixedDeltaTime;
		}
	}

	public void JumpingSet(bool _jumping)
	{
		if (_jumping != jumping)
		{
			if (_jumping)
			{
				enemy.Grounded.grounded = false;
			}
			jumping = _jumping;
			if (GameManager.Multiplayer() && PhotonNetwork.IsMasterClient)
			{
				enemy.Rigidbody.photonView.RPC("JumpingSetRPC", RpcTarget.Others, jumping);
			}
		}
	}

	public void JumpingDelaySet(bool _jumpingDelay)
	{
		if (jumpingDelay != _jumpingDelay)
		{
			jumpingDelay = _jumpingDelay;
			if (SemiFunc.IsMasterClient())
			{
				enemy.Rigidbody.photonView.RPC("JumpingDelaySetRPC", RpcTarget.Others, jumpingDelay);
			}
		}
	}

	public void LandDelaySet(bool _landDelay)
	{
		if (landDelay != _landDelay)
		{
			landDelay = _landDelay;
			if (SemiFunc.IsMasterClient())
			{
				enemy.Rigidbody.photonView.RPC("LandDelaySetRPC", RpcTarget.Others, landDelay);
			}
		}
	}

	public void CartJump(float _time)
	{
		cartJumpTimer = _time;
	}

	public void GapJumpOverride(float _time, float _up, float _forward)
	{
		gapJumpOverrideTimer = _time;
		gapJumpOverrideUp = _up;
		gapJumpOverrideForward = _forward;
	}

	[PunRPC]
	private void JumpingSetRPC(bool _jumping)
	{
		jumping = _jumping;
	}

	[PunRPC]
	private void JumpingDelaySetRPC(bool _jumpingDelay)
	{
		jumpingDelay = _jumpingDelay;
	}

	[PunRPC]
	private void LandDelaySetRPC(bool _landDelay)
	{
		landDelay = _landDelay;
	}
}
