using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PhysGrabber : MonoBehaviour, IPunObservable
{
	private enum ColorState
	{
		Orange = 0,
		Green = 1,
		Purple = 2
	}

	private Camera playerCamera;

	[HideInInspector]
	public float grabRange = 4f;

	[HideInInspector]
	public float grabReleaseDistance = 8f;

	public static PhysGrabber instance;

	[Space]
	[HideInInspector]
	public float minDistanceFromPlayer = 1f;

	[HideInInspector]
	public float maxDistanceFromPlayer = 2.5f;

	[Space]
	public PhysGrabBeam physGrabBeamComponent;

	public GameObject physGrabBeam;

	public Transform physGrabPoint;

	public Transform physGrabPointPuller;

	public Transform physGrabPointPlane;

	private GameObject physGrabPointVisual1;

	private GameObject physGrabPointVisual2;

	internal Vector3 grabbedcObjectPrevCamRelForward;

	internal Vector3 grabbedObjectPrevCamRelUp;

	internal PhysGrabObject grabbedPhysGrabObject;

	internal int grabbedPhysGrabObjectColliderID;

	internal Collider grabbedPhysGrabObjectCollider;

	internal StaticGrabObject grabbedStaticGrabObject;

	internal Rigidbody grabbedObject;

	[HideInInspector]
	public Transform grabbedObjectTransform;

	[HideInInspector]
	public float physGrabPointPullerDampen = 80f;

	[HideInInspector]
	public float springConstant = 0.9f;

	[HideInInspector]
	public float dampingConstant = 0.5f;

	[HideInInspector]
	public float forceConstant = 4f;

	[HideInInspector]
	public float forceMax = 4f;

	private bool physGrabBeamActive;

	[HideInInspector]
	public PhotonView photonView;

	[HideInInspector]
	public bool isLocal;

	[HideInInspector]
	public bool grabbed;

	internal float grabDisableTimer;

	[HideInInspector]
	public Vector3 physGrabPointPosition;

	[HideInInspector]
	public Vector3 physGrabPointPullerPosition;

	[HideInInspector]
	public PlayerAvatar playerAvatar;

	[HideInInspector]
	public Vector3 localGrabPosition;

	[HideInInspector]
	public Vector3 cameraRelativeGrabbedForward;

	[HideInInspector]
	public Vector3 cameraRelativeGrabbedUp;

	[HideInInspector]
	public Vector3 cameraRelativeGrabbedRight;

	private Transform physGrabPointVisualRotate;

	[HideInInspector]
	public Transform physGrabPointVisualGrid;

	[HideInInspector]
	public GameObject physGrabPointVisualGridObject;

	private List<GameObject> physGrabPointVisualGridObjects = new List<GameObject>();

	private int prevColorState = -1;

	[HideInInspector]
	public int colorState;

	private float colorStateOverrideTimer;

	[Space]
	public LayerMask maskLayers;

	internal bool healing;

	internal ItemAttributes currentlyLookingAtItemAttributes;

	internal PhysGrabObject currentlyLookingAtPhysGrabObject;

	internal StaticGrabObject currentlyLookingAtStaticGrabObject;

	[Space]
	public Material physGrabBeamMaterial;

	public Material physGrabBeamMaterialBatteryCharge;

	[HideInInspector]
	public bool physGrabForcesDisabled;

	[HideInInspector]
	public float initialPressTimer;

	private bool overrideGrab;

	private bool overrideGrabRelease;

	private PhysGrabObject overrideGrabTarget;

	private float physGrabBeamAlpha = 1f;

	private float physGrabBeamAlphaChangeTo = 1f;

	private float physGramBeamAlphaTimer;

	private float physGrabBeamAlphaChangeProgress;

	private float physGrabBeamAlphaOriginal;

	private float overrideGrabDistance;

	private float overrideGrabDistanceTimer;

	private float overrideDisableRotationControlsTimer;

	private bool overrideDisableRotationControls;

	private LayerMask mask;

	private float grabCheckTimer;

	internal float pullerDistance;

	[Space]
	public Transform grabberAudioTransform;

	public Sound startSound;

	public Sound loopSound;

	public Sound stopSound;

	private float physRotatingTimer;

	internal Quaternion physRotation;

	private Quaternion physRotationBase;

	[HideInInspector]
	public Vector3 mouseTurningVelocity;

	[HideInInspector]
	public float grabStrength = 1f;

	[HideInInspector]
	public float throwStrength;

	internal bool debugStickyGrabber;

	[HideInInspector]
	public float stopRotationTimer;

	[HideInInspector]
	public Quaternion nextPhysRotation;

	[HideInInspector]
	public bool isRotating;

	private float isRotatingTimer;

	internal bool isPushing;

	internal bool isPulling;

	private float isPushingTimer;

	private float isPullingTimer;

	private float prevPullerDistance;

	private bool prevGrabbed;

	private bool toggleGrab;

	private float toggleGrabTimer;

	private float overrideGrabPointTimer;

	private Transform overrideGrabPointTransform;

	private void Start()
	{
		StartCoroutine(LateStart());
		physRotation = Quaternion.identity;
		physRotationBase = Quaternion.identity;
		mask = (int)SemiFunc.LayerMaskGetVisionObstruct() - LayerMask.GetMask("Player");
		playerAvatar = GetComponent<PlayerAvatar>();
		photonView = GetComponent<PhotonView>();
		if (GameManager.instance.gameMode == 0 || photonView.IsMine)
		{
			isLocal = true;
			instance = this;
		}
		foreach (Transform item in physGrabPoint)
		{
			if (item.name == "Visual1")
			{
				physGrabPointVisual1 = item.gameObject;
				foreach (Transform item2 in item)
				{
					if (item2.name == "Visual2")
					{
						physGrabPointVisual2 = item2.gameObject;
					}
				}
			}
			if (item.name == "Rotate")
			{
				physGrabPointVisualRotate = item;
				item.GetComponent<PhysGrabPointRotate>().physGrabber = this;
			}
			if (!(item.name == "Grid"))
			{
				continue;
			}
			physGrabPointVisualGrid = item;
			foreach (Transform item3 in item)
			{
				physGrabPointVisualGridObject = item3.gameObject;
				physGrabPointVisualGridObject.SetActive(value: false);
			}
		}
		physGrabPoint.SetParent(null, worldPositionStays: true);
		PhysGrabPointDeactivate();
		physGrabPointPuller.gameObject.SetActive(value: false);
		physGrabBeam.transform.SetParent(null, worldPositionStays: false);
		physGrabBeam.transform.position = Vector3.zero;
		physGrabBeam.transform.rotation = Quaternion.identity;
		physGrabBeam.SetActive(value: false);
		physGrabBeamAlphaOriginal = physGrabBeam.GetComponent<LineRenderer>().material.color.a;
		SoundSetup(startSound);
		SoundSetup(loopSound);
		SoundSetup(stopSound);
		if (isLocal)
		{
			playerCamera = Camera.main;
			PlayerController.instance.physGrabPoint = physGrabPoint;
			physGrabPointPlane.SetParent(null, worldPositionStays: false);
			physGrabPointPlane.position = Vector3.zero;
			physGrabPointPlane.rotation = Quaternion.identity;
			physGrabPointPlane.SetParent(CameraAim.Instance.transform, worldPositionStays: false);
			physGrabPointPlane.localPosition = Vector3.zero;
			physGrabPointPlane.localRotation = Quaternion.identity;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(physGrabBeam);
	}

	public void OverrideGrabDistance(float dist)
	{
		prevPullerDistance = pullerDistance;
		pullerDistance = dist;
		overrideGrabDistance = dist;
		overrideGrabDistanceTimer = 0.1f;
	}

	private void OverrideGrabDistanceTick()
	{
		if (overrideGrabDistanceTimer > 0f)
		{
			overrideGrabDistanceTimer -= Time.deltaTime;
		}
		else if (overrideGrabDistanceTimer != -123f)
		{
			overrideGrabDistance = 0f;
			overrideGrabDistanceTimer = -123f;
		}
	}

	private IEnumerator LateStart()
	{
		while (!playerAvatar)
		{
			yield return new WaitForSeconds(0.2f);
		}
		string _steamID = SemiFunc.PlayerGetSteamID(playerAvatar);
		yield return new WaitForSeconds(0.2f);
		while (!StatsManager.instance.playerUpgradeStrength.ContainsKey(_steamID))
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (!SemiFunc.MenuLevel())
		{
			grabStrength += (float)StatsManager.instance.playerUpgradeStrength[_steamID] * 0.2f;
			throwStrength += (float)StatsManager.instance.playerUpgradeThrow[_steamID] * 0.3f;
			grabRange += (float)StatsManager.instance.playerUpgradeRange[_steamID] * 1f;
		}
	}

	public void SoundSetup(Sound _sound)
	{
		if (!SemiFunc.IsMultiplayer() || photonView.IsMine)
		{
			_sound.SpatialBlend = 0f;
			return;
		}
		_sound.Volume *= 0.5f;
		_sound.VolumeRandom *= 0.5f;
		_sound.SpatialBlend = 1f;
	}

	public void OverrideDisableRotationControls()
	{
		overrideDisableRotationControls = true;
		overrideDisableRotationControlsTimer = 0.1f;
	}

	private void OverrideDisableRotationControlsTick()
	{
		if (overrideDisableRotationControlsTimer > 0f)
		{
			overrideDisableRotationControlsTimer -= Time.fixedDeltaTime;
			if (overrideDisableRotationControlsTimer <= 0f)
			{
				overrideDisableRotationControls = false;
			}
		}
	}

	public void OverrideGrab(PhysGrabObject target)
	{
		overrideGrab = true;
		overrideGrabTarget = target;
	}

	public void OverrideGrabPoint(Transform grabPoint)
	{
		overrideGrabPointTransform = grabPoint;
		overrideGrabPointTimer = 0.1f;
	}

	public void OverrideGrabRelease()
	{
		overrideGrabRelease = true;
		overrideGrab = false;
		overrideGrabTarget = null;
	}

	public void GrabberHeal()
	{
		if (!healing)
		{
			photonView.RPC("HealStart", RpcTarget.All);
		}
	}

	private void ColorStateSetColor(Color mainColor, Color emissionColor)
	{
		Material material = physGrabBeam.GetComponent<LineRenderer>().material;
		Material material2 = physGrabPointVisual1.GetComponent<MeshRenderer>().material;
		Material material3 = physGrabPointVisual2.GetComponent<MeshRenderer>().material;
		Material material4 = physGrabPointVisualRotate.GetComponent<MeshRenderer>().material;
		Light grabberLight = playerAvatar.playerAvatarVisuals.playerAvatarRightArm.grabberLight;
		Material material5 = playerAvatar.playerAvatarVisuals.playerAvatarRightArm.grabberOrbSpheres[0].GetComponent<MeshRenderer>().material;
		Material material6 = playerAvatar.playerAvatarVisuals.playerAvatarRightArm.grabberOrbSpheres[1].GetComponent<MeshRenderer>().material;
		if ((bool)material)
		{
			material.color = mainColor;
		}
		if ((bool)material)
		{
			material.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material2)
		{
			material2.color = mainColor;
		}
		if ((bool)material2)
		{
			material2.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material3)
		{
			material3.color = mainColor;
		}
		if ((bool)material3)
		{
			material3.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material4)
		{
			material4.color = mainColor;
		}
		if ((bool)material4)
		{
			material4.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)grabberLight)
		{
			grabberLight.color = mainColor;
		}
		if ((bool)material5)
		{
			material5.color = mainColor;
		}
		if ((bool)material5)
		{
			material5.SetColor("_EmissionColor", emissionColor);
		}
		if ((bool)material6)
		{
			material6.color = mainColor;
		}
		if ((bool)material6)
		{
			material6.SetColor("_EmissionColor", emissionColor);
		}
	}

	public void OverrideColorToGreen(float time = 0.1f)
	{
		colorState = 1;
		colorStateOverrideTimer = time;
	}

	public void OverrideColorToPurple(float time = 0.1f)
	{
		colorState = 2;
		colorStateOverrideTimer = time;
	}

	private void ColorStates()
	{
		if (prevColorState != colorState)
		{
			prevColorState = colorState;
			Color color = new Color(1f, 0.1856f, 0f, 0.15f);
			Color color2 = new Color(1f, 0.1856f, 0f, 1f);
			if (colorState == 0)
			{
				color = (VideoGreenScreen.instance ? new Color(1f, 0.1856f, 0f, 1f) : new Color(1f, 0.1856f, 0f, 0.15f));
				color2 = new Color(1f, 0.1856f, 0f, 1f);
				ColorStateSetColor(color, color2);
			}
			else if (colorState == 1)
			{
				color = (VideoGreenScreen.instance ? new Color(0f, 1f, 0f, 1f) : new Color(0f, 1f, 0f, 0.15f));
				color2 = new Color(0f, 1f, 0f, 1f);
				ColorStateSetColor(color, color2);
			}
			else if (colorState == 2)
			{
				color = (VideoGreenScreen.instance ? new Color(1f, 0f, 1f, 1f) : new Color(1f, 0f, 1f, 0.15f));
				color2 = new Color(1f, 0f, 1f, 1f);
				ColorStateSetColor(color, color2);
			}
		}
	}

	private void ColorStateTick()
	{
		if (colorStateOverrideTimer > 0f)
		{
			colorStateOverrideTimer -= Time.fixedDeltaTime;
		}
		else
		{
			colorState = 0;
		}
	}

	[PunRPC]
	private void HealStart()
	{
		physGrabBeam.GetComponent<LineRenderer>().material = physGrabBeamMaterialBatteryCharge;
		physGrabPointVisual1.GetComponent<MeshRenderer>().material = physGrabBeamMaterialBatteryCharge;
		physGrabPointVisual2.GetComponent<MeshRenderer>().material = physGrabBeamMaterialBatteryCharge;
		physGrabBeam.GetComponent<PhysGrabBeam>().scrollSpeed = new Vector2(-5f, 0f);
		physGrabBeam.GetComponent<PhysGrabBeam>().lineMaterial = physGrabBeam.GetComponent<LineRenderer>().material;
		healing = true;
	}

	private void ResetBeam()
	{
		if (healing)
		{
			physGrabBeam.GetComponent<LineRenderer>().material = physGrabBeamMaterial;
			physGrabPointVisual1.GetComponent<MeshRenderer>().material = physGrabBeamMaterial;
			physGrabPointVisual2.GetComponent<MeshRenderer>().material = physGrabBeamMaterial;
			physGrabBeam.GetComponent<PhysGrabBeam>().scrollSpeed = physGrabBeam.GetComponent<PhysGrabBeam>().originalScrollSpeed;
			physGrabBeam.GetComponent<PhysGrabBeam>().lineMaterial = physGrabBeam.GetComponent<LineRenderer>().material;
			healing = false;
		}
	}

	public void ChangeBeamAlpha(float alpha)
	{
		if (physGramBeamAlphaTimer == -123f)
		{
			physGrabBeamAlpha = physGrabBeamAlphaOriginal;
		}
		physGrabBeamAlphaChangeTo = alpha;
		physGramBeamAlphaTimer = 0.1f;
		physGrabBeamAlphaChangeProgress = 0f;
	}

	private void TickerBeamAlphaChange()
	{
		if (physGramBeamAlphaTimer > 0f)
		{
			physGrabBeamAlpha = Mathf.Lerp(physGrabBeamAlpha, physGrabBeamAlphaChangeTo, physGrabBeamAlphaChangeProgress);
			if (physGrabBeamAlphaChangeProgress < 1f)
			{
				physGrabBeamAlphaChangeProgress += 4f * Time.deltaTime;
				Material material = physGrabBeam.GetComponent<LineRenderer>().material;
				material.SetColor("_Color", new Color(material.color.r, material.color.g, material.color.b, physGrabBeamAlpha));
				Material material2 = physGrabPointVisual1.GetComponent<MeshRenderer>().material;
				Material material3 = physGrabPointVisual2.GetComponent<MeshRenderer>().material;
				material2.SetColor("_Color", new Color(material2.color.r, material2.color.g, material2.color.b, physGrabBeamAlpha));
				material3.SetColor("_Color", new Color(material3.color.r, material3.color.g, material3.color.b, physGrabBeamAlpha));
			}
		}
		else if (physGramBeamAlphaTimer != -123f)
		{
			physGrabBeamAlphaChangeProgress = 0f;
			Material material4 = physGrabBeam.GetComponent<LineRenderer>().material;
			material4.SetColor("_Color", new Color(material4.color.r, material4.color.g, material4.color.b, physGrabBeamAlphaOriginal));
			Material material5 = physGrabPointVisual1.GetComponent<MeshRenderer>().material;
			Material material6 = physGrabPointVisual2.GetComponent<MeshRenderer>().material;
			material5.SetColor("_Color", new Color(material5.color.r, material5.color.g, material5.color.b, physGrabBeamAlphaOriginal));
			material6.SetColor("_Color", new Color(material6.color.r, material6.color.g, material6.color.b, physGrabBeamAlphaOriginal));
			physGramBeamAlphaTimer = -123f;
		}
		if (physGramBeamAlphaTimer > 0f)
		{
			physGramBeamAlphaTimer -= Time.deltaTime;
		}
	}

	public Quaternion GetRotationInput()
	{
		Quaternion quaternion = Quaternion.AngleAxis(mouseTurningVelocity.y, Vector3.right);
		Quaternion quaternion2 = Quaternion.AngleAxis(0f - mouseTurningVelocity.x, Vector3.up);
		Quaternion quaternion3 = Quaternion.AngleAxis(mouseTurningVelocity.z, Vector3.forward);
		return quaternion2 * quaternion * quaternion3;
	}

	private void ObjectTurning()
	{
		if (!grabbedPhysGrabObject)
		{
			return;
		}
		if (!grabbed)
		{
			mouseTurningVelocity = Vector3.zero;
			physGrabPointVisualGrid.gameObject.SetActive(value: false);
			isRotating = false;
			return;
		}
		if ((bool)physGrabPointVisualGrid && (bool)grabbedPhysGrabObject)
		{
			physGrabPointVisualGrid.position = grabbedPhysGrabObject.midPoint;
		}
		if (mouseTurningVelocity.magnitude > 0.01f)
		{
			mouseTurningVelocity = Vector3.Lerp(mouseTurningVelocity, Vector3.zero, 1f * Time.deltaTime);
		}
		else
		{
			mouseTurningVelocity = Vector3.zero;
		}
		cameraRelativeGrabbedForward = cameraRelativeGrabbedForward.normalized;
		cameraRelativeGrabbedUp = cameraRelativeGrabbedUp.normalized;
		bool flag = false;
		if (isLocal && SemiFunc.InputHold(InputKey.Rotate))
		{
			flag = true;
		}
		if (flag)
		{
			float axis = Input.GetAxis("Mouse X");
			float axis2 = Input.GetAxis("Mouse Y");
			Vector3 vector = new Vector3(axis, axis2, 0f) * 8f * Time.deltaTime;
			mouseTurningVelocity += vector;
			if (isLocal)
			{
				isRotatingTimer = 0.1f;
			}
		}
		if (isRotating)
		{
			physGrabPointVisualGrid.gameObject.SetActive(value: true);
			Transform localCameraTransform = playerAvatar.localCameraTransform;
			if (physRotatingTimer <= 0f)
			{
				physRotatingTimer = 0.25f;
				cameraRelativeGrabbedForward = localCameraTransform.InverseTransformDirection(grabbedObjectTransform.forward);
				cameraRelativeGrabbedUp = localCameraTransform.InverseTransformDirection(grabbedObjectTransform.up);
				physGrabPointVisualGrid.rotation = grabbedObjectTransform.rotation;
			}
			physRotatingTimer = 0.25f;
			float mass = grabbedPhysGrabObject.rb.mass;
			float value = 1f / mass;
			value = Mathf.Clamp(value, 0f, 0.5f);
			if (value != 0f)
			{
				grabbedPhysGrabObject.OverrideAngularDrag(40f * value);
			}
			Quaternion quaternion = Quaternion.AngleAxis(mouseTurningVelocity.y, localCameraTransform.right);
			Quaternion quaternion2 = Quaternion.AngleAxis(0f - mouseTurningVelocity.x, localCameraTransform.up);
			Quaternion quaternion3 = Quaternion.AngleAxis(mouseTurningVelocity.z, localCameraTransform.forward);
			Quaternion quaternion4 = quaternion2 * quaternion * quaternion3;
			float fixedDeltaTime = Time.fixedDeltaTime;
			float num = 10000f * Time.fixedDeltaTime;
			float num2 = Quaternion.Angle(Quaternion.identity, quaternion4);
			if (num2 > num)
			{
				quaternion4 = Quaternion.Slerp(Quaternion.identity, quaternion4, num / num2);
			}
			quaternion4 = Quaternion.Slerp(Quaternion.identity, quaternion4, fixedDeltaTime * 20f);
			physGrabPointVisualGrid.rotation = quaternion4 * physGrabPointVisualGrid.rotation;
			cameraRelativeGrabbedForward = localCameraTransform.InverseTransformDirection(grabbedObjectTransform.forward);
			cameraRelativeGrabbedUp = localCameraTransform.InverseTransformDirection(grabbedObjectTransform.up);
			foreach (PhysGrabber item in grabbedPhysGrabObject.playerGrabbing)
			{
				Transform localCameraTransform2 = item.playerAvatar.localCameraTransform;
				item.cameraRelativeGrabbedForward = localCameraTransform2.InverseTransformDirection(physGrabPointVisualGrid.forward);
				item.cameraRelativeGrabbedUp = localCameraTransform2.InverseTransformDirection(physGrabPointVisualGrid.up);
			}
			physGrabPointVisualGrid.transform.rotation = Quaternion.Slerp(physGrabPointVisualGrid.transform.rotation, grabbedObjectTransform.rotation, Time.deltaTime * 10f);
		}
		else
		{
			physGrabPointVisualGrid.gameObject.SetActive(value: false);
		}
	}

	private void OverrideGrabPointTimer()
	{
		if (overrideGrabPointTimer > 0f)
		{
			overrideGrabPointTimer -= Time.fixedDeltaTime;
		}
		else
		{
			overrideGrabPointTransform = null;
		}
	}

	private void FixedUpdate()
	{
		OverrideGrabPointTimer();
		OverrideDisableRotationControlsTick();
		if (isLocal)
		{
			if ((bool)grabbedPhysGrabObject)
			{
				_ = grabbedPhysGrabObject.isMelee;
			}
			else
				_ = 0;
			if (!overrideDisableRotationControls)
			{
				if (isRotatingTimer > 0f)
				{
					SemiFunc.CameraOverrideStopAim();
					if (!isRotating && (bool)grabbedObjectTransform)
					{
						_ = playerAvatar.localCameraTransform;
						mouseTurningVelocity = Vector3.zero;
					}
					isRotating = true;
				}
				else
				{
					isRotating = false;
				}
			}
		}
		if (stopRotationTimer > 0f)
		{
			stopRotationTimer -= Time.fixedDeltaTime;
		}
		ColorStateTick();
	}

	private void PushingPullingChecker()
	{
		if (overrideGrabDistanceTimer > 0f)
		{
			pullerDistance = overrideGrabDistance;
			prevPullerDistance = pullerDistance;
		}
		if (!grabbed)
		{
			isPushing = false;
			isPulling = false;
			isPushingTimer = 0f;
			isPullingTimer = 0f;
			prevPullerDistance = pullerDistance;
			return;
		}
		if (initialPressTimer > 0f)
		{
			prevPullerDistance = pullerDistance;
			isPushingTimer = 0f;
		}
		if (SemiFunc.InputScrollY() > 0f)
		{
			isPushingTimer = 0.1f;
		}
		if (SemiFunc.InputScrollY() < 0f)
		{
			isPullingTimer = 0.1f;
		}
		if (isPushingTimer > 0f)
		{
			isPushing = true;
			isPushingTimer -= Time.deltaTime;
		}
		else
		{
			isPushing = false;
		}
		if (isPullingTimer > 0f)
		{
			isPulling = true;
			isPullingTimer -= Time.deltaTime;
		}
		else
		{
			isPulling = false;
		}
		prevPullerDistance = pullerDistance;
		if (overrideGrabDistanceTimer > 0f)
		{
			pullerDistance = overrideGrabDistance;
			prevPullerDistance = pullerDistance;
		}
	}

	public void OverridePullDistanceIncrement(float distSpeed)
	{
		physGrabPointPlane.position += playerCamera.transform.forward * distSpeed;
	}

	private void Update()
	{
		if (isRotatingTimer > 0f)
		{
			isRotatingTimer -= Time.deltaTime;
		}
		PushingPullingChecker();
		ColorStates();
		ObjectTurning();
		if ((bool)grabbedObjectTransform && grabbedObjectTransform.name == playerAvatar.healthGrab.name)
		{
			OverrideColorToGreen();
		}
		OverrideGrabDistanceTick();
		TickerBeamAlphaChange();
		if (initialPressTimer > 0f)
		{
			initialPressTimer -= Time.deltaTime;
		}
		if (physRotatingTimer > 0f)
		{
			physRotatingTimer -= Time.deltaTime;
		}
		if (grabbed && (bool)grabbedObjectTransform)
		{
			if (!overrideGrabPointTransform)
			{
				physGrabPoint.position = grabbedObjectTransform.TransformPoint(localGrabPosition);
			}
			else
			{
				physGrabPoint.position = overrideGrabPointTransform.position;
			}
		}
		if (isLocal)
		{
			bool flag = (bool)grabbedPhysGrabObject && grabbedPhysGrabObject.isMelee;
			if (!SemiFunc.InputHold(InputKey.Rotate))
			{
				if (InputManager.instance.KeyPullAndPush() > 0f && Vector3.Distance(physGrabPointPuller.position, playerCamera.transform.position) < grabRange && !flag)
				{
					physGrabPointPlane.position += playerCamera.transform.forward * 0.2f;
				}
				if (InputManager.instance.KeyPullAndPush() < 0f && Vector3.Distance(physGrabPointPuller.position, playerCamera.transform.position) > minDistanceFromPlayer && !flag)
				{
					physGrabPointPlane.position -= playerCamera.transform.forward * 0.2f;
				}
			}
			if (overrideGrabDistanceTimer < 0f)
			{
				pullerDistance = Vector3.Distance(physGrabPointPuller.position, playerCamera.transform.position);
			}
			if (overrideGrabDistance > 0f)
			{
				Transform visionTransform = playerAvatar.PlayerVisionTarget.VisionTransform;
				physGrabPointPlane.position = visionTransform.position + visionTransform.forward * overrideGrabDistance;
			}
			else
			{
				if (pullerDistance < minDistanceFromPlayer)
				{
					physGrabPointPuller.position = playerCamera.transform.position + playerCamera.transform.forward * minDistanceFromPlayer;
				}
				if (pullerDistance > maxDistanceFromPlayer)
				{
					physGrabPointPuller.position = playerCamera.transform.position + playerCamera.transform.forward * maxDistanceFromPlayer;
				}
			}
		}
		else if (overrideGrabDistanceTimer <= 0f)
		{
			pullerDistance = Vector3.Distance(physGrabPointPuller.position, playerAvatar.localCameraPosition);
		}
		grabberAudioTransform.position = physGrabBeamComponent.PhysGrabPointOrigin.position;
		loopSound.PlayLoop(physGrabBeam.gameObject.activeSelf, 10f, 10f);
		if (!isLocal)
		{
			return;
		}
		ShowValue();
		bool flag2 = SemiFunc.InputHold(InputKey.Grab) || toggleGrab;
		if (debugStickyGrabber && !SemiFunc.InputHold(InputKey.Rotate))
		{
			flag2 = true;
		}
		if (InputManager.instance.InputToggleGet(InputKey.Grab))
		{
			if (SemiFunc.InputDown(InputKey.Grab))
			{
				toggleGrab = !toggleGrab;
				if (toggleGrab)
				{
					toggleGrabTimer = 0.1f;
				}
			}
		}
		else
		{
			toggleGrab = false;
		}
		if (toggleGrabTimer > 0f)
		{
			toggleGrabTimer -= Time.deltaTime;
		}
		else if (!grabbed && toggleGrab)
		{
			toggleGrab = false;
		}
		if (overrideGrab && (SemiFunc.InputHold(InputKey.Grab) || toggleGrab))
		{
			overrideGrab = false;
			overrideGrabTarget = null;
		}
		if (overrideGrab)
		{
			flag2 = true;
		}
		if (overrideGrabRelease)
		{
			flag2 = false;
			overrideGrabRelease = false;
		}
		if (PlayerController.instance.InputDisableTimer > 0f)
		{
			flag2 = false;
		}
		bool flag3 = false;
		if (flag2 && !grabbed)
		{
			if (grabDisableTimer <= 0f)
			{
				flag3 = true;
			}
		}
		else if (!flag2 && grabbed)
		{
			ReleaseObject();
		}
		if (LevelGenerator.Instance.Generated && PlayerController.instance.InputDisableTimer <= 0f)
		{
			if (grabCheckTimer <= 0f || flag3)
			{
				grabCheckTimer = 0.02f;
				RayCheck(flag3);
			}
			else
			{
				grabCheckTimer -= Time.deltaTime;
			}
		}
		PhysGrabLogic();
		if (grabDisableTimer > 0f)
		{
			grabDisableTimer -= Time.deltaTime;
		}
	}

	private void PhysGrabLogic()
	{
		grabReleaseDistance = Mathf.Max(grabRange * 2f, 10f);
		if (!grabbed)
		{
			return;
		}
		if (physRotatingTimer > 0f)
		{
			Aim.instance.SetState(Aim.State.Rotate);
		}
		else
		{
			Aim.instance.SetState(Aim.State.Grab);
		}
		if (Vector3.Distance(physGrabPoint.position, playerCamera.transform.position) > grabReleaseDistance)
		{
			ReleaseObject();
			return;
		}
		if ((bool)grabbedPhysGrabObject)
		{
			if (!grabbedPhysGrabObject.enabled || grabbedPhysGrabObject.dead || !grabbedPhysGrabObjectCollider || !grabbedPhysGrabObjectCollider.enabled)
			{
				ReleaseObject();
				return;
			}
		}
		else
		{
			if (!grabbedStaticGrabObject)
			{
				ReleaseObject();
				return;
			}
			if (!grabbedStaticGrabObject.isActiveAndEnabled || grabbedStaticGrabObject.dead)
			{
				ReleaseObject();
				return;
			}
		}
		physGrabPointPullerPosition = physGrabPointPuller.position;
		PhysGrabStarted();
		PhysGrabBeamActivate();
	}

	private void PhysGrabBeamActivate()
	{
		if (GameManager.instance.gameMode == 0)
		{
			if (!physGrabBeamActive)
			{
				physGrabForcesDisabled = false;
				physGrabBeam.SetActive(value: true);
				physGrabBeamComponent.physGrabPointPullerSmoothPosition = physGrabPoint.position;
				physGrabBeamActive = true;
				PhysGrabStartEffects();
			}
		}
		else if (!physGrabBeamActive)
		{
			photonView.RPC("PhysGrabBeamActivateRPC", RpcTarget.All);
			physGrabBeamActive = true;
		}
	}

	public void ShowValue()
	{
		if (!grabbed || !grabbedPhysGrabObject)
		{
			return;
		}
		ValuableObject component = grabbedPhysGrabObject.GetComponent<ValuableObject>();
		if ((bool)component)
		{
			WorldSpaceUIValue.instance.Show(grabbedPhysGrabObject, (int)component.dollarValueCurrent, _cost: false, Vector3.zero);
		}
		else if (SemiFunc.RunIsShop())
		{
			ItemAttributes component2 = grabbedPhysGrabObject.GetComponent<ItemAttributes>();
			if ((bool)component2)
			{
				WorldSpaceUIValue.instance.Show(grabbedPhysGrabObject, component2.value, _cost: true, component2.costOffset);
			}
		}
	}

	private void PhysGrabStartEffects()
	{
		startSound.Play(loopSound.Source.transform.position);
		if (!GameManager.Multiplayer() || photonView.IsMine)
		{
			GameDirector.instance.CameraImpact.Shake(0.5f, 0.1f);
		}
	}

	private void PhysGrabEndEffects()
	{
		stopSound.Play(loopSound.Source.transform.position);
		if (!GameManager.Multiplayer() || photonView.IsMine)
		{
			GameDirector.instance.CameraImpact.Shake(0.5f, 0.1f);
		}
	}

	[PunRPC]
	private void PhysGrabBeamActivateRPC()
	{
		PhysGrabStartEffects();
		initialPressTimer = 0.1f;
		physGrabForcesDisabled = false;
		physGrabBeam.SetActive(value: true);
		physGrabBeamComponent.physGrabPointPullerSmoothPosition = physGrabPoint.position;
		physGrabBeamActive = true;
		physGrabPointVisualRotate.GetComponent<PhysGrabPointRotate>().animationEval = 0f;
		PhysGrabPointActivate();
	}

	private void PhysGrabPointDeactivate()
	{
		physGrabPointVisualGrid.parent = physGrabPoint;
		physGrabPointVisualRotate.localScale = Vector3.zero;
		physGrabPointVisualRotate.GetComponent<PhysGrabPointRotate>().animationEval = 0f;
		GridObjectsRemove();
		physGrabPoint.gameObject.SetActive(value: false);
	}

	private void PhysGrabPointActivate()
	{
		if ((bool)grabbedObjectTransform)
		{
			physGrabPointVisualRotate.localScale = Vector3.zero;
			PhysGrabPointRotate component = physGrabPointVisualRotate.GetComponent<PhysGrabPointRotate>();
			if ((bool)component)
			{
				component.animationEval = 0f;
				component.rotationActiveTimer = 0f;
			}
			physGrabPointVisualGrid.localPosition = Vector3.zero;
			physGrabPointVisualGrid.parent = null;
			physGrabPointVisualGrid.localScale = Vector3.one;
			grabbedPhysGrabObject = grabbedObjectTransform.GetComponent<PhysGrabObject>();
			if ((bool)grabbedPhysGrabObject)
			{
				physGrabPointVisualGrid.localRotation = grabbedPhysGrabObject.rb.rotation;
			}
			if ((bool)grabbedPhysGrabObject)
			{
				GridObjectsInstantiate();
			}
			physGrabPointVisualGrid.gameObject.SetActive(value: false);
			physGrabPoint.gameObject.SetActive(value: true);
		}
	}

	[PunRPC]
	private void PhysGrabBeamDeactivateRPC()
	{
		physGrabForcesDisabled = false;
		ResetBeam();
		physGrabBeam.SetActive(value: false);
		PhysGrabPointDeactivate();
		physGrabBeamActive = false;
		PhysGrabEndEffects();
		physRotation = Quaternion.identity;
	}

	private void PhysGrabBeamDeactivate()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			PhysGrabBeamDeactivateRPC();
		}
		else
		{
			photonView.RPC("PhysGrabBeamDeactivateRPC", RpcTarget.All);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(physGrabPointPullerPosition);
			stream.SendNext(physGrabPointPlane.position);
			stream.SendNext(mouseTurningVelocity);
			stream.SendNext(isRotating);
			stream.SendNext(colorState);
		}
		else
		{
			physGrabPointPullerPosition = (Vector3)stream.ReceiveNext();
			physGrabPointPuller.position = physGrabPointPullerPosition;
			physGrabPointPlane.position = (Vector3)stream.ReceiveNext();
			mouseTurningVelocity = (Vector3)stream.ReceiveNext();
			isRotating = (bool)stream.ReceiveNext();
			colorState = (int)stream.ReceiveNext();
		}
	}

	private void PhysGrabStarted()
	{
		if ((bool)grabbedPhysGrabObject)
		{
			grabbedPhysGrabObject.GrabStarted(this);
		}
		else if ((bool)grabbedStaticGrabObject)
		{
			grabbedStaticGrabObject.GrabStarted(this);
		}
		else
		{
			ReleaseObject();
		}
	}

	private void PhysGrabEnded()
	{
		if ((bool)grabbedPhysGrabObject)
		{
			grabbedPhysGrabObject.GrabEnded(this);
		}
		else if ((bool)grabbedStaticGrabObject)
		{
			grabbedStaticGrabObject.GrabEnded(this);
		}
	}

	private void RayCheck(bool _grab)
	{
		if (playerAvatar.isDisabled || playerAvatar.isTumbling || playerAvatar.deadSet)
		{
			return;
		}
		float maxDistance = 10f;
		if (_grab)
		{
			grabDisableTimer = 0.1f;
		}
		Vector3 direction = playerCamera.transform.forward;
		if (overrideGrab && (bool)overrideGrabTarget)
		{
			direction = (overrideGrabTarget.transform.position - playerCamera.transform.position).normalized;
		}
		if (!_grab)
		{
			RaycastHit[] array = Physics.SphereCastAll(playerCamera.transform.position, 1f, direction, maxDistance, mask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				ValuableObject component = raycastHit.transform.GetComponent<ValuableObject>();
				if (!component)
				{
					continue;
				}
				if (!component.discovered)
				{
					Vector3 direction2 = playerCamera.transform.position - raycastHit.point;
					RaycastHit[] array2 = Physics.SphereCastAll(raycastHit.point, 0.01f, direction2, direction2.magnitude, mask, QueryTriggerInteraction.Collide);
					bool flag = true;
					RaycastHit[] array3 = array2;
					for (int j = 0; j < array3.Length; j++)
					{
						RaycastHit raycastHit2 = array3[j];
						if (!raycastHit2.transform.CompareTag("Player") && raycastHit2.transform != raycastHit.transform)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						component.Discover(ValuableDiscoverGraphic.State.Discover);
					}
				}
				else
				{
					if (!component.discoveredReminder)
					{
						continue;
					}
					Vector3 direction3 = playerCamera.transform.position - raycastHit.point;
					RaycastHit[] array4 = Physics.RaycastAll(raycastHit.point, direction3, direction3.magnitude, mask, QueryTriggerInteraction.Collide);
					bool flag2 = true;
					RaycastHit[] array3 = array4;
					foreach (RaycastHit raycastHit3 in array3)
					{
						if (raycastHit3.collider.transform.CompareTag("Wall"))
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						component.discoveredReminder = false;
						component.Discover(ValuableDiscoverGraphic.State.Reminder);
					}
				}
			}
		}
		if (!Physics.Raycast(playerCamera.transform.position, direction, out var hitInfo, maxDistance, mask, QueryTriggerInteraction.Ignore))
		{
			return;
		}
		bool flag3 = false;
		flag3 = overrideGrab && !overrideGrabTarget;
		flag3 = overrideGrab && (bool)overrideGrabTarget && hitInfo.transform.GetComponentInParent<PhysGrabObject>() == overrideGrabTarget;
		if (!overrideGrab)
		{
			flag3 = true;
		}
		if (!(hitInfo.collider.CompareTag("Phys Grab Object") && flag3) || hitInfo.distance > grabRange)
		{
			return;
		}
		if (_grab)
		{
			grabbedPhysGrabObject = hitInfo.transform.GetComponent<PhysGrabObject>();
			if ((bool)grabbedPhysGrabObject && grabbedPhysGrabObject.grabDisableTimer > 0f)
			{
				return;
			}
			if ((bool)grabbedPhysGrabObject && grabbedPhysGrabObject.rb.IsSleeping())
			{
				grabbedPhysGrabObject.OverrideIndestructible(0.5f);
				grabbedPhysGrabObject.OverrideBreakEffects(0.5f);
			}
			grabbedObjectTransform = hitInfo.transform;
			if ((bool)grabbedPhysGrabObject)
			{
				PhysGrabObjectCollider component2 = hitInfo.collider.GetComponent<PhysGrabObjectCollider>();
				grabbedPhysGrabObjectCollider = hitInfo.collider;
				grabbedPhysGrabObjectColliderID = component2.colliderID;
				grabbedStaticGrabObject = null;
			}
			else
			{
				grabbedPhysGrabObject = null;
				grabbedPhysGrabObjectCollider = null;
				grabbedPhysGrabObjectColliderID = 0;
				grabbedStaticGrabObject = grabbedObjectTransform.GetComponent<StaticGrabObject>();
				if (!grabbedStaticGrabObject)
				{
					StaticGrabObject[] componentsInParent = grabbedObjectTransform.GetComponentsInParent<StaticGrabObject>();
					foreach (StaticGrabObject staticGrabObject in componentsInParent)
					{
						if (staticGrabObject.colliderTransform == hitInfo.collider.transform)
						{
							grabbedStaticGrabObject = staticGrabObject;
						}
					}
				}
				if (!grabbedStaticGrabObject || !grabbedStaticGrabObject.enabled)
				{
					return;
				}
			}
			PhysGrabPointActivate();
			physGrabPointPuller.gameObject.SetActive(value: true);
			grabbedObject = hitInfo.rigidbody;
			Vector3 vector = hitInfo.point;
			if ((bool)grabbedPhysGrabObject && grabbedPhysGrabObject.roomVolumeCheck.currentSize.magnitude < 0.5f)
			{
				vector = hitInfo.collider.bounds.center;
			}
			float num = Vector3.Distance(playerCamera.transform.position, vector);
			Vector3 position = playerCamera.transform.position + playerCamera.transform.forward * num;
			physGrabPointPlane.position = position;
			physGrabPointPuller.position = position;
			if (physRotatingTimer <= 0f)
			{
				cameraRelativeGrabbedForward = Camera.main.transform.InverseTransformDirection(grabbedObjectTransform.forward);
				cameraRelativeGrabbedUp = Camera.main.transform.InverseTransformDirection(grabbedObjectTransform.up);
				cameraRelativeGrabbedRight = Camera.main.transform.InverseTransformDirection(grabbedObjectTransform.right);
			}
			if (GameManager.instance.gameMode == 0)
			{
				physGrabPoint.position = vector;
				if (!grabbedPhysGrabObject || !grabbedPhysGrabObject.forceGrabPoint)
				{
					localGrabPosition = grabbedObjectTransform.InverseTransformPoint(vector);
				}
				else
				{
					vector = grabbedPhysGrabObject.forceGrabPoint.position;
					num = 1f;
					position = playerCamera.transform.position + playerCamera.transform.forward * num - playerCamera.transform.up * 0.3f;
					physGrabPoint.position = vector;
					physGrabPointPlane.position = position;
					physGrabPointPuller.position = position;
					localGrabPosition = grabbedObjectTransform.InverseTransformPoint(vector);
				}
			}
			else if ((bool)grabbedPhysGrabObject)
			{
				if ((bool)grabbedPhysGrabObject.forceGrabPoint)
				{
					vector = grabbedPhysGrabObject.forceGrabPoint.position;
					Quaternion quaternion = Quaternion.Euler(45f, 0f, 0f);
					cameraRelativeGrabbedForward = quaternion * Vector3.forward;
					cameraRelativeGrabbedUp = quaternion * Vector3.up;
					cameraRelativeGrabbedRight = quaternion * Vector3.right;
					num = 1f;
					position = playerCamera.transform.position + playerCamera.transform.forward * num - playerCamera.transform.up * 0.3f;
					if (!overrideGrabPointTransform)
					{
						physGrabPoint.position = vector;
					}
					else
					{
						physGrabPoint.position = overrideGrabPointTransform.position;
					}
					physGrabPointPlane.position = position;
					physGrabPointPuller.position = position;
				}
				grabbedPhysGrabObject.GrabLink(photonView.ViewID, grabbedPhysGrabObjectColliderID, vector, cameraRelativeGrabbedForward, cameraRelativeGrabbedUp);
			}
			else if ((bool)grabbedStaticGrabObject)
			{
				grabbedStaticGrabObject.GrabLink(photonView.ViewID, vector);
			}
			if (isLocal)
			{
				PlayerController.instance.physGrabObject = grabbedObjectTransform.gameObject;
				PlayerController.instance.physGrabActive = true;
			}
			initialPressTimer = 0.1f;
			prevGrabbed = grabbed;
			grabbed = true;
		}
		if (!grabbed)
		{
			bool flag4 = false;
			PhysGrabObject physGrabObject = hitInfo.transform.GetComponent<PhysGrabObject>();
			if (!physGrabObject)
			{
				physGrabObject = hitInfo.transform.GetComponentInParent<PhysGrabObject>();
			}
			if ((bool)physGrabObject)
			{
				currentlyLookingAtPhysGrabObject = physGrabObject;
				flag4 = true;
			}
			StaticGrabObject staticGrabObject2 = hitInfo.transform.GetComponent<StaticGrabObject>();
			if (!staticGrabObject2)
			{
				staticGrabObject2 = hitInfo.transform.GetComponentInParent<StaticGrabObject>();
			}
			if ((bool)staticGrabObject2 && staticGrabObject2.enabled)
			{
				currentlyLookingAtStaticGrabObject = staticGrabObject2;
				flag4 = true;
			}
			ItemAttributes component3 = hitInfo.transform.GetComponent<ItemAttributes>();
			if ((bool)component3)
			{
				currentlyLookingAtItemAttributes = component3;
				component3.ShowInfo();
			}
			if (flag4)
			{
				Aim.instance.SetState(Aim.State.Grabbable);
			}
		}
	}

	public void ReleaseObject(float _disableTimer = 0.1f)
	{
		if (!grabbed)
		{
			return;
		}
		overrideGrab = false;
		overrideGrabTarget = null;
		if ((bool)physGrabPoint)
		{
			PhysGrabEnded();
			physGrabPoint.SetParent(null, worldPositionStays: true);
			grabbedObject = null;
			grabbedObjectTransform = null;
			prevGrabbed = grabbed;
			grabbed = false;
			if (isLocal)
			{
				PlayerController.instance.physGrabObject = null;
				PlayerController.instance.physGrabActive = false;
			}
			if ((bool)physGrabPoint)
			{
				PhysGrabPointDeactivate();
			}
			if ((bool)physGrabPointPuller)
			{
				physGrabPointPuller.gameObject.SetActive(value: false);
			}
			PhysGrabBeamDeactivate();
			grabDisableTimer = 0.1f;
		}
	}

	[PunRPC]
	public void ReleaseObjectRPC(bool physGrabEnded, float _disableTimer = 0.1f)
	{
		if (isLocal)
		{
			if (!physGrabEnded)
			{
				grabbedStaticGrabObject = null;
			}
			ReleaseObject();
			grabDisableTimer = _disableTimer;
		}
	}

	private void GridObjectsInstantiate()
	{
		PhysGrabObject physGrabObject = grabbedPhysGrabObject;
		if (physGrabObject.GetComponent<PhysGrabObjectImpactDetector>().isCart)
		{
			return;
		}
		Quaternion rotation = grabbedPhysGrabObject.rb.rotation;
		grabbedPhysGrabObject.rb.rotation = Quaternion.identity;
		Collider[] componentsInChildren = physGrabObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (!collider.isTrigger && collider.gameObject.activeSelf && !(collider is MeshCollider))
			{
				GameObject gameObject = Object.Instantiate(physGrabPointVisualGridObject);
				gameObject.SetActive(value: true);
				SetGridObjectScale(gameObject.transform, collider);
				Quaternion rotation2 = grabbedObjectTransform.rotation;
				physGrabPointVisualGrid.rotation = Quaternion.identity;
				grabbedObjectTransform.rotation = Quaternion.identity;
				physGrabPointVisualGrid.localRotation = Quaternion.identity;
				Vector3 position = grabbedPhysGrabObject.transform.position;
				_ = grabbedPhysGrabObject.transform.localRotation;
				physGrabPointVisualGrid.position = grabbedPhysGrabObject.transform.TransformPoint(grabbedPhysGrabObject.midPointOffset);
				grabbedPhysGrabObject.transform.position = Vector3.zero;
				gameObject.transform.position = collider.bounds.center;
				gameObject.transform.rotation = collider.transform.rotation;
				gameObject.transform.SetParent(physGrabPointVisualGrid);
				physGrabPointVisualGridObjects.Add(gameObject);
				grabbedObjectTransform.rotation = rotation2;
				grabbedPhysGrabObject.transform.position = position;
			}
		}
		grabbedPhysGrabObject.rb.rotation = rotation;
	}

	private void SetGridObjectScale(Transform _itemEquipCubeTransform, Collider _collider)
	{
		Quaternion rotation = _collider.transform.rotation;
		_collider.transform.rotation = Quaternion.identity;
		if (_collider is BoxCollider boxCollider)
		{
			_itemEquipCubeTransform.localScale = Vector3.Scale(boxCollider.size, _collider.transform.lossyScale);
		}
		else if (_collider is SphereCollider { radius: var radius })
		{
			float num = radius * Mathf.Max(_collider.transform.lossyScale.x, _collider.transform.lossyScale.y, _collider.transform.lossyScale.z) * 2f;
			_itemEquipCubeTransform.localScale = new Vector3(num, num, num);
		}
		else if (_collider is CapsuleCollider capsuleCollider)
		{
			float num2 = capsuleCollider.radius * Mathf.Max(_collider.transform.lossyScale.x, _collider.transform.lossyScale.z) * 2f;
			float y = capsuleCollider.height * _collider.transform.lossyScale.y;
			_itemEquipCubeTransform.localScale = new Vector3(num2, y, num2);
		}
		else
		{
			_itemEquipCubeTransform.localScale = _collider.bounds.size;
		}
		_collider.transform.rotation = rotation;
	}

	private void GridObjectsRemove()
	{
		foreach (GameObject physGrabPointVisualGridObject in physGrabPointVisualGridObjects)
		{
			Object.Destroy(physGrabPointVisualGridObject);
		}
		physGrabPointVisualGridObjects.Clear();
	}
}
