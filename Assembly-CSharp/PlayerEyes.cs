using UnityEngine;

public class PlayerEyes : MonoBehaviour
{
	public bool debugDraw;

	private PlayerAvatarVisuals playerAvatarVisuals;

	private PlayerAvatar playerAvatar;

	private PlayerAvatarRightArm playerAvatarRightArm;

	public Transform menuAvatarPointer;

	public Transform eyeLeft;

	public Transform eyeRight;

	public Transform pupilLeft;

	public Transform pupilRight;

	[Space]
	public Transform targetIdle;

	public Transform targetLead;

	[Space]
	public Transform lookAt;

	public SpringQuaternion springQuaternionLeft;

	public SpringQuaternion springQuaternionRight;

	private Transform otherPhysGrabPoint;

	private float eyeLeadTimer;

	private float eyeSideAmount;

	private float eyeUpAmount;

	internal bool lookAtActive;

	private PlayerAvatar currentTalker;

	private float currentTalkerTime;

	private float currentTalkerTimer;

	private bool overrideActive;

	private float overrideTimer;

	private Vector3 overridePosition;

	private GameObject overrideObject;

	private bool overrideSoftActive;

	private float overrideSoftTimer;

	private Vector3 overrideSoftPosition;

	private GameObject overrideSoftObject;

	private float deltaTime;

	internal float pupilLeftSizeMultiplier = 1f;

	internal float pupilRightSizeMultiplier = 1f;

	internal float pupilSizeMultiplier = 1f;

	private void Start()
	{
		playerAvatarVisuals = GetComponent<PlayerAvatarVisuals>();
		playerAvatar = playerAvatarVisuals.playerAvatar;
		playerAvatarRightArm = GetComponent<PlayerAvatarRightArm>();
		if (!playerAvatarVisuals.isMenuAvatar && (!GameManager.Multiplayer() || ((bool)playerAvatar && playerAvatar.photonView.IsMine)))
		{
			base.enabled = false;
		}
	}

	private void MenuLookAt()
	{
		if (playerAvatarVisuals.isMenuAvatar)
		{
			Override(menuAvatarPointer.position, 0.1f, menuAvatarPointer.gameObject);
		}
	}

	private void LookAtTransform(Transform _otherPhysGrabPoint, bool _softOverride)
	{
		lookAtActive = false;
		if (overrideActive)
		{
			lookAtActive = true;
			lookAt.transform.position = overridePosition;
		}
		else if (playerAvatarRightArm.mapToolController.Active && playerAvatarRightArm.mapToolController.HideLerp <= 0f)
		{
			lookAt.transform.position = playerAvatarRightArm.mapToolController.PlayerLookTarget.position;
		}
		else if (playerAvatarRightArm.physGrabBeam.isActiveAndEnabled && (bool)playerAvatar.physGrabber.grabbedObjectTransform && !playerAvatar.physGrabber.grabbedObjectTransform.GetComponent<PhysGrabCart>())
		{
			lookAtActive = true;
			lookAt.transform.position = playerAvatarRightArm.physGrabBeam.PhysGrabPoint.position;
		}
		else if (playerAvatar.healthGrab.staticGrabObject.playerGrabbing.Count > 0)
		{
			lookAtActive = true;
			lookAt.transform.position = playerAvatarVisuals.transform.position + playerAvatarVisuals.transform.forward * 2f;
		}
		else if (playerAvatar.isTumbling && playerAvatar.tumble.physGrabObject.playerGrabbing.Count > 0)
		{
			lookAtActive = true;
			Vector3 position = playerAvatar.tumble.physGrabObject.playerGrabbing[0].playerAvatar.playerAvatarVisuals.headLookAtTransform.position;
			if (playerAvatar.isLocal)
			{
				position = playerAvatar.localCameraPosition;
			}
			lookAt.transform.position = position;
		}
		else if ((bool)_otherPhysGrabPoint)
		{
			lookAtActive = true;
			lookAt.transform.position = _otherPhysGrabPoint.position;
		}
		else if (_softOverride)
		{
			lookAtActive = true;
			lookAt.transform.position = overrideSoftPosition;
		}
		else if ((bool)currentTalker)
		{
			Vector3 position2 = currentTalker.playerAvatarVisuals.headLookAtTransform.position;
			if (currentTalker.isLocal)
			{
				position2 = currentTalker.localCameraPosition;
			}
			lookAtActive = true;
			lookAt.transform.position = position2;
		}
		else if (playerAvatar.isTumbling)
		{
			lookAtActive = true;
			lookAt.transform.position = base.transform.position + playerAvatar.localCameraTransform.forward * 2f;
		}
		else
		{
			lookAt.transform.position = targetIdle.position;
		}
		lookAt.transform.rotation = targetIdle.rotation;
	}

	private void ClosestPhysGrabPoint()
	{
		otherPhysGrabPoint = null;
		float num = 6f;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player != playerAvatar && player.physGrabber.physGrabBeam.gameObject.activeSelf && (bool)player.physGrabber.grabbedObjectTransform && !player.physGrabber.grabbedObjectTransform.GetComponent<PhysGrabCart>())
			{
				float num2 = Vector3.Distance(player.physGrabber.physGrabPoint.position, eyeLeft.position);
				if (num2 < num)
				{
					num = num2;
					otherPhysGrabPoint = player.physGrabber.physGrabPoint;
				}
			}
		}
	}

	private void EyesLead()
	{
		if (playerAvatarVisuals.turnDifference > 0.5f && playerAvatarVisuals.turnDirection != 0f)
		{
			eyeSideAmount = playerAvatarVisuals.turnDifference * (0f - playerAvatarVisuals.turnDirection) * 20f;
			eyeLeadTimer = 0.1f;
		}
		if (playerAvatarVisuals.upDifference > 0.5f && playerAvatarVisuals.upDirection != 0f)
		{
			eyeUpAmount = playerAvatarVisuals.upDifference * (0f - playerAvatarVisuals.upDirection) * 20f;
			eyeLeadTimer = 0.1f;
		}
		if (eyeLeadTimer > 0f)
		{
			eyeLeadTimer -= deltaTime;
			Vector3 localEulerAngles = new Vector3(eyeUpAmount, eyeSideAmount, 0f);
			Quaternion localRotation = targetLead.localRotation;
			targetLead.localEulerAngles = localEulerAngles;
			Quaternion localRotation2 = targetLead.localRotation;
			targetLead.localRotation = localRotation;
			targetLead.localRotation = Quaternion.Lerp(localRotation, localRotation2, deltaTime * 5f);
		}
		else
		{
			eyeSideAmount = 0f;
			eyeUpAmount = 0f;
			targetLead.localRotation = Quaternion.Lerp(targetLead.localRotation, Quaternion.identity, deltaTime * 20f);
		}
	}

	private void Update()
	{
		if (playerAvatarVisuals.isMenuAvatar && !playerAvatar)
		{
			playerAvatar = PlayerAvatar.instance;
		}
		if (!playerAvatarVisuals.isMenuAvatar && (!LevelGenerator.Instance.Generated || playerAvatar.playerHealth.hurtFreeze))
		{
			return;
		}
		deltaTime = playerAvatarVisuals.deltaTime;
		MenuLookAt();
		pupilLeft.localScale = Vector3.one * pupilSizeMultiplier * pupilLeftSizeMultiplier;
		pupilRight.localScale = Vector3.one * pupilSizeMultiplier * pupilRightSizeMultiplier;
		EyesLead();
		ClosestPhysGrabPoint();
		if ((bool)currentTalker && !currentTalker.voiceChat.isTalking)
		{
			currentTalkerTime = 0f;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (!player.isDisabled && (bool)player.voiceChat && player.voiceChat.isTalking && Vector3.Distance(base.transform.position, player.transform.position) < 6f)
			{
				currentTalkerTimer = Random.Range(2f, 4f);
				if (player != playerAvatar && player.voiceChat.isTalkingStartTime > currentTalkerTime)
				{
					currentTalker = player;
					currentTalkerTime = player.voiceChat.isTalkingStartTime;
				}
			}
		}
		if (currentTalkerTimer > 0f)
		{
			currentTalkerTimer -= deltaTime;
			if (currentTalkerTimer <= 0f)
			{
				currentTalker = null;
			}
		}
		bool softOverride = false;
		if (overrideSoftActive)
		{
			softOverride = true;
			if ((bool)overrideSoftObject)
			{
				PlayerAvatar component = overrideSoftObject.GetComponent<PlayerAvatar>();
				if ((bool)component && component == playerAvatar)
				{
					softOverride = false;
				}
				else
				{
					PlayerTumble component2 = overrideSoftObject.GetComponent<PlayerTumble>();
					if ((bool)component2 && component2.playerAvatar == playerAvatar)
					{
						softOverride = false;
					}
				}
			}
		}
		LookAtTransform(otherPhysGrabPoint, softOverride);
		if (overrideSoftTimer > 0f)
		{
			overrideSoftTimer -= deltaTime;
			if (overrideSoftTimer <= 0f)
			{
				overrideSoftActive = false;
			}
		}
		if (overrideTimer > 0f)
		{
			overrideTimer -= deltaTime;
			if (overrideTimer <= 0f)
			{
				overrideActive = false;
			}
		}
		EyeLookAt(ref eyeRight, ref springQuaternionRight, _useSpring: true, 50f, 30f);
		EyeLookAt(ref eyeLeft, ref springQuaternionLeft, _useSpring: true, 50f, 30f);
	}

	public void Override(Vector3 _position, float _time, GameObject _obj)
	{
		if (!overrideActive || !(_obj != overrideObject))
		{
			overrideActive = true;
			overrideObject = _obj;
			overridePosition = _position;
			overrideTimer = _time;
		}
	}

	public void OverrideSoft(Vector3 _position, float _time, GameObject _obj)
	{
		if (!overrideSoftActive || !(_obj != overrideSoftObject))
		{
			overrideSoftActive = true;
			overrideSoftObject = _obj;
			overrideSoftPosition = _position;
			overrideSoftTimer = _time;
		}
	}

	public void EyeLookAt(ref Transform _eyeTransform, ref SpringQuaternion _springQuaternion, bool _useSpring, float _clampX, float _clamY)
	{
		Quaternion localRotation = _eyeTransform.localRotation;
		Vector3 forward = SemiFunc.ClampDirection(lookAt.position - _eyeTransform.transform.position, lookAt.forward, _clampX);
		_eyeTransform.rotation = Quaternion.LookRotation(forward);
		float num = _eyeTransform.localEulerAngles.y;
		if (num > _clamY && num < 180f)
		{
			num = _clamY;
		}
		else if (num < 360f - _clamY && num > 180f)
		{
			num = 360f - _clamY;
		}
		else if (num < 0f - _clamY)
		{
			num = 0f - _clamY;
		}
		_eyeTransform.localEulerAngles = new Vector3(_eyeTransform.localEulerAngles.x, num, _eyeTransform.localEulerAngles.z);
		Quaternion localRotation2 = _eyeTransform.localRotation;
		_eyeTransform.localRotation = localRotation;
		if (_useSpring)
		{
			_eyeTransform.localRotation = SemiFunc.SpringQuaternionGet(_springQuaternion, localRotation2, deltaTime);
		}
		else
		{
			_eyeTransform.localRotation = localRotation2;
		}
	}

	private void OnDrawGizmos()
	{
		if (debugDraw)
		{
			float num = 0.075f;
			Gizmos.color = new Color(1f, 0.93f, 0.99f, 0.6f);
			Gizmos.matrix = lookAt.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one * num);
			Gizmos.color = new Color(0f, 1f, 0.98f, 0.3f);
			Gizmos.DrawCube(Vector3.zero, Vector3.one * num);
		}
	}
}
