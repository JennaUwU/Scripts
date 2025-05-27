using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
	public static CameraZoom Instance;

	public PlayerController PlayerController;

	public List<Camera> cams;

	public CameraTarget camController;

	public AnimNoise camNoise;

	public float playerZoomDefault;

	public float SprintZoom;

	private float SprintZoomCurrent;

	private float TumbleVelocityZoom;

	private float zoomLerp;

	private float zoomPrev;

	private float zoomCurrent;

	private float zoomNew;

	private GameObject OverrideZoomObject;

	private float OverrideZoomTimer;

	private float OverrideZoomSpeedIn;

	private float OverrideZoomSpeedOut;

	public AnimationCurve OverrideZoomCurve;

	private int OverrideZoomPriority = 999;

	private bool OverrideActive;

	private void Awake()
	{
		Instance = this;
		zoomPrev = playerZoomDefault;
		zoomNew = playerZoomDefault;
	}

	public void OverrideZoomSet(float zoom, float time, float speedIn, float speedOut, GameObject obj, int priority)
	{
		if (priority <= OverrideZoomPriority)
		{
			if (obj != OverrideZoomObject)
			{
				zoomLerp = 0f;
				zoomPrev = zoomCurrent;
			}
			zoomNew = zoom;
			OverrideZoomObject = obj;
			OverrideZoomTimer = time;
			OverrideZoomSpeedIn = speedIn;
			OverrideZoomSpeedOut = speedOut;
			OverrideZoomPriority = priority;
			OverrideActive = true;
		}
	}

	private void Update()
	{
		if ((bool)SpectateCamera.instance || !LevelGenerator.Instance.Generated || PlayerController.playerAvatarScript.isDisabled)
		{
			return;
		}
		if (OverrideZoomTimer > 0f)
		{
			OverrideZoomTimer -= Time.deltaTime;
			zoomLerp += Time.deltaTime * OverrideZoomSpeedIn;
		}
		else if (OverrideZoomTimer <= 0f)
		{
			if (OverrideActive)
			{
				OverrideActive = false;
				OverrideZoomObject = null;
				OverrideZoomPriority = 999;
				zoomLerp = 0f;
				zoomPrev = zoomCurrent;
				zoomNew = playerZoomDefault;
			}
			zoomLerp += Time.deltaTime * OverrideZoomSpeedOut;
		}
		zoomLerp = Mathf.Clamp01(zoomLerp);
		if (PlayerController.CanSlide)
		{
			float num = SprintZoom + (float)StatsManager.instance.playerUpgradeSpeed[PlayerController.instance.playerAvatarScript.steamID] * 2f;
			num *= GameplayManager.instance.cameraAnimation;
			float num2 = Mathf.Lerp(0f, num, PlayerController.SprintSpeedLerp);
			SprintZoomCurrent = Mathf.Lerp(SprintZoomCurrent, num2, 2f * Time.deltaTime);
		}
		else
		{
			SprintZoomCurrent = Mathf.Lerp(SprintZoomCurrent, 0f, 2f * Time.deltaTime);
		}
		if (PlayerController.playerAvatarScript.isTumbling)
		{
			float value = PlayerController.playerAvatarScript.tumble.physGrabObject.rbVelocity.magnitude * 5f;
			value = Mathf.Clamp(value, 0f, 30f);
			value *= GameplayManager.instance.cameraAnimation;
			TumbleVelocityZoom = Mathf.Lerp(TumbleVelocityZoom, value, 2f * Time.deltaTime);
		}
		else
		{
			TumbleVelocityZoom = Mathf.Lerp(TumbleVelocityZoom, 0f, 2f * Time.deltaTime);
		}
		zoomCurrent = Mathf.LerpUnclamped(zoomPrev, zoomNew, OverrideZoomCurve.Evaluate(zoomLerp));
		if (SemiFunc.MenuLevel() && (bool)CameraNoPlayerTarget.instance)
		{
			zoomCurrent = CameraNoPlayerTarget.instance.cam.fieldOfView;
		}
		foreach (Camera cam in cams)
		{
			cam.fieldOfView = zoomCurrent + SprintZoomCurrent + TumbleVelocityZoom;
		}
	}
}
