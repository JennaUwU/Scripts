using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerDeathHead : MonoBehaviour
{
	public PlayerAvatar playerAvatar;

	public MeshRenderer headRenderer;

	public ParticleSystem smokeParticles;

	public MapCustom mapCustom;

	public GameObject arenaCrown;

	private float smokeParticleTime = 3f;

	private float smokeParticleTimer;

	private float smokeParticleRateOverTimeDefault;

	private float smokeParticleRateOverTimeCurrent;

	private float smokeParticleRateOverDistanceDefault;

	private float smokeParticleRateOverDistanceCurrent;

	internal PhysGrabObject physGrabObject;

	private PhotonView photonView;

	private RoomVolumeCheck roomVolumeCheck;

	private bool setup;

	private bool triggered;

	private float triggeredTimer;

	internal bool inExtractionPoint;

	private bool inExtractionPointPrevious;

	internal bool inTruck;

	private bool inTruckPrevious;

	[Space]
	public MeshRenderer[] eyeRenderers;

	public Light eyeFlashLight;

	public Color eyeFlashPositiveColor;

	public Color eyeFlashNegativeColor;

	public float eyeFlashStrength;

	public float eyeFlashLightIntensity;

	public Sound eyeFlashPositiveSound;

	public Sound eyeFlashNegativeSound;

	private Material eyeMaterial;

	private int eyeMaterialAmount;

	private int eyeMaterialColor;

	private AnimationCurve eyeFlashCurve;

	private float eyeFlashLerp;

	private bool eyeFlash;

	public AudioClip seenSound;

	private bool serverSeen;

	private float seenCooldownTime = 2f;

	private float seenCooldownTimer;

	private bool localSeen;

	private bool localSeenEffect;

	private float localSeenEffectTime = 2f;

	private float localSeenEffectTimer;

	private float outsideLevelTimer;

	private bool tutorialPossible;

	private float tutorialTimer;

	private float inTruckReviveTimer;

	private Collider[] colliders;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		roomVolumeCheck = GetComponent<RoomVolumeCheck>();
		smokeParticleRateOverTimeDefault = smokeParticles.emission.rateOverTime.constant;
		smokeParticleRateOverDistanceDefault = smokeParticles.emission.rateOverDistance.constant;
		localSeenEffectTimer = localSeenEffectTime;
		MeshRenderer[] array = eyeRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!eyeMaterial)
			{
				eyeMaterial = meshRenderer.material;
			}
			meshRenderer.material = eyeMaterial;
		}
		eyeMaterialAmount = Shader.PropertyToID("_ColorOverlayAmount");
		eyeMaterialColor = Shader.PropertyToID("_ColorOverlay");
		eyeFlashCurve = AssetManager.instance.animationCurveImpact;
		smokeParticleTimer = smokeParticleTime;
		physGrabObject.impactDetector.destroyDisableTeleport = false;
		colliders = GetComponentsInChildren<Collider>();
		SetColliders(_enabled: false);
		StartCoroutine(Setup());
	}

	private IEnumerator Setup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("SetupRPC", RpcTarget.OthersBuffered, playerAvatar.playerName);
			}
			SetupDone();
			physGrabObject.Teleport(new Vector3(0f, 3000f, 0f), Quaternion.identity);
			if (SemiFunc.RunIsArena())
			{
				physGrabObject.impactDetector.destroyDisable = false;
			}
			setup = true;
		}
	}

	private IEnumerator SetupClient()
	{
		while (!physGrabObject)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (!physGrabObject.impactDetector)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (!physGrabObject.impactDetector.particles)
		{
			yield return new WaitForSeconds(0.1f);
		}
		SetupDone();
	}

	private void SetupDone()
	{
		if (!playerAvatar)
		{
			Debug.LogError("PlayerDeathHead: PlayerAvatar not found", base.gameObject);
			return;
		}
		if (SemiFunc.RunIsLevel() && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialReviving, 1) && !playerAvatar.isLocal)
		{
			tutorialPossible = true;
		}
		base.transform.parent = playerAvatar.transform.parent;
		if (SemiFunc.IsMultiplayer() && playerAvatar == SessionManager.instance.CrownedPlayerGet())
		{
			arenaCrown.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (!serverSeen)
		{
			mapCustom.Hide();
		}
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && setup)
		{
			if (!triggered)
			{
				physGrabObject.OverrideDeactivate();
			}
			else if (triggeredTimer > 0f)
			{
				physGrabObject.OverrideDeactivate();
				triggeredTimer -= Time.deltaTime;
				if (triggeredTimer <= 0f)
				{
					physGrabObject.OverrideDeactivateReset();
					physGrabObject.rb.AddForce(playerAvatar.localCameraTransform.up * 2f, ForceMode.Impulse);
					physGrabObject.rb.AddForce(physGrabObject.transform.forward * 0.5f, ForceMode.Impulse);
					physGrabObject.rb.AddTorque(physGrabObject.transform.right * 0.2f, ForceMode.Impulse);
				}
			}
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (triggered)
			{
				inExtractionPoint = roomVolumeCheck.inExtractionPoint;
				if (inExtractionPoint != inExtractionPointPrevious)
				{
					if (GameManager.Multiplayer())
					{
						photonView.RPC("FlashEyeRPC", RpcTarget.All, inExtractionPoint);
					}
					else
					{
						FlashEyeRPC(inExtractionPoint);
					}
					inExtractionPointPrevious = inExtractionPoint;
				}
			}
			else
			{
				inExtractionPoint = false;
				inExtractionPointPrevious = false;
			}
		}
		if (smokeParticles.isPlaying)
		{
			smokeParticleTimer -= Time.deltaTime;
			if (smokeParticleTimer <= 0f)
			{
				smokeParticleRateOverTimeCurrent -= 1f * Time.deltaTime;
				smokeParticleRateOverTimeCurrent = Mathf.Max(smokeParticleRateOverTimeCurrent, 0f);
				smokeParticleRateOverDistanceCurrent -= 10f * Time.deltaTime;
				smokeParticleRateOverDistanceCurrent = Mathf.Max(smokeParticleRateOverDistanceCurrent, 0f);
				ParticleSystem.EmissionModule emission = smokeParticles.emission;
				emission.rateOverTime = new ParticleSystem.MinMaxCurve(smokeParticleRateOverTimeCurrent);
				emission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeParticleRateOverDistanceCurrent);
				if (smokeParticleRateOverTimeCurrent <= 0f && smokeParticleRateOverDistanceCurrent <= 0f)
				{
					smokeParticles.Stop();
				}
			}
		}
		if (eyeFlash)
		{
			eyeFlashLerp += 2f * Time.deltaTime;
			eyeFlashLerp = Mathf.Clamp01(eyeFlashLerp);
			eyeMaterial.SetFloat(eyeMaterialAmount, eyeFlashCurve.Evaluate(eyeFlashLerp));
			eyeFlashLight.intensity = eyeFlashCurve.Evaluate(eyeFlashLerp) * eyeFlashLightIntensity;
			if (eyeFlashLerp > 1f)
			{
				eyeFlash = false;
				eyeMaterial.SetFloat(eyeMaterialAmount, 0f);
				eyeFlashLight.gameObject.SetActive(value: false);
			}
		}
		if (triggered && !localSeen && !PlayerController.instance.playerAvatarScript.isDisabled)
		{
			if (seenCooldownTimer > 0f)
			{
				seenCooldownTimer -= Time.deltaTime;
			}
			else
			{
				Vector3 localCameraPosition = PlayerController.instance.playerAvatarScript.localCameraPosition;
				float num = Vector3.Distance(base.transform.position, localCameraPosition);
				if (num <= 10f && SemiFunc.OnScreen(base.transform.position, -0.15f, -0.15f))
				{
					Vector3 normalized = (localCameraPosition - base.transform.position).normalized;
					if (!Physics.Raycast(physGrabObject.centerPoint, normalized, out var _, num, LayerMask.GetMask("Default")))
					{
						localSeen = true;
						TutorialDirector.instance.playerSawHead = true;
						if (!serverSeen && SemiFunc.RunIsLevel())
						{
							if (SemiFunc.IsMultiplayer())
							{
								photonView.RPC("SeenSetRPC", RpcTarget.All, true);
							}
							else
							{
								SeenSetRPC(_toggle: true);
							}
							if (PlayerController.instance.deathSeenTimer <= 0f)
							{
								localSeenEffect = true;
								PlayerController.instance.deathSeenTimer = 30f;
								GameDirector.instance.CameraImpact.Shake(2f, 0.5f);
								GameDirector.instance.CameraShake.Shake(2f, 1f);
								AudioScare.instance.PlayCustom(seenSound, 0.3f, 60f);
								ValuableDiscover.instance.New(physGrabObject, ValuableDiscoverGraphic.State.Bad);
							}
						}
					}
				}
			}
		}
		if (localSeenEffect)
		{
			localSeenEffectTimer -= Time.deltaTime;
			CameraZoom.Instance.OverrideZoomSet(75f, 0.1f, 0.25f, 0.25f, base.gameObject, 150);
			PostProcessing.Instance.VignetteOverride(Color.black, 0.4f, 1f, 1f, 0.5f, 0.1f, base.gameObject);
			PostProcessing.Instance.SaturationOverride(-50f, 1f, 0.5f, 0.1f, base.gameObject);
			PostProcessing.Instance.ContrastOverride(5f, 1f, 0.5f, 0.1f, base.gameObject);
			GameDirector.instance.CameraImpact.Shake(10f * Time.deltaTime, 0.1f);
			GameDirector.instance.CameraShake.Shake(10f * Time.deltaTime, 1f);
			if (localSeenEffectTimer <= 0f)
			{
				localSeenEffect = false;
			}
		}
		if (triggered && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (roomVolumeCheck.CurrentRooms.Count <= 0)
			{
				outsideLevelTimer += Time.deltaTime;
				if (outsideLevelTimer >= 5f)
				{
					if (RoundDirector.instance.extractionPointActive)
					{
						physGrabObject.Teleport(RoundDirector.instance.extractionPointCurrent.safetySpawn.position, RoundDirector.instance.extractionPointCurrent.safetySpawn.rotation);
					}
					else
					{
						physGrabObject.Teleport(TruckSafetySpawnPoint.instance.transform.position, TruckSafetySpawnPoint.instance.transform.rotation);
					}
				}
			}
			else
			{
				outsideLevelTimer = 0f;
			}
		}
		if (tutorialPossible)
		{
			if (triggered && localSeen)
			{
				tutorialTimer -= Time.deltaTime;
				if (tutorialTimer <= 0f)
				{
					if (!RoundDirector.instance.allExtractionPointsCompleted && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialReviving, 1))
					{
						TutorialDirector.instance.ActivateTip("Reviving", 0.5f, _interrupt: false);
					}
					tutorialPossible = false;
				}
			}
			else
			{
				tutorialTimer = 5f;
			}
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (RoundDirector.instance.allExtractionPointsCompleted && triggered && !playerAvatar.finalHeal)
		{
			inTruck = roomVolumeCheck.inTruck;
			if (inTruck != inTruckPrevious)
			{
				if (GameManager.Multiplayer())
				{
					photonView.RPC("FlashEyeRPC", RpcTarget.All, inTruck);
				}
				else
				{
					FlashEyeRPC(inTruck);
				}
				inTruckPrevious = inTruck;
			}
		}
		else
		{
			inTruck = false;
			inTruckPrevious = false;
		}
		if (inTruck)
		{
			inTruckReviveTimer -= Time.deltaTime;
			if (inTruckReviveTimer <= 0f)
			{
				playerAvatar.Revive(_revivedByTruck: true);
			}
		}
		else
		{
			inTruckReviveTimer = 2f;
		}
	}

	private void UpdateColor()
	{
		if ((bool)headRenderer)
		{
			headRenderer.material = playerAvatar.playerHealth.bodyMaterial;
			headRenderer.material.SetFloat(Shader.PropertyToID("_ColorOverlayAmount"), 0f);
			Color color = playerAvatar.playerAvatarVisuals.color;
			physGrabObject.impactDetector.particles.gradient = new Gradient
			{
				colorKeys = new GradientColorKey[2]
				{
					new GradientColorKey(color, 0f),
					new GradientColorKey(color, 1f)
				}
			};
		}
	}

	public void Revive()
	{
		if (triggered && inExtractionPoint)
		{
			playerAvatar.Revive();
		}
	}

	public void Trigger()
	{
		seenCooldownTimer = seenCooldownTime;
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (playerAvatar.isLocal)
			{
				PlayerController.instance.col.enabled = false;
			}
			else
			{
				playerAvatar.playerAvatarCollision.Collider.enabled = false;
			}
			Collider[] array = playerAvatar.tumble.colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			physGrabObject.Teleport(playerAvatar.playerAvatarCollision.deathHeadPosition, playerAvatar.localCameraTransform.rotation);
			triggeredTimer = 0.1f;
		}
		UpdateColor();
		triggered = true;
		SetColliders(_enabled: true);
		if ((bool)smokeParticles)
		{
			smokeParticles.Play();
		}
		smokeParticleRateOverTimeCurrent = smokeParticleRateOverTimeDefault;
		smokeParticleRateOverDistanceCurrent = smokeParticleRateOverDistanceDefault;
	}

	public void Reset()
	{
		triggered = false;
		smokeParticleTimer = smokeParticleTime;
		localSeenEffectTimer = localSeenEffectTime;
		localSeen = false;
		localSeenEffect = false;
		SetColliders(_enabled: false);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.Teleport(new Vector3(0f, 3000f, 0f), Quaternion.identity);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SeenSetRPC", RpcTarget.All, false);
			}
			else
			{
				SeenSetRPC(_toggle: false);
			}
		}
	}

	private void SetColliders(bool _enabled)
	{
		Collider[] array = colliders;
		foreach (Collider collider in array)
		{
			if ((bool)collider)
			{
				collider.enabled = _enabled;
			}
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
				playerAvatar.playerDeathHead = this;
				break;
			}
		}
		StartCoroutine(SetupClient());
	}

	[PunRPC]
	public void FlashEyeRPC(bool _positive)
	{
		inExtractionPoint = _positive;
		if (_positive)
		{
			eyeMaterial.SetColor(eyeMaterialColor, eyeFlashPositiveColor);
			eyeFlashPositiveSound.Play(base.transform.position);
			eyeFlashLight.color = eyeFlashPositiveColor;
		}
		else
		{
			eyeMaterial.SetColor(eyeMaterialColor, eyeFlashNegativeColor);
			eyeFlashNegativeSound.Play(base.transform.position);
			eyeFlashLight.color = eyeFlashNegativeColor;
		}
		eyeFlash = true;
		eyeFlashLerp = 0f;
		eyeFlashLight.gameObject.SetActive(value: true);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.25f);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.5f);
	}

	[PunRPC]
	public void SeenSetRPC(bool _toggle)
	{
		serverSeen = _toggle;
	}
}
