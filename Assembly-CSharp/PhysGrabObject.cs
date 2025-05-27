using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PhysGrabObject : MonoBehaviour, IPunObservable
{
	public bool clientNonKinematic;

	public bool overrideTagsAndLayers = true;

	internal PhotonView photonView;

	internal PhotonTransformView photonTransformView;

	[HideInInspector]
	public Rigidbody rb;

	private bool isMaster;

	internal RoomVolumeCheck roomVolumeCheck;

	internal PhysGrabObjectImpactDetector impactDetector;

	private bool hasImpactDetector;

	internal Vector3 targetPos;

	private float distance;

	internal Quaternion targetRot;

	private float angle;

	internal Vector3 grabDisplacementCurrent;

	[HideInInspector]
	public bool dead;

	[HideInInspector]
	public bool grabbed;

	[HideInInspector]
	public bool grabbedLocal;

	public List<PhysGrabber> playerGrabbing = new List<PhysGrabber>();

	[HideInInspector]
	public bool spawned;

	internal PlayerAvatar lastPlayerGrabbing;

	internal float grabbedTimer;

	[HideInInspector]
	public bool lightBreakImpulse;

	[HideInInspector]
	public bool mediumBreakImpulse;

	[HideInInspector]
	public bool heavyBreakImpulse;

	[HideInInspector]
	public bool lightImpactImpulse;

	[HideInInspector]
	public bool mediumImpactImpulse;

	[HideInInspector]
	public bool heavyImpactImpulse;

	[HideInInspector]
	public float enemyInteractTimer;

	internal float angularDragOriginal;

	internal float dragOriginal;

	internal bool isValuable;

	internal bool isEnemy;

	internal bool isPlayer;

	internal bool isMelee;

	internal bool isNonValuable;

	internal bool isKinematic;

	[HideInInspector]
	public float massOriginal;

	private float lastUpdateTime;

	private List<(Vector3 position, double timestamp)> positionBuffer = new List<(Vector3, double)>();

	private List<(Quaternion rotation, double timestamp)> rotationBuffer = new List<(Quaternion, double)>();

	private float gradualLerp;

	private Vector3 prevTargetPos;

	private Quaternion prevTargetRot;

	internal Vector3 rbVelocity = Vector3.zero;

	internal Vector3 rbAngularVelocity = Vector3.zero;

	internal Vector3 currentPosition;

	internal Quaternion currentRotation;

	private bool hasHinge;

	private PhysGrabHinge hinge;

	private float timerZeroGravity;

	private float timerAlterDrag;

	private float alterDragValue;

	private float timerAlterAngularDrag;

	private float alterAngularDragValue;

	private float timerAlterMass;

	private float alterMassValue;

	private float timerAlterMaterial;

	private float timerAlterDeactivate = -123f;

	private float overrideFragilityTimer;

	internal float overrideDisableBreakEffectsTimer;

	private bool isActive = true;

	private PhysicMaterial alterMaterialPrevious;

	private PhysicMaterial alterMaterialCurrent;

	[HideInInspector]
	public Vector3 midPoint;

	[HideInInspector]
	public Vector3 midPointOffset;

	private Vector3 grabRotation;

	private bool isHidden;

	internal float grabDisableTimer;

	internal bool heldByLocalPlayer;

	private CollisionDetectionMode previousCollisionDetectionMode;

	private Camera mainCamera;

	private float timerAlterIndestructible;

	internal Transform forceGrabPoint;

	private MapCustom mapCustom;

	private bool hasMapCustom;

	private bool isCart;

	private PhysGrabCart physGrabCart;

	[HideInInspector]
	public List<Transform> colliders = new List<Transform>();

	[HideInInspector]
	public Vector3 centerPoint;

	public Vector3 camRelForward;

	public Vector3 camRelUp;

	internal bool frozen;

	private float frozenTimer;

	private Vector3 frozenPosition;

	private Quaternion frozenRotation;

	private Vector3 frozenVelocity;

	private Vector3 frozenAngularVelocity;

	private Vector3 frozenForce;

	private Vector3 frozenTorque;

	private float overrideDragGoDownTimer;

	private float overrideAngularDragGoDownTimer;

	private float overrideMassGoDownTimer;

	internal float impactHappenedTimer;

	internal float impactLightTimer;

	internal float impactMediumTimer;

	internal float impactHeavyTimer;

	internal float breakLightTimer;

	internal float breakMediumTimer;

	internal float breakHeavyTimer;

	internal bool hasNeverBeenGrabbed = true;

	[HideInInspector]
	public Vector3 boundingBox;

	internal Vector3 spawnTorque = Vector3.zero;

	private float smoothRotationDelta;

	private bool rbIsSleepingPrevious;

	private float overrideTorqueStrengthX = 1f;

	private float overrideTorqueStrengthXTimer;

	private float overrideTorqueStrengthY = 1f;

	private float overrideTorqueStrengthYTimer;

	private float overrideTorqueStrengthZ = 1f;

	private float overrideTorqueStrengthZTimer;

	private float overrideTorqueStrength = 1f;

	private float overrideTorqueStrengthTimer;

	private float overrideGrabStrength = 1f;

	private float overrideGrabStrengthTimer;

	private float overrideGrabRelativeVerticalPosition;

	private float overrideGrabRelativeVerticalPositionTimer;

	private float overrideGrabRelativeHorizontalPosition;

	private float overrideGrabRelativeHorizontalPositionTimer;

	private void Awake()
	{
		photonTransformView = GetComponent<PhotonTransformView>();
		if (!photonTransformView)
		{
			Debug.LogError("No Photon Transform View found on " + base.gameObject.name);
		}
		physGrabCart = GetComponent<PhysGrabCart>();
		isCart = physGrabCart;
		forceGrabPoint = base.transform.Find("Force Grab Point");
		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;
		Transform transform = base.transform.Find("Center of Mass");
		if ((bool)transform)
		{
			rb.centerOfMass = transform.localPosition;
		}
		rb.interpolation = RigidbodyInterpolation.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		angularDragOriginal = rb.angularDrag;
		dragOriginal = rb.drag;
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		if ((bool)impactDetector)
		{
			hasImpactDetector = true;
		}
		if ((bool)GetComponent<ValuableObject>())
		{
			isValuable = true;
		}
		mapCustom = GetComponent<MapCustom>();
		if ((bool)mapCustom)
		{
			hasMapCustom = true;
		}
		Transform[] componentsInParent = GetComponentsInParent<Transform>();
		foreach (Transform transform2 in componentsInParent)
		{
			if (transform2.name.Contains("debug") || transform2.name.Contains("Debug"))
			{
				spawned = true;
			}
		}
	}

	public void TurnXYZ(Quaternion turnX, Quaternion turnY, Quaternion turnZ)
	{
		Vector3 vector = turnY * Vector3.forward;
		Vector3 vector2 = turnY * Vector3.up;
		vector = turnZ * vector;
		vector2 = turnZ * vector2;
		foreach (PhysGrabber item in playerGrabbing)
		{
			item.cameraRelativeGrabbedForward = turnX * vector;
			item.cameraRelativeGrabbedUp = turnX * vector2;
		}
	}

	public void TorqueToTarget(PhysGrabber player, Quaternion target, float strength, float dampen)
	{
		if (!rb.isKinematic)
		{
			Vector3 zero = Vector3.zero;
			Vector3 forward = base.transform.forward;
			Vector3 up = base.transform.up;
			Vector3 vector = target * Vector3.forward;
			Vector3 vector2 = (player.cameraRelativeGrabbedUp = target * Vector3.up);
			player.cameraRelativeGrabbedForward = vector;
			Vector3 vector3 = Vector3.Cross(forward, vector);
			if (vector3.sqrMagnitude > 1E-08f)
			{
				float value = Vector3.Angle(forward, vector);
				zero += vector3.normalized * Mathf.Clamp(value, 0f, 60f);
			}
			Vector3 vector4 = Vector3.Cross(up, vector2);
			if (vector4.sqrMagnitude > 1E-08f)
			{
				float value2 = Vector3.Angle(up, vector2);
				zero += vector4.normalized * Mathf.Clamp(value2, 0f, 60f);
			}
			zero *= rb.mass;
			zero = Vector3.ClampMagnitude(zero, 60f).normalized;
			if (rb.mass < 1f)
			{
				zero *= 0.75f;
			}
			float num = Vector3.Angle(base.transform.forward, vector) / 180f;
			float num2 = Vector3.Angle(base.transform.up, vector2) / 180f;
			float num3 = Mathf.Clamp01(rb.mass);
			float num4 = (num + num2) * dampen * num3;
			zero *= strength;
			zero *= num4;
			Vector3 torque = zero * num3;
			rb.AddTorque(torque, ForceMode.Impulse);
		}
	}

	private void Start()
	{
		if (!SemiFunc.IsMultiplayer() && (bool)photonTransformView)
		{
			photonTransformView.enabled = false;
			photonTransformView = null;
		}
		mainCamera = Camera.main;
		isEnemy = GetComponent<EnemyRigidbody>();
		isMelee = GetComponent<ItemMelee>();
		if (!isEnemy)
		{
			isNonValuable = GetComponent<NotValuableObject>();
		}
		Quaternion rotation = base.transform.rotation;
		base.transform.rotation = Quaternion.identity;
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		bool flag = false;
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			if (!collider.isTrigger)
			{
				if (flag)
				{
					bounds.Encapsulate(collider.bounds);
					continue;
				}
				bounds = collider.bounds;
				flag = true;
			}
		}
		base.transform.rotation = rotation;
		if (flag)
		{
			boundingBox = bounds.size;
			midPointOffset = base.transform.InverseTransformPoint(bounds.center);
		}
		else
		{
			boundingBox = Vector3.one;
			Debug.LogWarning("No colliders found on the object or its children!");
		}
		int num = 0;
		PhysGrabObjectCollider[] componentsInChildren2 = GetComponentsInChildren<PhysGrabObjectCollider>();
		foreach (PhysGrabObjectCollider physGrabObjectCollider in componentsInChildren2)
		{
			colliders.Add(physGrabObjectCollider.transform);
			physGrabObjectCollider.colliderID = num;
			num++;
		}
		roomVolumeCheck = GetComponent<RoomVolumeCheck>();
		photonView = GetComponent<PhotonView>();
		hinge = GetComponent<PhysGrabHinge>();
		if ((bool)hinge)
		{
			hasHinge = true;
		}
		if (GameManager.instance.gameMode == 1)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				prevTargetPos = base.transform.position;
				prevTargetRot = base.transform.rotation;
				targetPos = base.transform.position;
				targetRot = base.transform.rotation;
				isMaster = true;
			}
			if (PhotonNetwork.IsMasterClient && spawned)
			{
				photonView.TransferOwnership(PhotonNetwork.MasterClient);
			}
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (!GetComponent<EnemyRigidbody>())
			{
				StartCoroutine(EnableRigidbody());
			}
		}
		else
		{
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
		if (!overrideTagsAndLayers)
		{
			return;
		}
		foreach (Transform item in base.transform)
		{
			if (!item.CompareTag("Cart") && !item.CompareTag("Grab Area"))
			{
				if (item.gameObject.layer != LayerMask.NameToLayer("PlayerOnlyCollision") && item.gameObject.layer != LayerMask.NameToLayer("Triggers"))
				{
					item.gameObject.tag = "Phys Grab Object";
				}
				if (item.gameObject.layer != LayerMask.NameToLayer("IgnorePhysGrab") && item.gameObject.layer != LayerMask.NameToLayer("CollisionCheck") && item.gameObject.layer != LayerMask.NameToLayer("CartWheels") && item.gameObject.layer != LayerMask.NameToLayer("PhysGrabObjectHinge") && item.gameObject.layer != LayerMask.NameToLayer("PhysGrabObjectCart") && item.gameObject.layer != LayerMask.NameToLayer("Triggers") && item.gameObject.layer != LayerMask.NameToLayer("PlayerOnlyCollision"))
				{
					item.gameObject.layer = LayerMask.NameToLayer("PhysGrabObject");
				}
			}
		}
	}

	public void OverrideFragility(float multiplier)
	{
		if ((bool)impactDetector && isValuable)
		{
			overrideFragilityTimer = 0.1f;
			impactDetector.fragilityMultiplier = multiplier;
		}
	}

	private void OverrideTimersTick()
	{
		if (timerAlterDeactivate > 0f)
		{
			if (isActive)
			{
				base.transform.position = new Vector3(0f, 3000f, 0f);
			}
			isActive = false;
			rb.detectCollisions = false;
			rb.isKinematic = true;
			if (SemiFunc.IsMultiplayer() && !SemiFunc.MenuLevel() && photonTransformView.enabled)
			{
				photonTransformView.enabled = false;
			}
			timerAlterDeactivate -= Time.fixedDeltaTime;
		}
		else if (timerAlterDeactivate != -123f)
		{
			OverrideDeactivateReset();
		}
		if ((bool)mapCustom && hasMapCustom && !isActive)
		{
			mapCustom.Hide();
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		OverrideStrengthTick();
		if (overrideMassGoDownTimer > 0f)
		{
			overrideMassGoDownTimer -= Time.fixedDeltaTime;
		}
		if (timerAlterMass > 0f)
		{
			rb.mass = alterMassValue;
			timerAlterMass -= Time.fixedDeltaTime;
		}
		else if (timerAlterMass != -123f)
		{
			if (massOriginal == 0f)
			{
				massOriginal = rb.mass;
			}
			ResetMass();
		}
		if ((bool)impactDetector)
		{
			if (overrideFragilityTimer > 0f)
			{
				overrideFragilityTimer -= Time.fixedDeltaTime;
			}
			else if (overrideFragilityTimer != -123f)
			{
				if (impactDetector.fragilityMultiplier != 1f)
				{
					impactDetector.fragilityMultiplier = 1f;
				}
				overrideFragilityTimer = -123f;
			}
		}
		if (overrideAngularDragGoDownTimer > 0f)
		{
			overrideAngularDragGoDownTimer -= Time.fixedDeltaTime;
		}
		if (timerAlterAngularDrag > 0f)
		{
			rb.angularDrag = alterAngularDragValue;
			timerAlterAngularDrag -= Time.fixedDeltaTime;
		}
		else if (timerAlterAngularDrag != -123f)
		{
			rb.angularDrag = angularDragOriginal;
			timerAlterAngularDrag = -123f;
			alterAngularDragValue = 0f;
		}
		if (overrideDragGoDownTimer > 0f)
		{
			overrideDragGoDownTimer -= Time.fixedDeltaTime;
		}
		if (timerAlterDrag > 0f)
		{
			rb.drag = alterDragValue;
			timerAlterDrag -= Time.fixedDeltaTime;
		}
		else if (timerAlterDrag != -123f)
		{
			rb.drag = dragOriginal;
			timerAlterDrag = -123f;
			alterDragValue = 0f;
		}
		if (timerAlterIndestructible > 0f)
		{
			if ((bool)impactDetector)
			{
				impactDetector.isIndestructible = true;
			}
			timerAlterIndestructible -= Time.fixedDeltaTime;
		}
		else if (timerAlterIndestructible != -123f)
		{
			ResetIndestructible();
		}
		if (timerAlterMaterial > 0f)
		{
			timerAlterMaterial -= Time.fixedDeltaTime;
		}
		else if (timerAlterMaterial != -123f)
		{
			foreach (Transform collider in colliders)
			{
				if ((bool)collider)
				{
					collider.GetComponent<Collider>().material = SemiFunc.PhysicMaterialPhysGrabObject();
				}
				else
				{
					colliders.Remove(collider);
				}
			}
			timerAlterMaterial = -123f;
			alterMaterialCurrent = null;
		}
		if (timerZeroGravity > 0f)
		{
			rb.useGravity = false;
			timerZeroGravity -= Time.fixedDeltaTime;
		}
		else if (timerZeroGravity != -123f)
		{
			rb.useGravity = true;
			timerZeroGravity = -123f;
		}
		if ((hasHinge && !hinge.dead && !hinge.broken) || !rb.useGravity)
		{
			return;
		}
		if (grabbed)
		{
			if (timerAlterAngularDrag <= 0f)
			{
				rb.angularDrag = 0.5f;
			}
			if (timerAlterDrag <= 0f)
			{
				rb.drag = 0.5f;
			}
		}
		else
		{
			if (timerAlterAngularDrag <= 0f)
			{
				rb.angularDrag = angularDragOriginal;
			}
			if (timerAlterDrag <= 0f)
			{
				rb.drag = dragOriginal;
			}
		}
	}

	private IEnumerator EnableRigidbody()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.1f);
		spawned = true;
		rb.isKinematic = false;
		if (spawnTorque != Vector3.zero)
		{
			rb.AddTorque(spawnTorque, ForceMode.Impulse);
		}
	}

	private void TickImpactTimers()
	{
		if (impactHappenedTimer > 0f)
		{
			impactHappenedTimer -= Time.fixedDeltaTime;
		}
		if (impactLightTimer > 0f)
		{
			impactLightTimer -= Time.fixedDeltaTime;
		}
		if (impactMediumTimer > 0f)
		{
			impactMediumTimer -= Time.fixedDeltaTime;
		}
		if (impactHeavyTimer > 0f)
		{
			impactHeavyTimer -= Time.fixedDeltaTime;
		}
		if (breakLightTimer > 0f)
		{
			breakLightTimer -= Time.fixedDeltaTime;
		}
		if (breakMediumTimer > 0f)
		{
			breakMediumTimer -= Time.fixedDeltaTime;
		}
		if (breakHeavyTimer > 0f)
		{
			breakHeavyTimer -= Time.fixedDeltaTime;
		}
	}

	public void OverrideGrabStrength(float value, float time = 0.1f)
	{
		overrideGrabStrengthTimer = time;
		overrideGrabStrength = value;
	}

	public void OverrideTorqueStrengthX(float value, float time = 0.1f)
	{
		overrideTorqueStrengthXTimer = time;
		overrideTorqueStrengthX = value;
	}

	public void OverrideTorqueStrengthY(float value, float time = 0.1f)
	{
		overrideTorqueStrengthYTimer = time;
		overrideTorqueStrengthY = value;
	}

	public void OverrideTorqueStrengthZ(float value, float time = 0.1f)
	{
		overrideTorqueStrengthZTimer = time;
		overrideTorqueStrengthZ = value;
	}

	public void OverrideTorqueStrength(float value, float time = 0.1f)
	{
		overrideTorqueStrengthTimer = time;
		overrideTorqueStrength = value;
	}

	public void OverrideStrengthTick()
	{
		if (overrideTorqueStrengthXTimer > 0f)
		{
			overrideTorqueStrengthXTimer -= Time.fixedDeltaTime;
			if (overrideTorqueStrengthXTimer <= 0f)
			{
				overrideTorqueStrengthX = 1f;
			}
		}
		if (overrideTorqueStrengthYTimer > 0f)
		{
			overrideTorqueStrengthYTimer -= Time.fixedDeltaTime;
			if (overrideTorqueStrengthYTimer <= 0f)
			{
				overrideTorqueStrengthY = 1f;
			}
		}
		if (overrideTorqueStrengthZTimer > 0f)
		{
			overrideTorqueStrengthZTimer -= Time.fixedDeltaTime;
			if (overrideTorqueStrengthZTimer <= 0f)
			{
				overrideTorqueStrengthZ = 1f;
			}
		}
		if (overrideTorqueStrengthTimer > 0f)
		{
			overrideTorqueStrengthTimer -= Time.fixedDeltaTime;
			if (overrideTorqueStrengthTimer <= 0f)
			{
				overrideTorqueStrength = 1f;
			}
		}
		if (overrideGrabStrengthTimer > 0f)
		{
			overrideGrabStrengthTimer -= Time.fixedDeltaTime;
			if (overrideGrabStrengthTimer <= 0f)
			{
				overrideGrabStrength = 1f;
			}
		}
	}

	private void FixedUpdate()
	{
		TickImpactTimers();
		OverrideTimersTick();
		OverrideGrabRelativePositionTick();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			rbVelocity = rb.velocity;
			rbAngularVelocity = rb.angularVelocity;
			isKinematic = rb.isKinematic;
			if (!isKinematic)
			{
				float maxLength = 40f;
				float maxLength2 = 30f;
				rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxLength);
				rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxLength2);
			}
		}
		if (frozenTimer > 0f)
		{
			frozenTimer -= Time.fixedDeltaTime;
			rb.MovePosition(frozenPosition);
			rb.MoveRotation(frozenRotation);
			if (!rb.isKinematic)
			{
				rb.velocity = Vector3.zero;
				rbVelocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
				rbAngularVelocity = Vector3.zero;
			}
			return;
		}
		if (frozen)
		{
			rb.AddForce(frozenVelocity, ForceMode.VelocityChange);
			rb.AddTorque(frozenAngularVelocity, ForceMode.VelocityChange);
			rb.AddForce(frozenForce, ForceMode.Impulse);
			rb.AddTorque(frozenTorque, ForceMode.Impulse);
			frozenForce = Vector3.zero;
			frozenTorque = Vector3.zero;
			frozen = false;
			return;
		}
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			rbVelocity = rb.velocity;
			rbAngularVelocity = rb.angularVelocity;
		}
		if (playerGrabbing.Count > 0)
		{
			if (hasNeverBeenGrabbed)
			{
				OverrideIndestructible(0.5f);
				hasNeverBeenGrabbed = false;
			}
			grabbed = true;
			heldByLocalPlayer = false;
			if (GameManager.Multiplayer())
			{
				foreach (PhysGrabber item in playerGrabbing)
				{
					if (item.photonView.IsMine)
					{
						heldByLocalPlayer = true;
					}
				}
			}
			else
			{
				heldByLocalPlayer = true;
			}
		}
		else
		{
			heldByLocalPlayer = false;
			grabbed = false;
		}
		if ((GameManager.Multiplayer() && !isMaster) || rb.isKinematic)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		grabDisplacementCurrent = Vector3.zero;
		foreach (PhysGrabber item2 in playerGrabbing)
		{
			float num = item2.forceMax;
			if (isCart && physGrabCart.inCart.GetComponent<BoxCollider>().bounds.Contains(item2.transform.position))
			{
				num *= 0.25f;
			}
			item2.grabbedPhysGrabObject = this;
			if (item2.physGrabForcesDisabled)
			{
				continue;
			}
			Vector3 physGrabPointPullerPosition = item2.physGrabPointPullerPosition;
			if (overrideGrabRelativeVerticalPositionTimer != 0f)
			{
				Vector3 up = item2.playerAvatar.localCameraTransform.up;
				physGrabPointPullerPosition += up * overrideGrabRelativeVerticalPosition;
			}
			if (overrideGrabRelativeHorizontalPositionTimer != 0f)
			{
				Vector3 right = item2.playerAvatar.localCameraTransform.right;
				physGrabPointPullerPosition += right * overrideGrabRelativeHorizontalPosition;
			}
			Vector3 vector = Vector3.ClampMagnitude(physGrabPointPullerPosition - item2.physGrabPoint.position, num) * 10f;
			vector = Vector3.ClampMagnitude(vector, num);
			Vector3 pointVelocity = rb.GetPointVelocity(item2.physGrabPoint.position);
			Vector3 vector2 = vector * item2.springConstant - pointVelocity * item2.dampingConstant;
			vector2 = Vector3.ClampMagnitude(vector2, num) * 2f;
			if (isMelee)
			{
				vector2 = Vector3.ClampMagnitude(vector2, num) * 4f;
			}
			vector2 *= item2.grabStrength;
			vector2 *= overrideGrabStrength;
			Vector3 force = vector2 * item2.forceConstant;
			if (hasHinge && !hinge.dead && !hinge.broken)
			{
				force *= 2f;
			}
			foreach (PhysGrabObject physGrabObject in item2.playerAvatar.physObjectStander.physGrabObjects)
			{
				if (physGrabObject == this && force.y > 0f)
				{
					force.y = 0f;
				}
			}
			if (isCart && physGrabCart.inCart.GetComponent<BoxCollider>().bounds.Contains(item2.playerAvatar.transform.position) && force.y > 0f)
			{
				force.y = 0f;
			}
			rb.AddForceAtPosition(force, item2.physGrabPoint.position);
			grabDisplacementCurrent += vector * item2.grabStrength;
			if (hasHinge && !hinge.dead && !hinge.broken)
			{
				continue;
			}
			Transform localCameraTransform = item2.playerAvatar.localCameraTransform;
			Vector3 vector3 = localCameraTransform.TransformDirection(item2.physRotation * item2.cameraRelativeGrabbedForward);
			Vector3 vector4 = localCameraTransform.TransformDirection(item2.physRotation * item2.cameraRelativeGrabbedUp);
			Vector3 forward = base.transform.forward;
			Vector3 up2 = base.transform.up;
			Vector3 zero2 = Vector3.zero;
			Vector3 vector5 = Vector3.Cross(forward, vector3);
			if (vector5.sqrMagnitude > 1E-08f)
			{
				zero2 += vector5.normalized * Mathf.Clamp(Vector3.Angle(forward, vector3), 0f, 60f);
			}
			Vector3 vector6 = Vector3.Cross(up2, vector4);
			if (vector6.sqrMagnitude > 1E-08f)
			{
				zero2 += vector6.normalized * Mathf.Clamp(Vector3.Angle(up2, vector4), 0f, 60f);
			}
			zero2 *= rb.mass;
			zero2 = Vector3.ClampMagnitude(zero2, 60f).normalized;
			if (rb.mass < 1f)
			{
				zero2 *= 0.75f;
			}
			if (isMelee && item2.grabStrength > 0f)
			{
				OverrideMass(massOriginal + item2.grabStrength * 0.2f);
			}
			if (isMelee)
			{
				Vector3 vector7 = Vector3.zero;
				Vector3 vector8 = Vector3.Cross(base.transform.forward, vector3);
				if (vector8.sqrMagnitude > 1E-08f)
				{
					vector7 = vector8.normalized * Vector3.Angle(base.transform.forward, vector3);
				}
				Vector3 vector9 = Vector3.zero;
				Vector3 vector10 = Vector3.Cross(base.transform.up, vector4);
				if (vector10.sqrMagnitude > 1E-08f)
				{
					vector9 = vector10.normalized * Vector3.Angle(base.transform.up, vector4);
				}
				zero2 = vector7.normalized + vector9 * 3f;
				float num2 = 0.8f;
				zero2 *= num2;
			}
			float num3 = Mathf.Clamp01(rb.mass);
			num3 = Mathf.Max(num3, 1f);
			if (massOriginal > 2f)
			{
				num3 *= item2.grabStrength;
			}
			float num4 = 10f;
			if (item2.mouseTurningVelocity.magnitude > 0.1f && massOriginal > 1f)
			{
				float num5 = Mathf.Max(rb.mass, 0.1f);
				num4 = 2f / num5;
				float num6 = 1f + boundingBox.magnitude;
				num4 += num6;
				if (num4 < 1f)
				{
					num4 = 1f;
				}
				if (num4 > 10f)
				{
					num4 = 10f;
				}
				zero2 *= num4;
			}
			float num7 = Mathf.Clamp01(rb.mass);
			if (rb.mass > 1f)
			{
				num7 *= 0.9f;
			}
			if (!isMelee)
			{
				zero2 *= 5f;
			}
			else
			{
				zero2 *= 0.05f;
			}
			zero2 *= overrideTorqueStrength;
			float num8 = Vector3.Angle(base.transform.forward, vector3) / 180f;
			float num9 = Vector3.Angle(base.transform.up, vector4) / 180f;
			float num10 = num8 + num9;
			zero2 *= num10 * 15f * num7 * Time.fixedDeltaTime;
			Vector3 direction = base.transform.InverseTransformDirection(zero2);
			float num11 = overrideTorqueStrengthX;
			float num12 = overrideTorqueStrengthY;
			float num13 = overrideTorqueStrengthZ;
			if (num11 > 1f)
			{
				num11 *= num10;
			}
			if (num12 > 1f)
			{
				num12 *= num10;
			}
			if (num13 > 1f)
			{
				num13 *= num10;
			}
			direction.x *= num11;
			direction.y *= num12;
			direction.z *= num13;
			zero2 = base.transform.TransformDirection(direction);
			Vector3 vector11 = zero2 * num7;
			zero += vector11;
			rb.angularVelocity *= 0.9f;
		}
		if (zero.magnitude > 0f)
		{
			rb.AddTorque(zero, ForceMode.Impulse);
		}
	}

	public void OverrideGrabVerticalPosition(float pos)
	{
		overrideGrabRelativeVerticalPosition = pos;
		overrideGrabRelativeVerticalPositionTimer = 0.1f;
	}

	public void OverrideGrabHorizontalPosition(float pos)
	{
		overrideGrabRelativeHorizontalPosition = pos;
		overrideGrabRelativeHorizontalPositionTimer = 0.1f;
	}

	private void OverrideGrabRelativePositionTick()
	{
		if (overrideGrabRelativeHorizontalPositionTimer > 0f)
		{
			overrideGrabRelativeHorizontalPositionTimer -= Time.deltaTime;
			if (overrideGrabRelativeHorizontalPositionTimer <= 0f)
			{
				overrideGrabRelativeHorizontalPosition = 0f;
			}
		}
		if (overrideGrabRelativeVerticalPositionTimer > 0f)
		{
			overrideGrabRelativeVerticalPositionTimer -= Time.deltaTime;
			if (overrideGrabRelativeVerticalPositionTimer <= 0f)
			{
				overrideGrabRelativeVerticalPosition = 0f;
			}
		}
	}

	public void OverrideZeroGravity(float time = 0.1f)
	{
		timerZeroGravity = time;
	}

	public void OverrideDrag(float value, float time = 0.1f)
	{
		timerAlterDrag = time;
		if (alterDragValue <= value)
		{
			alterDragValue = value;
			overrideDragGoDownTimer = 0.1f;
		}
		else if (overrideDragGoDownTimer <= 0f)
		{
			alterDragValue = value;
		}
	}

	public void OverrideAngularDrag(float value, float time = 0.1f)
	{
		timerAlterAngularDrag = time;
		if (alterAngularDragValue <= value)
		{
			alterAngularDragValue = value;
			overrideAngularDragGoDownTimer = 0.1f;
		}
		else if (overrideAngularDragGoDownTimer <= 0f)
		{
			timerAlterAngularDrag = value;
		}
	}

	public void OverrideIndestructible(float time = 0.1f)
	{
		timerAlterIndestructible = time;
	}

	public void OverrideDeactivate(float time = 0.1f)
	{
		timerAlterDeactivate = time;
		rb.isKinematic = true;
	}

	public void OverrideDeactivateReset()
	{
		isActive = true;
		rb.detectCollisions = true;
		if (spawned)
		{
			rb.isKinematic = false;
		}
		if (SemiFunc.IsMultiplayer() && !SemiFunc.MenuLevel())
		{
			photonTransformView.enabled = true;
		}
		timerAlterDeactivate = -123f;
	}

	public void OverrideBreakEffects(float _time)
	{
		overrideDisableBreakEffectsTimer = _time;
	}

	public void OverrideMaterial(PhysicMaterial material, float time = 0.1f)
	{
		if (alterMaterialCurrent != alterMaterialPrevious || alterMaterialCurrent == null)
		{
			alterMaterialPrevious = alterMaterialCurrent;
			foreach (Transform collider in colliders)
			{
				if ((bool)collider)
				{
					Collider component = collider.GetComponent<Collider>();
					if ((bool)component)
					{
						component.material = material;
					}
				}
				else
				{
					colliders.Remove(collider);
				}
			}
		}
		alterMaterialCurrent = material;
		timerAlterMaterial = time;
	}

	public void ResetIndestructible()
	{
		if ((bool)impactDetector)
		{
			impactDetector.isIndestructible = false;
		}
		timerAlterIndestructible = -123f;
	}

	public void SetPositionLogic(Vector3 _position, Quaternion _rotation)
	{
		photonTransformView.Teleport(_position, _rotation);
	}

	[PunRPC]
	private void SetPositionRPC(Vector3 position, Quaternion rotation)
	{
		if (SemiFunc.IsMultiplayer())
		{
			SetPositionLogic(position, rotation);
			return;
		}
		base.transform.position = position;
		base.transform.rotation = rotation;
		rb.position = position;
		rb.rotation = rotation;
	}

	public void Teleport(Vector3 position, Quaternion rotation)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonTransformView.Teleport(position, rotation);
				return;
			}
			photonView.RPC("SetPositionRPC", RpcTarget.MasterClient, position, rotation);
		}
		else
		{
			base.transform.position = position;
			base.transform.rotation = rotation;
			rb.position = position;
			rb.rotation = rotation;
		}
	}

	public void OverrideMass(float value, float time = 0.1f)
	{
		timerAlterMass = time;
		if (alterMassValue <= value)
		{
			alterMassValue = value;
			overrideMassGoDownTimer = 0.1f;
		}
		else if (overrideMassGoDownTimer <= 0f)
		{
			alterMassValue = value;
		}
	}

	public void ResetMass()
	{
		rb.mass = massOriginal;
		timerAlterMass = -123f;
		alterMassValue = 0f;
	}

	private void Update()
	{
		if (grabbed)
		{
			for (int i = 0; i < playerGrabbing.Count; i++)
			{
				if (!playerGrabbing[i])
				{
					playerGrabbing.RemoveAt(i);
				}
			}
		}
		midPoint = base.transform.TransformPoint(midPointOffset);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (playerGrabbing.Count > 0)
			{
				lastPlayerGrabbing = playerGrabbing[playerGrabbing.Count - 1].playerAvatar;
				grabbedTimer = 0.5f;
			}
			else if ((bool)lastPlayerGrabbing)
			{
				grabbedTimer -= Time.deltaTime;
				if (grabbedTimer <= 0f)
				{
					lastPlayerGrabbing = null;
				}
			}
			if (enemyInteractTimer > 0f)
			{
				enemyInteractTimer -= Time.deltaTime;
				if (playerGrabbing.Count > 0)
				{
					enemyInteractTimer = 0f;
				}
			}
			if (hasImpactDetector && !impactDetector.isIndestructible)
			{
				if (heavyImpactImpulse)
				{
					impactDetector.ImpactHeavy(150f, centerPoint);
					heavyImpactImpulse = false;
				}
				if (mediumImpactImpulse)
				{
					impactDetector.ImpactMedium(80f, centerPoint);
					mediumImpactImpulse = false;
				}
				if (lightImpactImpulse)
				{
					impactDetector.ImpactLight(20f, centerPoint);
					lightImpactImpulse = false;
				}
				if (heavyBreakImpulse)
				{
					if (isValuable)
					{
						impactDetector.BreakHeavy(centerPoint);
					}
					else
					{
						impactDetector.Break(0f, centerPoint, impactDetector.breakLevelHeavy);
					}
					heavyBreakImpulse = false;
				}
				if (mediumBreakImpulse)
				{
					if (isValuable)
					{
						impactDetector.BreakMedium(centerPoint);
					}
					else
					{
						impactDetector.Break(0f, centerPoint, impactDetector.breakLevelMedium);
					}
					mediumBreakImpulse = false;
				}
				if (lightBreakImpulse)
				{
					if (isValuable)
					{
						impactDetector.BreakLight(centerPoint);
					}
					else
					{
						impactDetector.Break(0f, centerPoint, impactDetector.breakLevelLight);
					}
					lightBreakImpulse = false;
				}
			}
			if (overrideDisableBreakEffectsTimer > 0f)
			{
				overrideDisableBreakEffectsTimer -= Time.deltaTime;
			}
			if (dead && playerGrabbing.Count == 0)
			{
				DestroyPhysGrabObject();
			}
		}
		if (grabDisableTimer > 0f)
		{
			grabDisableTimer -= Time.deltaTime;
		}
		centerPoint = midPoint;
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !(base.transform.position.y < -50f))
		{
			return;
		}
		if (impactDetector.destroyDisable)
		{
			if (impactDetector.destroyDisableTeleport)
			{
				Teleport(TruckSafetySpawnPoint.instance.transform.position, TruckSafetySpawnPoint.instance.transform.rotation);
			}
		}
		else
		{
			impactDetector.DestroyObject();
		}
	}

	public void EnemyInteractTimeSet()
	{
		enemyInteractTimer = 10f;
	}

	public void FreezeForces(float _time, Vector3 _force, Vector3 _torque)
	{
		if (!rb.isKinematic)
		{
			frozenTimer = _time;
			if (!frozen)
			{
				frozenPosition = base.transform.position;
				frozenRotation = base.transform.rotation;
				frozenVelocity = rb.velocity;
				frozenAngularVelocity = rb.angularVelocity;
				frozenForce = Vector3.zero;
				frozenTorque = Vector3.zero;
				frozen = true;
			}
			frozenForce += _force;
			frozenTorque += _torque;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}

	private void OnDestroy()
	{
		if (RoundDirector.instance.dollarHaulList.Contains(base.gameObject))
		{
			RoundDirector.instance.dollarHaulList.Remove(base.gameObject);
		}
	}

	public void DestroyPhysGrabObject()
	{
		if (GameManager.instance.gameMode == 0)
		{
			DestroyPhysGrabObjectRPC();
		}
		else
		{
			photonView.RPC("DestroyPhysGrabObjectRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void DestroyPhysGrabObjectRPC()
	{
		Object.Destroy(base.gameObject);
	}

	private void OnDisable()
	{
		RoundDirector.instance.PhysGrabObjectRemove(this);
	}

	private void OnEnable()
	{
		RoundDirector.instance.PhysGrabObjectAdd(this);
	}

	public void GrabStarted(PhysGrabber player)
	{
		if (grabbedLocal)
		{
			return;
		}
		grabbedLocal = true;
		if (GameManager.instance.gameMode == 0)
		{
			if (!playerGrabbing.Contains(player))
			{
				playerGrabbing.Add(player);
			}
		}
		else
		{
			photonView.RPC("GrabStartedRPC", RpcTarget.MasterClient, player.photonView.ViewID);
		}
	}

	public void GrabEnded(PhysGrabber player)
	{
		if (!grabbedLocal)
		{
			return;
		}
		grabbedLocal = false;
		if (GameManager.instance.gameMode == 0)
		{
			Throw(player);
			if (playerGrabbing.Contains(player))
			{
				playerGrabbing.Remove(player);
			}
		}
		else
		{
			photonView.RPC("GrabEndedRPC", RpcTarget.MasterClient, player.photonView.ViewID);
		}
	}

	public void GrabLink(int playerPhotonID, int colliderID, Vector3 point, Vector3 cameraRelativeGrabbedForward, Vector3 cameraRelativeGrabbedUp)
	{
		photonView.RPC("GrabLinkRPC", RpcTarget.All, playerPhotonID, colliderID, point, cameraRelativeGrabbedForward, cameraRelativeGrabbedUp);
	}

	[PunRPC]
	private void GrabLinkRPC(int playerPhotonID, int colliderID, Vector3 point, Vector3 cameraRelativeGrabbedForward, Vector3 cameraRelativeGrabbedUp)
	{
		PhysGrabber component = PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>();
		component.physGrabPoint.position = point;
		component.localGrabPosition = base.transform.InverseTransformPoint(point);
		component.grabbedObjectTransform = base.transform;
		component.grabbedPhysGrabObjectColliderID = colliderID;
		component.grabbedPhysGrabObjectCollider = FindColliderFromID(colliderID).GetComponent<Collider>();
		component.grabbed = true;
		Transform localCameraTransform = component.playerAvatar.localCameraTransform;
		if (playerGrabbing.Count != 0)
		{
			component.cameraRelativeGrabbedForward = localCameraTransform.InverseTransformDirection(base.transform.forward);
			component.cameraRelativeGrabbedUp = localCameraTransform.InverseTransformDirection(base.transform.up);
		}
		else
		{
			component.cameraRelativeGrabbedForward = localCameraTransform.InverseTransformDirection(base.transform.forward);
			component.cameraRelativeGrabbedUp = localCameraTransform.InverseTransformDirection(base.transform.up);
			camRelForward = base.transform.InverseTransformDirection(base.transform.forward);
			camRelUp = base.transform.InverseTransformDirection(base.transform.up);
		}
		component.cameraRelativeGrabbedForward = component.cameraRelativeGrabbedForward.normalized;
		component.cameraRelativeGrabbedUp = component.cameraRelativeGrabbedUp.normalized;
		if (component.photonView.IsMine)
		{
			Vector3 localGrabPosition = component.localGrabPosition;
			photonView.RPC("GrabPointSyncRPC", RpcTarget.All, playerPhotonID, localGrabPosition);
		}
	}

	[PunRPC]
	private void GrabPointSyncRPC(int playerPhotonID, Vector3 localPointInBox)
	{
		PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>().localGrabPosition = localPointInBox;
	}

	[PunRPC]
	private void GrabStartedRPC(int playerPhotonID)
	{
		PhysGrabber component = PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>();
		if (!playerGrabbing.Contains(component))
		{
			photonView.RPC("GrabPlayerAddRPC", RpcTarget.All, playerPhotonID);
		}
	}

	[PunRPC]
	private void GrabPlayerAddRPC(int photonViewID)
	{
		PhysGrabber component = PhotonView.Find(photonViewID).GetComponent<PhysGrabber>();
		playerGrabbing.Add(component);
	}

	[PunRPC]
	private void GrabPlayerRemoveRPC(int photonViewID)
	{
		PhysGrabber component = PhotonView.Find(photonViewID).GetComponent<PhysGrabber>();
		playerGrabbing.Remove(component);
	}

	private void Throw(PhysGrabber player)
	{
		float num = Mathf.Max(rb.mass * 1.5f, 1f);
		Vector3 force = Vector3.ClampMagnitude(player.physGrabPointPullerPosition - player.physGrabPoint.position, player.forceMax) * num;
		force *= 0.5f + player.throwStrength;
		rb.AddForce(force, ForceMode.Impulse);
	}

	[PunRPC]
	private void GrabEndedRPC(int playerPhotonID)
	{
		PhysGrabber component = PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>();
		Throw(component);
		component.grabbed = false;
		if (playerGrabbing.Contains(component))
		{
			photonView.RPC("GrabPlayerRemoveRPC", RpcTarget.All, playerPhotonID);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			if (!impactDetector)
			{
				impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
			}
			if (!rb)
			{
				rb = GetComponent<Rigidbody>();
			}
			stream.SendNext(rbVelocity);
			stream.SendNext(rbAngularVelocity);
			stream.SendNext(impactDetector.isSliding);
			stream.SendNext(isKinematic);
		}
		else
		{
			if (!impactDetector)
			{
				impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
			}
			rbVelocity = (Vector3)stream.ReceiveNext();
			rbAngularVelocity = (Vector3)stream.ReceiveNext();
			impactDetector.isSliding = (bool)stream.ReceiveNext();
			isKinematic = (bool)stream.ReceiveNext();
			lastUpdateTime = Time.time;
		}
	}

	public Transform FindColliderFromID(int colliderID)
	{
		foreach (Transform collider in colliders)
		{
			if (collider.GetComponent<PhysGrabObjectCollider>().colliderID == colliderID)
			{
				return collider;
			}
		}
		return null;
	}
}
