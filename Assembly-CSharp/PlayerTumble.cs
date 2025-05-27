using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerTumble : MonoBehaviour
{
	internal bool setup;

	public PlayerAvatar playerAvatar;

	public Transform followPosition;

	public ParticleSystem impactParticle;

	public Sound impactSound;

	[Space]
	public Collider[] colliders;

	public HurtCollider hurtCollider;

	[Space]
	public float customGravity = 10f;

	private float customGravityOverrideTimer;

	internal Rigidbody rb;

	internal PhysGrabObject physGrabObject;

	internal PhotonView photonView;

	internal bool isTumbling;

	private bool isTumblingPrevious = true;

	internal float tumbleSetTimer;

	internal float notMovingTimer;

	private Vector3 notMovingPositionLast;

	private Vector3 tumbleForce;

	private Vector3 tumbleTorque;

	private float tumbleForceTimer;

	private float tumbleOverrideTimer;

	internal bool tumbleOverride;

	private bool tumbleOverridePrevious;

	private float lookAtLerp;

	[Space]
	public Sound tumbleMoveSound;

	public Sound tumbleLaunchSound;

	private float tumbleMoveSoundTimer;

	private float tumbleMoveSoundSpeed;

	internal int tumbleLaunch;

	private float overrideEnemyHurtTimer;

	private float impactHurtTimer;

	private int impactHurtDamage;

	private float hurtColliderPauseTimer;

	private float breakFreeCooldown;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		if (!SemiFunc.RunIsLobbyMenu())
		{
			StartCoroutine(Setup());
		}
	}

	private IEnumerator Setup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("SetupRPC", RpcTarget.OthersBuffered, playerAvatar.playerName);
			}
			SetupDone();
		}
	}

	private void SetupDone()
	{
		Collider[] array = colliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		playerAvatar.SoundSetup(tumbleLaunchSound);
		base.transform.parent = playerAvatar.transform.parent;
		setup = true;
		string key = SemiFunc.PlayerGetSteamID(playerAvatar);
		if (StatsManager.instance.playerUpgradeLaunch.ContainsKey(key))
		{
			tumbleLaunch = StatsManager.instance.playerUpgradeLaunch[SemiFunc.PlayerGetSteamID(playerAvatar)];
		}
	}

	[PunRPC]
	public void SetupRPC(string _playerName)
	{
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.playerName == _playerName)
			{
				playerAvatar = player;
				playerAvatar.tumble = this;
				break;
			}
		}
		SetupDone();
	}

	private void Update()
	{
		if (SemiFunc.RunIsLobbyMenu() || !physGrabObject.spawned)
		{
			return;
		}
		if (isTumbling)
		{
			rb.isKinematic = false;
		}
		else
		{
			rb.isKinematic = true;
		}
		if (!isTumbling && (bool)playerAvatar)
		{
			Vector3 position = playerAvatar.transform.position + Vector3.up * 0.3f;
			Quaternion rotation = playerAvatar.transform.rotation;
			rb.MovePosition(position);
			rb.MoveRotation(rotation);
		}
		if (tumbleSetTimer > 0f)
		{
			tumbleSetTimer -= Time.deltaTime;
		}
		if (tumbleMoveSoundTimer > 0f)
		{
			tumbleMoveSoundTimer -= Time.deltaTime;
			tumbleMoveSound.PlayLoop(playing: true, 1f, 1f, tumbleMoveSoundSpeed);
		}
		else
		{
			tumbleMoveSound.PlayLoop(playing: false, 1f, 1f, tumbleMoveSoundSpeed);
		}
		if (isTumbling && playerAvatar.isLocal)
		{
			CameraZoom.Instance.OverrideZoomSet(55f, 0.1f, 1f, 1f, base.gameObject, 150);
			PostProcessing.Instance.VignetteOverride(Color.black, 0.6f, 0.2f, 2f, 2f, 0.1f, base.gameObject);
		}
		bool flag = false;
		if (isTumbling)
		{
			Vector3 rbVelocity = physGrabObject.rbVelocity;
			if (rbVelocity.magnitude > 4f && !physGrabObject.impactDetector.inCart)
			{
				flag = true;
				hurtCollider.transform.LookAt(hurtCollider.transform.position + rbVelocity);
				if (physGrabObject.playerGrabbing.Count == 0 && overrideEnemyHurtTimer <= 0f)
				{
					hurtCollider.enemyLogic = true;
				}
				else
				{
					hurtCollider.enemyLogic = false;
				}
				if (playerAvatar.isLocal)
				{
					hurtCollider.playerLogic = false;
				}
			}
		}
		if (hurtColliderPauseTimer > 0f)
		{
			flag = false;
			hurtColliderPauseTimer -= Time.deltaTime;
		}
		if (flag)
		{
			if (!hurtCollider.gameObject.activeSelf)
			{
				hurtCollider.gameObject.SetActive(value: true);
			}
		}
		else if (hurtCollider.gameObject.activeSelf)
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
		if (overrideEnemyHurtTimer > 0f)
		{
			overrideEnemyHurtTimer -= Time.deltaTime;
		}
		if (isTumbling)
		{
			if ((Vector3.Distance(notMovingPositionLast, base.transform.position) <= 0.5f || physGrabObject.impactDetector.inCart) && physGrabObject.playerGrabbing.Count <= 0)
			{
				notMovingTimer += Time.deltaTime;
			}
			else
			{
				notMovingTimer = 0f;
				notMovingPositionLast = base.transform.position;
			}
		}
		else
		{
			notMovingTimer = 0f;
			notMovingPositionLast = base.transform.position;
		}
		if (breakFreeCooldown <= 0f)
		{
			if (physGrabObject.playerGrabbing.Count > 0 && playerAvatar.isLocal && SemiFunc.InputDown(InputKey.Jump))
			{
				breakFreeCooldown = 0.5f;
				TumbleForce(playerAvatar.localCameraTransform.forward * 15f);
				TumbleTorque(base.transform.right * 10f);
				BreakFree(playerAvatar.localCameraTransform.forward);
			}
		}
		else
		{
			breakFreeCooldown -= Time.deltaTime;
		}
		if (impactHurtTimer > 0f)
		{
			impactHurtTimer -= Time.deltaTime;
		}
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (physGrabObject.playerGrabbing.Count > 0)
		{
			TumbleOverrideTime(1f);
		}
		if (tumbleOverrideTimer > 0f)
		{
			tumbleOverrideTimer -= Time.deltaTime;
			tumbleOverride = true;
		}
		else
		{
			tumbleOverride = false;
		}
		if (tumbleOverride != tumbleOverridePrevious)
		{
			if (tumbleOverride)
			{
				TumbleOverride(_active: true);
			}
			else
			{
				TumbleOverride(_active: false);
			}
			tumbleOverridePrevious = tumbleOverride;
		}
		if (isTumbling && playerAvatar.isDisabled)
		{
			TumbleRequest(_isTumbling: false, _playerInput: false);
		}
		if (isTumbling == isTumblingPrevious)
		{
			return;
		}
		if (isTumbling)
		{
			SetPosition();
			Vector3 rbVelocityRaw = playerAvatar.rbVelocityRaw;
			rb.AddForce(rbVelocityRaw, ForceMode.VelocityChange);
			Vector3 vector = Vector3.Cross(Vector3.up, rbVelocityRaw);
			if (vector.magnitude <= 0f)
			{
				vector = Random.insideUnitSphere.normalized * 1f;
			}
			rb.AddTorque(vector * 2f, ForceMode.VelocityChange);
		}
		isTumblingPrevious = isTumbling;
	}

	private void FixedUpdate()
	{
		if (!isTumbling || (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient))
		{
			return;
		}
		if (isTumbling && playerAvatar.playerHealth.hurtFreeze && playerAvatar.deadSet)
		{
			physGrabObject.FreezeForces(0.1f, Vector3.zero, Vector3.zero);
			return;
		}
		if (customGravityOverrideTimer > 0f)
		{
			customGravityOverrideTimer -= Time.fixedDeltaTime;
		}
		if (rb.useGravity && physGrabObject.playerGrabbing.Count <= 0 && customGravityOverrideTimer <= 0f)
		{
			rb.AddForce(-Vector3.up * customGravity, ForceMode.Force);
		}
		if (tumbleForceTimer > 0f)
		{
			tumbleForceTimer -= Time.fixedDeltaTime;
		}
		if (tumbleForceTimer <= 0f && !playerAvatar.playerHealth.hurtFreeze)
		{
			if (tumbleForce.magnitude > 0f)
			{
				rb.AddForce(tumbleForce, ForceMode.Impulse);
				tumbleForce = Vector3.zero;
			}
			if (tumbleTorque.magnitude > 0f)
			{
				rb.AddTorque(tumbleTorque, ForceMode.Impulse);
				tumbleTorque = Vector3.zero;
			}
		}
		if (notMovingTimer > 2f)
		{
			lookAtLerp += 0.5f * Time.fixedDeltaTime;
			lookAtLerp = Mathf.Clamp01(lookAtLerp);
			Vector3 vector = SemiFunc.PhysFollowRotation(base.transform, playerAvatar.localCameraRotation, rb, 5f);
			vector = Vector3.Lerp(Vector3.zero, vector, 3f * Time.fixedDeltaTime);
			vector = Vector3.Lerp(Vector3.zero, vector, lookAtLerp);
			rb.AddTorque(vector, ForceMode.Impulse);
		}
		else
		{
			lookAtLerp = 0f;
		}
	}

	public void DisableCustomGravity(float _time)
	{
		customGravityOverrideTimer = _time;
	}

	private void SetPosition()
	{
		rb.isKinematic = false;
		tumbleForceTimer = 0.1f;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}

	public void OverrideEnemyHurt(float _time)
	{
		overrideEnemyHurtTimer = _time;
	}

	public void HitEnemy()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (playerAvatar.isLocal)
			{
				playerAvatar.playerHealth.Hurt(5, savingGrace: true);
			}
			else
			{
				playerAvatar.playerHealth.HurtOther(5, base.transform.position, savingGrace: true);
			}
		}
	}

	public void TumbleImpact()
	{
		if (playerAvatar.isLocal)
		{
			PlayerController.instance.CollisionController.StopFallLoop();
		}
		if (!(hurtColliderPauseTimer > 0f) && (!SemiFunc.IsMultiplayer() || hurtCollider.onImpactPlayerAvatar.photonView.IsMine))
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("TumbleImpactRPC", RpcTarget.All, hurtCollider.onImpactPlayerAvatar.photonView.ViewID);
			}
			else
			{
				TumbleImpactRPC(hurtCollider.onImpactPlayerAvatar.photonView.ViewID);
			}
		}
	}

	[PunRPC]
	public void TumbleImpactRPC(int _playerID)
	{
		float time = 0.15f;
		hurtColliderPauseTimer = 0.5f;
		Vector3 vector = Vector3.zero;
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == _playerID)
			{
				item.playerHealth.HurtFreezeOverride(time);
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					vector = (item.transform.position - base.transform.position).normalized;
					item.tumble.physGrabObject.FreezeForces(time, vector * 5f, Vector3.zero);
				}
				break;
			}
		}
		impactParticle.gameObject.SetActive(value: true);
		impactParticle.transform.position = Vector3.Lerp(base.transform.position, base.transform.position + vector, 0.5f);
		impactSound.Play(impactParticle.transform.position);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 5f, 15f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 5f, 15f, base.transform.position, 0.5f);
		playerAvatar.playerHealth.HurtFreezeOverride(time);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.FreezeForces(time, vector * -5f, Vector3.zero);
		}
	}

	public void TumbleOverride(bool _active)
	{
		if (!GameManager.Multiplayer())
		{
			TumbleOverrideRPC(_active);
			return;
		}
		photonView.RPC("TumbleOverrideRPC", RpcTarget.All, _active);
	}

	[PunRPC]
	public void TumbleOverrideRPC(bool _active)
	{
		tumbleOverride = _active;
	}

	public void TumbleOverrideTime(float _time)
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			TumbleOverrideTimeRPC(_time);
			return;
		}
		photonView.RPC("TumbleOverrideTimeRPC", RpcTarget.MasterClient, _time);
	}

	[PunRPC]
	public void TumbleOverrideTimeRPC(float _time)
	{
		tumbleOverrideTimer = _time;
	}

	public void TumbleForce(Vector3 _force)
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			TumbleForceRPC(_force);
			return;
		}
		photonView.RPC("TumbleForceRPC", RpcTarget.MasterClient, _force);
	}

	[PunRPC]
	public void TumbleForceRPC(Vector3 _force)
	{
		tumbleForce += _force;
	}

	public void TumbleTorque(Vector3 _torque)
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			TumbleTorqueRPC(_torque);
			return;
		}
		photonView.RPC("TumbleTorqueRPC", RpcTarget.MasterClient, _torque);
	}

	[PunRPC]
	public void TumbleTorqueRPC(Vector3 _torque)
	{
		tumbleTorque += _torque;
	}

	public void TumbleRequest(bool _isTumbling, bool _playerInput)
	{
		if ((!PlayerController.instance.DebugNoTumble || _playerInput) && !SemiFunc.MenuLevel() && isTumbling != _isTumbling)
		{
			if (!GameManager.Multiplayer())
			{
				TumbleRequestRPC(_isTumbling, _playerInput);
				return;
			}
			photonView.RPC("TumbleRequestRPC", RpcTarget.MasterClient, _isTumbling, _playerInput);
		}
	}

	[PunRPC]
	public void TumbleRequestRPC(bool _isTumbling, bool _playerInput)
	{
		if (!SemiFunc.MenuLevel() && isTumbling != _isTumbling)
		{
			TumbleSet(_isTumbling, _playerInput);
		}
	}

	public void TumbleSet(bool _isTumbling, bool _playerInput)
	{
		isTumbling = _isTumbling;
		SetPosition();
		if (isTumbling)
		{
			rb.isKinematic = false;
			if (tumbleLaunch > 0 && _playerInput)
			{
				Vector3 vector = playerAvatar.localCameraTransform.forward * (3f * (float)tumbleLaunch);
				tumbleForce += vector;
			}
		}
		else
		{
			rb.isKinematic = true;
			tumbleForce = Vector3.zero;
		}
		if (!GameManager.Multiplayer())
		{
			TumbleSetRPC(isTumbling, _playerInput);
			return;
		}
		photonView.RPC("TumbleSetRPC", RpcTarget.All, isTumbling, _playerInput);
	}

	[PunRPC]
	public void TumbleSetRPC(bool _isTumbling, bool _playerInput)
	{
		if (playerAvatar.isLocal && _isTumbling && !_playerInput)
		{
			ChatManager.instance.TumbleInterruption();
		}
		isTumbling = _isTumbling;
		playerAvatar.isTumbling = isTumbling;
		playerAvatar.EnemyVisionFreezeTimerSet(0.5f);
		Vector3 position = playerAvatar.transform.position + Vector3.up * 0.3f;
		Quaternion rotation = playerAvatar.transform.rotation;
		if (!rb.isKinematic)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		if (SemiFunc.IsMultiplayer())
		{
			physGrabObject.photonTransformView.Teleport(position, rotation);
		}
		else
		{
			physGrabObject.rb.position = position;
			physGrabObject.rb.rotation = rotation;
		}
		if (playerAvatar.isLocal)
		{
			playerAvatar.physGrabber.ReleaseObject();
			PlayerController.instance.tumbleInputDisableTimer = 1f;
			GameDirector.instance.CameraImpact.Shake(1f, 0.1f);
			GameDirector.instance.CameraShake.Shake(2f, 0.5f);
			CameraPosition.instance.TumbleSet();
		}
		if (isTumbling)
		{
			if (tumbleLaunch > 0 && _playerInput)
			{
				tumbleLaunchSound.Play(base.transform.position);
				playerAvatar.playerAvatarVisuals.PowerupJumpEffect();
			}
			playerAvatar.TumbleStart();
			tumbleSetTimer = 0.1f;
			if (playerAvatar.isLocal)
			{
				PlayerController.instance.col.enabled = false;
			}
			else
			{
				playerAvatar.playerAvatarCollision.Collider.enabled = false;
			}
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}
		else
		{
			playerAvatar.TumbleStop();
			if (playerAvatar.isLocal)
			{
				PlayerController.instance.col.enabled = true;
			}
			else
			{
				playerAvatar.playerAvatarCollision.Collider.enabled = true;
			}
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
	}

	public void BreakImpact()
	{
		if ((!SemiFunc.IsMultiplayer() || ((bool)playerAvatar && playerAvatar.isLocal)) && impactHurtTimer > 0f)
		{
			PlayerController.instance.CollisionController.ResetFalling();
			playerAvatar.playerHealth.Hurt(impactHurtDamage, savingGrace: true);
			impactHurtTimer = 0f;
		}
	}

	public void ImpactHurtSet(float _time, int _damage)
	{
		if (!GameManager.Multiplayer())
		{
			ImpactHurtSetRPC(_time, _damage);
			return;
		}
		photonView.RPC("ImpactHurtSetRPC", RpcTarget.All, _time, _damage);
	}

	[PunRPC]
	public void ImpactHurtSetRPC(float _time, int _damage)
	{
		if (impactHurtTimer <= 0f || (impactHurtTimer <= _time && _damage == impactHurtDamage) || _damage > impactHurtDamage)
		{
			impactHurtTimer = _time;
			impactHurtDamage = _damage;
		}
	}

	private void BreakFree(Vector3 _direction)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("BreakFreeRPC", RpcTarget.All, _direction);
		}
	}

	[PunRPC]
	private void BreakFreeRPC(Vector3 _direction)
	{
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 2f, 5f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(2f, 2f, 5f, base.transform.position, 0.25f);
		playerAvatar.TumbleBreakFree();
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if (item.playerAvatar.isLocal && Vector3.Dot((item.playerAvatar.PlayerVisionTarget.VisionTransform.position - base.transform.position).normalized, _direction) > 0.5f)
			{
				item.OverridePullDistanceIncrement(-1f);
			}
		}
	}

	public void TumbleMoveSoundSet(bool _active, float _speed)
	{
		_speed = 1f - _speed;
		_speed = 1f + _speed * 0.25f;
		tumbleMoveSoundSpeed = _speed;
		tumbleMoveSoundTimer = 0.1f;
	}

	private void OnDrawGizmos()
	{
		if (isTumbling)
		{
			float num = 0.1f;
			Gizmos.color = new Color(1f, 0.93f, 0.99f, 0.8f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one * num);
			Gizmos.color = new Color(0.28f, 1f, 0f, 0.5f);
			Gizmos.DrawCube(Vector3.zero, Vector3.one * num);
		}
	}
}
