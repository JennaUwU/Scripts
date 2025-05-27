using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemBattery : MonoBehaviour
{
	public bool isUnchargable;

	public Transform batteryTransform;

	private Camera mainCamera;

	public float upOffset = 0.5f;

	[HideInInspector]
	public bool batteryActive;

	[HideInInspector]
	public float batteryLife = 100f;

	internal int batteryLifeInt = 6;

	private Renderer itemBatteryMaterial;

	private float batteryOutBlinkTimer;

	private PhotonView photonView;

	[HideInInspector]
	public Color batteryColor;

	private float chargeTimer;

	private float chargeRate;

	private List<GameObject> chargerList = new List<GameObject>();

	internal bool isCharging;

	private float chargingBlinkTimer;

	private bool chargingBlink;

	private bool lowBatteryBeep;

	private ItemAttributes itemAttributes;

	private float showTimer;

	private bool showBattery;

	public bool autoDrain = true;

	private ItemEquippable itemEquippable;

	public bool onlyShowWhenItemToggleIsOn;

	public float batteryDrainRate = 1f;

	private float drainRate;

	private float drainTimer;

	private bool tutorialCheck;

	private PhysGrabObject physGrabObject;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		itemAttributes = GetComponent<ItemAttributes>();
		if (!itemAttributes)
		{
			Debug.LogWarning("ItemBattery.cs: No ItemAttributes found on " + base.gameObject.name);
		}
	}

	private void Start()
	{
		itemEquippable = GetComponent<ItemEquippable>();
		mainCamera = Camera.main;
		itemBatteryMaterial = batteryTransform.GetComponentInChildren<Renderer>();
		itemBatteryMaterial.material.SetColor("_Color", batteryColor);
		physGrabObject = GetComponentInChildren<PhysGrabObject>();
		if (SemiFunc.RunIsLevel() && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialChargingStation, 1))
		{
			tutorialCheck = true;
		}
	}

	private IEnumerator BatteryInit()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.2f);
		}
		while (GetComponent<ItemAttributes>().instanceName == null)
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (SemiFunc.RunIsArena())
		{
			StatsManager.instance.SetBatteryLevel(itemAttributes.instanceName, 100);
		}
		batteryLife = StatsManager.instance.GetBatteryLevel(itemAttributes.instanceName);
		if (batteryLife > 0f)
		{
			batteryLifeInt = (int)Mathf.Round(batteryLife / 16.6f);
			batteryColor = itemAttributes.colorPreset.GetColorLight();
		}
		else
		{
			batteryLife = 0f;
			batteryLifeInt = 0;
			batteryColor = itemAttributes.colorPreset.GetColorLight();
		}
		BatteryFullPercentChange(batteryLifeInt);
	}

	public void SetBatteryLife(int _batteryLife)
	{
		if (batteryLife > 0f)
		{
			batteryLife = _batteryLife;
			batteryLifeInt = (int)Mathf.Round(batteryLife / 16.6f);
		}
		else
		{
			batteryLife = 0f;
			batteryLifeInt = 0;
		}
		batteryColor = itemAttributes.colorPreset.GetColorLight();
		BatteryFullPercentChange(batteryLifeInt);
	}

	public void OverrideBatteryShow(float time = 0.1f)
	{
		showTimer = time;
	}

	public void ChargeBattery(GameObject chargerObject, float chargeAmount)
	{
		if (!chargerList.Contains(chargerObject))
		{
			chargerList.Add(chargerObject);
			chargeRate += chargeAmount;
		}
		chargeTimer = 0.1f;
	}

	private void FixedUpdate()
	{
		if (showTimer > 0f)
		{
			showTimer -= Time.fixedDeltaTime;
			showBattery = true;
		}
		else
		{
			showBattery = false;
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (chargeTimer > 0f && batteryLife < 99f)
		{
			batteryLife = Mathf.Clamp(batteryLife + chargeRate * Time.fixedDeltaTime, 0f, 100f);
			if (!isCharging)
			{
				BatteryChargeToggle(toggle: true);
			}
			chargeTimer -= Time.fixedDeltaTime;
		}
		else if (chargeRate != 0f)
		{
			chargeRate = 0f;
			chargeTimer = 0f;
			chargerList.Clear();
			BatteryChargeToggle(toggle: false);
		}
		if (drainTimer > 0f && batteryLife > 0f)
		{
			batteryLife = Mathf.Clamp(batteryLife - drainRate * Time.fixedDeltaTime, 0f, 100f);
			drainTimer -= Time.fixedDeltaTime;
		}
		else if (drainRate != 0f)
		{
			drainRate = 0f;
			drainTimer = 0f;
		}
	}

	private void Update()
	{
		if (itemAttributes.shopItem && SemiFunc.IsMasterClientOrSingleplayer())
		{
			batteryLife = 100f;
		}
		BatteryLookAt();
		BatteryChargingVisuals();
		if (SemiFunc.RunIsLobby() && batteryLifeInt < 6)
		{
			OverrideBatteryShow();
		}
		if (showBattery && !itemBatteryMaterial.gameObject.activeSelf)
		{
			itemBatteryMaterial.gameObject.SetActive(value: true);
			BatteryOffsetTexture(batteryLifeInt);
		}
		if (tutorialCheck && batteryLife <= 0f && SemiFunc.FPSImpulse15() && physGrabObject.playerGrabbing.Count > 0)
		{
			foreach (PhysGrabber item in physGrabObject.playerGrabbing)
			{
				if (item.isLocal && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialChargingStation, 1))
				{
					TutorialDirector.instance.ActivateTip("Charging Station", 2f, _interrupt: false);
					tutorialCheck = false;
				}
			}
		}
		if (batteryActive)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer() && autoDrain && !itemEquippable.isEquipped)
			{
				batteryLife -= batteryDrainRate * Time.deltaTime;
			}
			if (batteryLifeInt <= 1)
			{
				if (batteryLifeInt == 1)
				{
					batteryOutBlinkTimer += Time.deltaTime;
				}
				else
				{
					batteryOutBlinkTimer += 5f * Time.deltaTime;
				}
				if (batteryOutBlinkTimer >= 1f)
				{
					if (!lowBatteryBeep)
					{
						itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0f, 0.278f));
						if (batteryLifeInt < 1)
						{
							AssetManager.instance.batteryLowBeep.Play(base.transform.position);
							itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0f, 0.044f));
						}
						lowBatteryBeep = true;
					}
				}
				else if (lowBatteryBeep)
				{
					itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0.375f, 0.278f));
					lowBatteryBeep = false;
				}
				if (batteryOutBlinkTimer >= 2f)
				{
					batteryOutBlinkTimer = 0f;
				}
			}
			if (!itemBatteryMaterial.gameObject.activeSelf)
			{
				itemBatteryMaterial.gameObject.SetActive(value: true);
				BatteryOffsetTexture(batteryLifeInt);
			}
		}
		else if (!showBattery && itemBatteryMaterial.gameObject.activeSelf && !isCharging)
		{
			itemBatteryMaterial.gameObject.SetActive(value: false);
			BatteryOffsetTexture(batteryLifeInt);
		}
		if (GameManager.instance.gameMode == 0 || (GameManager.instance.gameMode == 1 && PhotonNetwork.IsMasterClient))
		{
			if (batteryLifeInt == 0 && batteryLife >= 17f)
			{
				BatteryFullPercentChange(1, charge: true);
			}
			else if (batteryLifeInt == 1 && batteryLife >= 34f)
			{
				BatteryFullPercentChange(2, charge: true);
			}
			else if (batteryLifeInt == 2 && batteryLife >= 50f)
			{
				BatteryFullPercentChange(3, charge: true);
			}
			else if (batteryLifeInt == 3 && batteryLife >= 67f)
			{
				BatteryFullPercentChange(4, charge: true);
			}
			else if (batteryLifeInt == 4 && batteryLife >= 84f)
			{
				BatteryFullPercentChange(5, charge: true);
			}
			else if (batteryLifeInt == 5 && batteryLife >= 99f)
			{
				BatteryFullPercentChange(6, charge: true);
			}
			if (batteryLifeInt == 6 && batteryLife <= 84f)
			{
				BatteryFullPercentChange(5);
			}
			else if (batteryLifeInt == 5 && batteryLife <= 67f)
			{
				BatteryFullPercentChange(4);
			}
			else if (batteryLifeInt == 4 && batteryLife <= 50f)
			{
				BatteryFullPercentChange(3);
			}
			else if (batteryLifeInt == 3 && batteryLife <= 34f)
			{
				BatteryFullPercentChange(2);
			}
			else if (batteryLifeInt == 2 && batteryLife <= 17f)
			{
				BatteryFullPercentChange(1);
			}
			else if (batteryLifeInt == 1 && batteryLife <= 0f)
			{
				BatteryFullPercentChange(0);
			}
		}
	}

	public void RemoveFullBar(int _bars)
	{
		if (!SemiFunc.RunIsShop() && batteryLifeInt > 0)
		{
			batteryLifeInt -= _bars;
			if (batteryLifeInt <= 0)
			{
				batteryLifeInt = 0;
				batteryLife = 0f;
			}
			else
			{
				batteryLife = (float)batteryLifeInt * 16.6f;
			}
			BatteryFullPercentChange(batteryLifeInt);
		}
	}

	public void BatteryToggle(bool toggle)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				photonView.RPC("BatteryToggleRPC", RpcTarget.All, toggle);
			}
		}
		else
		{
			BatteryToggleRPC(toggle);
		}
	}

	[PunRPC]
	public void BatteryToggleRPC(bool toggle)
	{
		batteryActive = toggle;
	}

	private void BatteryLookAt()
	{
		if (showBattery || batteryActive || isCharging)
		{
			batteryTransform.LookAt(mainCamera.transform);
			float num = Vector3.Distance(batteryTransform.position, mainCamera.transform.position);
			batteryTransform.localScale = Vector3.one * num * 0.8f;
			if (batteryTransform.localScale.x > 3f)
			{
				batteryTransform.localScale = Vector3.one * 3f;
			}
			batteryTransform.Rotate(0f, 180f, 0f);
			batteryTransform.position = base.transform.position + Vector3.up * upOffset;
		}
	}

	private void BatteryChargingVisuals()
	{
		if (!isCharging)
		{
			return;
		}
		if (!itemBatteryMaterial.gameObject.activeSelf)
		{
			itemBatteryMaterial.gameObject.SetActive(value: true);
		}
		chargingBlinkTimer += Time.deltaTime;
		if (chargingBlinkTimer > 0.5f)
		{
			chargingBlink = !chargingBlink;
			if (chargingBlink)
			{
				BatteryOffsetTexture(batteryLifeInt + 1);
			}
			else
			{
				BatteryOffsetTexture(batteryLifeInt);
			}
			chargingBlinkTimer = 0f;
		}
	}

	private void BatteryChargeToggle(bool toggle)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				photonView.RPC("BatteryChargeStartRPC", RpcTarget.All, toggle);
			}
		}
		else
		{
			BatteryChargeStartRPC(toggle);
		}
	}

	[PunRPC]
	private void BatteryChargeStartRPC(bool toggle)
	{
		isCharging = toggle;
		BatteryOffsetTexture(batteryLifeInt);
	}

	private void BatteryOffsetTexture(int batteryLevel)
	{
		if (!itemBatteryMaterial)
		{
			return;
		}
		Color red = batteryColor;
		switch (batteryLevel)
		{
		case 6:
			itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0f, 0.745f));
			itemBatteryMaterial.material.SetColor("_Color", red);
			break;
		case 5:
			itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0.375f, 0.745f));
			itemBatteryMaterial.material.SetColor("_Color", red);
			break;
		case 4:
			itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0f, 0.512f));
			itemBatteryMaterial.material.SetColor("_Color", red);
			break;
		case 3:
			itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0.375f, 0.512f));
			itemBatteryMaterial.material.SetColor("_Color", red);
			break;
		case 2:
			itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0f, 0.278f));
			itemBatteryMaterial.material.SetColor("_Color", red);
			break;
		case 1:
			itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0.375f, 0.278f));
			if (!isCharging)
			{
				red = Color.red;
			}
			itemBatteryMaterial.material.SetColor("_Color", red);
			break;
		case 0:
			itemBatteryMaterial.material.SetTextureOffset("_MainTex", new Vector2(0f, 0.044f));
			if (!isCharging)
			{
				red = Color.red;
			}
			itemBatteryMaterial.material.SetColor("_Color", red);
			break;
		}
	}

	private void BatteryFullPercentChangeLogic(int batteryLevel, bool charge)
	{
		if (batteryLifeInt > batteryLevel && batteryLevel == 1 && batteryActive)
		{
			AssetManager.instance.batteryLowWarning.Play(base.transform.position);
		}
		batteryLifeInt = batteryLevel;
		if (batteryLifeInt != 0)
		{
			batteryLife = (float)batteryLifeInt * 16.6f;
		}
		else
		{
			batteryLife = 0f;
		}
		SemiFunc.StatSetBattery(itemAttributes.instanceName, (int)batteryLife);
		BatteryOffsetTexture(batteryLifeInt);
		if (batteryActive || charge)
		{
			if (charge)
			{
				AssetManager.instance.batteryChargeSound.Play(base.transform.position);
			}
			else
			{
				AssetManager.instance.batteryDrainSound.Play(base.transform.position);
			}
		}
	}

	private void BatteryFullPercentChange(int batteryLifeInt, bool charge = false)
	{
		if (GameManager.instance.gameMode == 0)
		{
			BatteryFullPercentChangeLogic(batteryLifeInt, charge);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("BatteryFullPercentChangeRPC", RpcTarget.All, batteryLifeInt, charge);
		}
	}

	[PunRPC]
	private void BatteryFullPercentChangeRPC(int batteryLifeInt, bool charge)
	{
		BatteryFullPercentChangeLogic(batteryLifeInt, charge);
	}

	public void Drain(float amount)
	{
		drainRate = amount;
		drainTimer = 0.1f;
	}
}
