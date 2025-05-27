using UnityEngine;

public class CameraAim : MonoBehaviour
{
	public static CameraAim Instance;

	public CameraTarget camController;

	public Transform playerTransform;

	public float AimSpeedMouse = 1f;

	public float AimSpeedGamepad = 1f;

	private float aimVertical;

	private float aimHorizontal;

	internal float aimSmoothOriginal = 2f;

	private Quaternion playerAim = Quaternion.identity;

	private Vector3 AimTargetPosition = Vector3.zero;

	public AnimationCurve AimTargetCurve;

	[Space]
	public bool AimTargetActive;

	private float AimTargetTimer;

	private float AimTargetSpeed;

	private float AimTargetLerp;

	private GameObject AimTargetObject;

	private int AimTargetPriority = 999;

	private bool AimTargetSoftActive;

	private float AimTargetSoftTimer;

	private float AimTargetSoftStrengthCurrent;

	private float AimTargetSoftStrength;

	private float AimTargetSoftStrengthNoAim;

	private Vector3 AimTargetSoftPosition;

	private GameObject AimTargetSoftObject;

	private int AimTargetSoftPriority = 999;

	private float overrideAimStopTimer;

	internal bool overrideAimStop;

	private float PlayerAimingTimer;

	private float overrideAimSmooth;

	private float overrideAimSmoothTimer;

	private void Awake()
	{
		Instance = this;
	}

	public void AimTargetSet(Vector3 position, float time, float speed, GameObject obj, int priority)
	{
		if (priority <= AimTargetPriority && (!(obj != AimTargetObject) || AimTargetLerp == 0f))
		{
			AimTargetActive = true;
			AimTargetObject = obj;
			AimTargetPosition = position;
			AimTargetTimer = time;
			AimTargetSpeed = speed;
			AimTargetPriority = priority;
		}
	}

	public void AimTargetSoftSet(Vector3 position, float time, float strength, float strengthNoAim, GameObject obj, int priority)
	{
		if (priority <= AimTargetSoftPriority && (!AimTargetSoftObject || !(obj != AimTargetSoftObject)))
		{
			if (obj != AimTargetSoftObject)
			{
				PlayerAimingTimer = 0f;
			}
			AimTargetSoftPosition = position;
			AimTargetSoftTimer = time;
			AimTargetSoftStrength = strength;
			AimTargetSoftStrengthNoAim = strengthNoAim;
			AimTargetSoftObject = obj;
			AimTargetSoftPriority = priority;
		}
	}

	public void CameraAimSpawn(float _rotation)
	{
		aimHorizontal = _rotation;
		playerAim = Quaternion.Euler(aimVertical, aimHorizontal, 0f);
		base.transform.localRotation = playerAim;
	}

	public void OverrideAimStop()
	{
		overrideAimStopTimer = 0.2f;
	}

	private void OverrideAimStopTick()
	{
		if (overrideAimStopTimer > 0f)
		{
			overrideAimStop = true;
			overrideAimStopTimer -= Time.deltaTime;
		}
		else
		{
			overrideAimStop = false;
		}
	}

	private void Update()
	{
		AimSpeedMouse = Mathf.Lerp(0.2f, 4f, GameplayManager.instance.aimSensitivity / 100f);
		if (GameDirector.instance.currentState >= GameDirector.gameState.Main)
		{
			if (!GameDirector.instance.DisableInput && AimTargetTimer <= 0f && !overrideAimStop)
			{
				InputManager.instance.mouseSensitivity = 0.05f;
				Vector2 vector = new Vector2(SemiFunc.InputMouseX(), SemiFunc.InputMouseY());
				Vector2 vector2 = new Vector2(Input.GetAxis("Gamepad Aim X"), Input.GetAxis("Gamepad Aim Y"));
				vector2 = Vector2.zero;
				if (AimTargetSoftTimer > 0f)
				{
					vector = ((!(vector.magnitude > 1f)) ? Vector2.zero : vector.normalized);
					vector2 = ((!(vector2.magnitude > 0.1f)) ? Vector2.zero : vector2.normalized);
				}
				else
				{
					vector *= AimSpeedMouse;
					vector2 *= AimSpeedGamepad * Time.deltaTime;
				}
				aimHorizontal += vector[0];
				aimHorizontal += vector2[0];
				if (aimHorizontal > 360f)
				{
					aimHorizontal -= 360f;
				}
				if (aimHorizontal < -360f)
				{
					aimHorizontal += 360f;
				}
				aimVertical += 0f - vector[1];
				aimVertical += 0f - vector2[1];
				aimVertical = Mathf.Clamp(aimVertical, -70f, 80f);
				playerAim = Quaternion.Euler(aimVertical, aimHorizontal, 0f);
				if (GameplayManager.instance.cameraSmoothing != 0f)
				{
					playerAim = Quaternion.RotateTowards(base.transform.localRotation, playerAim, 10000f * Time.deltaTime);
				}
				if (vector2.magnitude > 0f || vector.magnitude > 0f)
				{
					PlayerAimingTimer = 0.1f;
				}
			}
			if (PlayerAimingTimer > 0f)
			{
				PlayerAimingTimer -= Time.deltaTime;
			}
			if (AimTargetTimer > 0f)
			{
				AimTargetTimer -= Time.deltaTime;
				AimTargetLerp += Time.deltaTime * AimTargetSpeed;
				AimTargetLerp = Mathf.Clamp01(AimTargetLerp);
			}
			else if (AimTargetLerp > 0f)
			{
				ResetPlayerAim(base.transform.localRotation);
				AimTargetLerp = 0f;
				AimTargetPriority = 999;
				AimTargetActive = false;
			}
			Quaternion rotation = Quaternion.LerpUnclamped(playerAim, Quaternion.LookRotation(AimTargetPosition - base.transform.position), AimTargetCurve.Evaluate(AimTargetLerp));
			if (AimTargetSoftTimer > 0f && AimTargetTimer <= 0f)
			{
				float num = AimTargetSoftStrength;
				if (PlayerAimingTimer <= 0f)
				{
					num = AimTargetSoftStrengthNoAim;
				}
				AimTargetSoftStrengthCurrent = Mathf.Lerp(AimTargetSoftStrengthCurrent, num, 10f * Time.deltaTime);
				Quaternion quaternion = Quaternion.LookRotation(AimTargetSoftPosition - base.transform.position);
				rotation = Quaternion.Lerp(rotation, quaternion, num * Time.deltaTime);
				AimTargetSoftTimer -= Time.deltaTime;
				if (AimTargetSoftTimer <= 0f)
				{
					AimTargetSoftObject = null;
					AimTargetSoftPriority = 999;
				}
			}
			float num2 = (aimSmoothOriginal = Mathf.Lerp(50f, 8f, GameplayManager.instance.cameraSmoothing / 100f));
			if (overrideAimSmoothTimer > 0f)
			{
				num2 = overrideAimSmooth;
			}
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, rotation, num2 * Time.deltaTime);
			ResetPlayerAim(rotation);
		}
		if (SemiFunc.MenuLevel() && (bool)CameraNoPlayerTarget.instance)
		{
			base.transform.localRotation = CameraNoPlayerTarget.instance.transform.rotation;
		}
		if (overrideAimSmoothTimer > 0f)
		{
			overrideAimSmoothTimer -= Time.deltaTime;
		}
		OverrideAimStopTick();
	}

	private void ResetPlayerAim(Quaternion _rotation)
	{
		if (_rotation.eulerAngles.x > 180f)
		{
			aimVertical = _rotation.eulerAngles.x - 360f;
		}
		else
		{
			aimVertical = _rotation.eulerAngles.x;
		}
		aimHorizontal = _rotation.eulerAngles.y;
		playerAim = _rotation;
	}

	public void OverrideAimSmooth(float _smooth, float _time)
	{
		overrideAimSmooth = _smooth;
		overrideAimSmoothTimer = _time;
	}
}
