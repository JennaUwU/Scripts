using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ChargingStation : MonoBehaviour, IPunObservable
{
	public static ChargingStation instance;

	private PhotonView photonView;

	private Transform chargeBar;

	internal float charge = 1f;

	private float chargeScale = 1f;

	private float chargeScaleTarget = 1f;

	internal int chargeInt;

	private int chargeSegments = 6;

	private float chargeRate = 0.05f;

	public AnimationCurve chargeCurve;

	private float chargeCurveTime;

	private Transform chargeArea;

	private float chargeAreaCheckTimer;

	private List<ItemBattery> itemsCharging = new List<ItemBattery>();

	private Transform lockedTransform;

	public GameObject meshObject;

	private Material chargingStationEmissionMaterial;

	private bool isCharging;

	private bool isChargingPrev;

	private Light light1;

	private Light light2;

	public Sound soundStart;

	public Sound soundStop;

	public Sound soundLoop;

	public Transform crystalCylinder;

	public List<Transform> crystals = new List<Transform>();

	public ParticleSystem lightParticle;

	public ParticleSystem fireflyParticles;

	public ParticleSystem bitsParticles;

	public Sound soundPowerCrystalBreak;

	private float crystalCooldown;

	public Item item;

	public GameObject subtleLight;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		chargeRate = 0.05f;
		foreach (Transform item in crystalCylinder)
		{
			crystals.Add(item);
		}
		chargingStationEmissionMaterial = meshObject.GetComponent<Renderer>().material;
		chargeBar = base.transform.Find("Charge");
		photonView = GetComponent<PhotonView>();
		chargeArea = base.transform.Find("Charge Area");
		lockedTransform = base.transform.Find("Locked");
		light1 = base.transform.Find("Light1").GetComponent<Light>();
		light2 = base.transform.Find("Light2").GetComponent<Light>();
		if (!SemiFunc.RunIsShop())
		{
			if ((bool)lockedTransform)
			{
				Object.Destroy(lockedTransform.gameObject);
			}
		}
		else
		{
			if ((bool)subtleLight)
			{
				Object.Destroy(subtleLight);
			}
			if ((bool)chargeArea)
			{
				Object.Destroy(chargeArea.gameObject);
			}
			if ((bool)chargeBar)
			{
				Object.Destroy(chargeBar.gameObject);
			}
			Object.Destroy(light1.gameObject);
			Object.Destroy(light2.gameObject);
		}
		charge = 0f;
		chargeScale = 0f;
		chargeScaleTarget = 0f;
		chargeBar.localScale = new Vector3(0f, 1f, 1f);
		chargeInt = SemiFunc.StatGetItemsPurchased("Item Power Crystal");
		if (chargeInt <= 0)
		{
			OutOfCrystalsShutdown();
		}
		int num = StatsManager.instance.runStats["chargingStationCharge"];
		if (chargeInt > num)
		{
			if (chargeInt > chargeSegments)
			{
				chargeInt = chargeSegments;
			}
			charge = (float)chargeInt * (1f / (float)chargeSegments);
			chargeScale = charge;
			chargeScaleTarget = charge;
			chargeBar.localScale = new Vector3(chargeScale, 1f, 1f);
			StatsManager.instance.runStats["chargingStationCharge"] = chargeInt;
		}
		else
		{
			int num2 = num;
			float chargingStationCharge = StatsManager.instance.chargingStationCharge;
			chargeInt = num2;
			if (chargeInt > chargeSegments)
			{
				chargeInt = chargeSegments;
			}
			charge = chargingStationCharge;
			if (charge > (float)chargeInt * (1f / (float)chargeSegments))
			{
				charge = (float)chargeInt * (1f / (float)chargeSegments);
			}
			chargeScale = (float)chargeInt * (1f / (float)chargeSegments);
			chargeScaleTarget = (float)chargeInt * (1f / (float)chargeSegments);
			chargeBar.localScale = new Vector3(chargeScale, 1f, 1f);
			StatsManager.instance.runStats["chargingStationCharge"] = chargeInt;
		}
		StartCoroutine(MissionText());
		while (crystals.Count > chargeInt)
		{
			Object.Destroy(crystals[0].gameObject);
			crystals.RemoveAt(0);
			if (crystals.Count == 0)
			{
				OutOfCrystalsShutdown();
				break;
			}
		}
		if (RunManager.instance.levelsCompleted < 1)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OutOfCrystalsShutdown()
	{
		chargingStationEmissionMaterial.SetColor("_EmissionColor", Color.black);
		light1.enabled = false;
		light2.enabled = false;
		Color color = new Color(0.1f, 0.1f, 0.2f);
		subtleLight.GetComponent<Light>().color = color;
	}

	public IEnumerator MissionText()
	{
		while (LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(2f);
		if (SemiFunc.RunIsLobby())
		{
			SemiFunc.UIFocusText("Enjoy the ride, recharge stuff and GEAR UP!", Color.white, AssetManager.instance.colorYellow);
		}
	}

	private void StopCharge()
	{
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("StopChargeRPC", RpcTarget.All);
		}
		else
		{
			StopChargeRPC();
		}
	}

	[PunRPC]
	public void StopChargeRPC()
	{
		soundStop.Play(base.transform.position);
		isCharging = false;
	}

	private void StartCharge()
	{
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("StartChargeRPC", RpcTarget.All);
		}
		else
		{
			StartChargeRPC();
		}
	}

	[PunRPC]
	public void StartChargeRPC()
	{
		soundStart.Play(base.transform.position);
		isCharging = true;
	}

	private void ChargeArea()
	{
		if (SemiFunc.RunIsShop())
		{
			return;
		}
		if (charge <= 0f)
		{
			if (isCharging)
			{
				isChargingPrev = isCharging;
				StopCharge();
				isCharging = false;
			}
			return;
		}
		chargeAreaCheckTimer += Time.deltaTime;
		if (chargeAreaCheckTimer > 0.5f)
		{
			Collider[] array = Physics.OverlapBox(chargeArea.position, chargeArea.localScale / 2f, chargeArea.localRotation, SemiFunc.LayerMaskGetPhysGrabObject());
			itemsCharging.Clear();
			Collider[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ItemBattery componentInParent = array2[i].GetComponentInParent<ItemBattery>();
				if ((bool)componentInParent && componentInParent.batteryLifeInt < 6 && !itemsCharging.Contains(componentInParent))
				{
					itemsCharging.Add(componentInParent);
				}
			}
			chargeAreaCheckTimer = 0f;
		}
		bool flag = false;
		foreach (ItemBattery item in itemsCharging)
		{
			if (item.batteryLifeInt < 6)
			{
				item.ChargeBattery(base.gameObject, 30f);
				charge -= chargeRate * Time.deltaTime;
				flag = true;
				if (!isCharging)
				{
					StartCharge();
					isChargingPrev = isCharging;
					isCharging = true;
				}
			}
		}
		if (!flag && isCharging)
		{
			isChargingPrev = isCharging;
			StopCharge();
			isCharging = false;
		}
	}

	private void ChargingEffects()
	{
		if (isCharging)
		{
			TutorialDirector.instance.playerUsedChargingStation = true;
			crystalCylinder.localRotation = Quaternion.Euler(90f, 0f, Mathf.PingPong(Time.time * 150f, 5f) - 2.5f);
			int num = 0;
			foreach (Transform crystal in crystals)
			{
				if ((bool)crystal)
				{
					num++;
					float value = 0.1f + Mathf.PingPong((Time.time + (float)num) * 5f, 1f);
					Color value2 = Color.yellow * Mathf.LinearToGammaSpace(value);
					crystal.GetComponent<Renderer>().material.SetColor("_EmissionColor", value2);
				}
			}
			crystalCooldown = 0f;
			return;
		}
		crystalCylinder.localRotation = Quaternion.Euler(90f, 0f, 0f);
		foreach (Transform crystal2 in crystals)
		{
			if ((bool)crystal2)
			{
				crystalCooldown += Time.deltaTime * 0.5f;
				float num2 = chargeCurve.Evaluate(crystalCooldown);
				float value3 = Mathf.Lerp(1f, 0.1f, num2);
				Color value4 = Color.yellow * Mathf.LinearToGammaSpace(value3);
				crystal2.GetComponent<Renderer>().material.SetColor("_EmissionColor", value4);
				crystalCylinder.localRotation = Quaternion.Euler(90f, 0f, (Mathf.PingPong(Time.time * 250f, 10f) - 5f) * (1f - num2));
			}
		}
	}

	private void Update()
	{
		if (SemiFunc.RunIsShop())
		{
			return;
		}
		soundLoop.PlayLoop(isCharging, 2f, 2f);
		AnimateChargeBar();
		ChargingEffects();
		int count = crystals.Count;
		if (isCharging && count > 0)
		{
			float value = 0.5f + Mathf.PingPong(Time.time * 5f, 0.5f);
			Color value2 = Color.yellow * Mathf.LinearToGammaSpace(value);
			chargingStationEmissionMaterial.SetColor("_EmissionColor", value2);
			if ((bool)light1 && (bool)light2)
			{
				light1.enabled = true;
				light2.enabled = true;
				light1.intensity = 0.5f + Mathf.PingPong(Time.time * 5f, 0.5f);
				light2.intensity = 0.5f + Mathf.PingPong(Time.time * 5f, 0.5f);
			}
		}
		else if ((bool)light1 && (bool)light2)
		{
			chargingStationEmissionMaterial.SetColor("_EmissionColor", Color.black);
			light1.enabled = false;
			light2.enabled = false;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && !RunManager.instance.restarting)
		{
			ChargeArea();
			float num = 1f / (float)chargeSegments;
			int num2 = Mathf.CeilToInt(charge / num);
			if (num2 > chargeSegments)
			{
				num2 = chargeSegments;
			}
			if (chargeInt != num2)
			{
				chargeInt = num2;
				UpdateChargeBar(chargeInt);
			}
		}
	}

	private void AnimateChargeBar()
	{
		if ((bool)chargeBar && chargeScale != chargeScaleTarget)
		{
			chargeCurveTime += Time.deltaTime;
			chargeScale = Mathf.Lerp(chargeScale, chargeScaleTarget, chargeCurve.Evaluate(chargeCurveTime));
			chargeBar.localScale = new Vector3(chargeScale, 1f, 1f);
		}
	}

	private void UpdateChargeBar(int segmentPassed)
	{
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpdateChargeBarRPC", RpcTarget.All, segmentPassed);
		}
		else
		{
			UpdateChargeBarRPC(segmentPassed);
		}
	}

	private void DestroyCrystal()
	{
		if (crystals.Count >= 1)
		{
			Vector3 position = crystals[0].position + crystals[0].up * 0.1f;
			lightParticle.transform.position = position;
			fireflyParticles.transform.position = position;
			bitsParticles.transform.position = position;
			lightParticle.Play();
			fireflyParticles.Play();
			bitsParticles.Play();
			soundPowerCrystalBreak.Play(position);
			Object.Destroy(crystals[0].gameObject);
			crystals.RemoveAt(0);
			if (crystals.Count == 0)
			{
				OutOfCrystalsShutdown();
			}
		}
	}

	[PunRPC]
	public void UpdateChargeBarRPC(int segmentPassed)
	{
		chargeCurveTime = 0f;
		float num = 1f / (float)chargeSegments;
		chargeScaleTarget = (float)segmentPassed * num;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			StatsManager.instance.SetItemPurchase(item, StatsManager.instance.GetItemPurchased(item) - 1);
		}
		DestroyCrystal();
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			stream.SendNext(isCharging);
		}
		else
		{
			isCharging = (bool)stream.ReceiveNext();
		}
	}
}
