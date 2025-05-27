using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class PunManager : MonoBehaviour
{
	internal PhotonView photonView;

	internal StatsManager statsManager;

	private ShopManager shopManager;

	private ItemManager itemManager;

	public static PunManager instance;

	private List<ExitGames.Client.Photon.Hashtable> syncData = new List<ExitGames.Client.Photon.Hashtable>();

	public PhotonLagSimulationGui lagSimulationGui;

	private int totalHaul;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		statsManager = StatsManager.instance;
		shopManager = ShopManager.instance;
		itemManager = ItemManager.instance;
		photonView = GetComponent<PhotonView>();
	}

	public void SetItemName(string name, ItemAttributes itemAttributes, int photonViewID)
	{
		if (photonViewID != -1 && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SetItemNameRPC", RpcTarget.All, name, photonViewID);
			}
			else
			{
				SetItemNameLOGIC(name, photonViewID, itemAttributes);
			}
		}
	}

	private void SetItemNameLOGIC(string name, int photonViewID, ItemAttributes _itemAttributes = null)
	{
		if (photonViewID == -1 && SemiFunc.IsMultiplayer())
		{
			return;
		}
		ItemAttributes itemAttributes = _itemAttributes;
		if (SemiFunc.IsMultiplayer())
		{
			itemAttributes = PhotonView.Find(photonViewID).GetComponent<ItemAttributes>();
		}
		if (_itemAttributes == null && !SemiFunc.IsMultiplayer())
		{
			return;
		}
		itemAttributes.instanceName = name;
		ItemBattery component = itemAttributes.GetComponent<ItemBattery>();
		if ((bool)component)
		{
			component.SetBatteryLife(statsManager.itemStatBattery[name]);
		}
		ItemEquippable component2 = itemAttributes.GetComponent<ItemEquippable>();
		if (!component2)
		{
			return;
		}
		int spot = 0;
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		int hashCode = name.GetHashCode();
		bool flag = false;
		PlayerAvatar playerAvatar = null;
		foreach (PlayerAvatar item in list)
		{
			string steamID = item.steamID;
			if (StatsManager.instance.playerInventorySpot1[steamID] == hashCode && StatsManager.instance.playerInventorySpot1Taken[steamID] == 0)
			{
				spot = 0;
				flag = true;
				playerAvatar = item;
				StatsManager.instance.playerInventorySpot1Taken[steamID] = 1;
				break;
			}
			if (StatsManager.instance.playerInventorySpot2[steamID] == hashCode && StatsManager.instance.playerInventorySpot2Taken[steamID] == 0)
			{
				spot = 1;
				flag = true;
				playerAvatar = item;
				StatsManager.instance.playerInventorySpot2Taken[steamID] = 1;
				break;
			}
			if (StatsManager.instance.playerInventorySpot3[steamID] == hashCode && StatsManager.instance.playerInventorySpot3Taken[steamID] == 0)
			{
				spot = 2;
				flag = true;
				playerAvatar = item;
				StatsManager.instance.playerInventorySpot3Taken[steamID] = 1;
				break;
			}
		}
		if (flag)
		{
			int requestingPlayerId = -1;
			if (SemiFunc.IsMultiplayer())
			{
				requestingPlayerId = playerAvatar.photonView.ViewID;
			}
			component2.RequestEquip(spot, requestingPlayerId);
		}
	}

	public void CrownPlayerSync(string _steamID)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.IsMultiplayer())
		{
			photonView.RPC("CrownPlayerRPC", RpcTarget.AllBuffered, _steamID);
		}
	}

	[PunRPC]
	public void CrownPlayerRPC(string _steamID)
	{
		SessionManager.instance.crownedPlayerSteamID = _steamID;
		PlayerCrownSet component = Object.Instantiate(SessionManager.instance.crownPrefab).GetComponent<PlayerCrownSet>();
		component.crownOwnerFetched = true;
		component.crownOwnerSteamID = _steamID;
		StatsManager.instance.UpdateCrown(_steamID);
	}

	[PunRPC]
	public void SetItemNameRPC(string name, int photonViewID)
	{
		SetItemNameLOGIC(name, photonViewID);
	}

	public void ShopUpdateCost()
	{
		int num = 0;
		List<ItemAttributes> list = new List<ItemAttributes>();
		foreach (ItemAttributes shopping in ShopManager.instance.shoppingList)
		{
			if ((bool)shopping)
			{
				shopping.roomVolumeCheck.CheckSet();
				if (!shopping.roomVolumeCheck.inExtractionPoint)
				{
					list.Add(shopping);
				}
				else
				{
					num += shopping.value;
				}
			}
			else
			{
				list.Add(shopping);
			}
		}
		foreach (ItemAttributes item in list)
		{
			ShopManager.instance.shoppingList.Remove(item);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateShoppingCostRPC", RpcTarget.All, num);
			}
			else
			{
				UpdateShoppingCostRPC(num);
			}
		}
	}

	private void Update()
	{
		if (SemiFunc.FPSImpulse5() && SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient() && totalHaul != RoundDirector.instance.totalHaul)
		{
			totalHaul = RoundDirector.instance.totalHaul;
			photonView.RPC("SyncHaul", RpcTarget.Others, totalHaul);
		}
	}

	[PunRPC]
	public void SyncHaul(int value)
	{
		RoundDirector.instance.totalHaul = value;
	}

	[PunRPC]
	public void UpdateShoppingCostRPC(int value)
	{
		ShopManager.instance.totalCost = value;
	}

	public void ShopPopulateItemVolumes()
	{
		if (SemiFunc.IsNotMasterClient())
		{
			return;
		}
		int spawnCount = 0;
		int spawnCount2 = 0;
		int spawnCount3 = 0;
		int spawnCount4 = 0;
		foreach (KeyValuePair<SemiFunc.itemSecretShopType, List<ItemVolume>> secretItemVolume in ShopManager.instance.secretItemVolumes)
		{
			List<ItemVolume> value = secretItemVolume.Value;
			foreach (ItemVolume item in value)
			{
				if (ShopManager.instance.potentialSecretItems.ContainsKey(secretItemVolume.Key))
				{
					_ = ShopManager.instance.potentialSecretItems[secretItemVolume.Key];
					if (Random.Range(0, 3) == 0 && (bool)item)
					{
						SpawnShopItem(item, ShopManager.instance.potentialSecretItems[secretItemVolume.Key], ref spawnCount, isSecret: true);
					}
				}
			}
			foreach (ItemVolume item2 in value)
			{
				if ((bool)item2)
				{
					Object.Destroy(item2.gameObject);
				}
			}
		}
		foreach (ItemVolume itemVolume in shopManager.itemVolumes)
		{
			if (shopManager.potentialItems.Count == 0 && shopManager.potentialItemConsumables.Count == 0)
			{
				break;
			}
			if ((spawnCount >= shopManager.itemSpawnTargetAmount || !SpawnShopItem(itemVolume, shopManager.potentialItems, ref spawnCount)) && (spawnCount2 >= shopManager.itemConsumablesAmount || !SpawnShopItem(itemVolume, shopManager.potentialItemConsumables, ref spawnCount2)))
			{
				if (spawnCount3 < shopManager.itemUpgradesAmount)
				{
					SpawnShopItem(itemVolume, shopManager.potentialItemUpgrades, ref spawnCount3);
				}
				if (spawnCount4 < shopManager.itemHealthPacksAmount)
				{
					SpawnShopItem(itemVolume, shopManager.potentialItemHealthPacks, ref spawnCount4);
				}
			}
		}
		foreach (ItemVolume itemVolume2 in shopManager.itemVolumes)
		{
			Object.Destroy(itemVolume2.gameObject);
		}
	}

	private bool SpawnShopItem(ItemVolume itemVolume, List<Item> itemList, ref int spawnCount, bool isSecret = false)
	{
		for (int num = itemList.Count - 1; num >= 0; num--)
		{
			Item item = itemList[num];
			if (item.itemVolume == itemVolume.itemVolume)
			{
				ShopManager.instance.itemRotateHelper.transform.parent = itemVolume.transform;
				ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
				Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
				ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
				string prefabName = "Items/" + item.prefab.name;
				if (SemiFunc.IsMultiplayer())
				{
					PhotonNetwork.InstantiateRoomObject(prefabName, itemVolume.transform.position, rotation, 0);
				}
				else
				{
					Object.Instantiate(item.prefab, itemVolume.transform.position, rotation);
				}
				itemList.RemoveAt(num);
				if (!isSecret)
				{
					spawnCount++;
				}
				return true;
			}
		}
		return false;
	}

	public void TruckPopulateItemVolumes()
	{
		ItemManager.instance.spawnedItems.Clear();
		if (SemiFunc.IsNotMasterClient())
		{
			return;
		}
		List<ItemVolume> list = new List<ItemVolume>(itemManager.itemVolumes);
		List<Item> list2 = new List<Item>(itemManager.purchasedItems);
		while (list.Count > 0 && list2.Count > 0)
		{
			bool flag = false;
			for (int i = 0; i < list2.Count; i++)
			{
				Item item = list2[i];
				ItemVolume itemVolume = list.Find((ItemVolume v) => v.itemVolume == item.itemVolume);
				if ((bool)itemVolume)
				{
					SpawnItem(item, itemVolume);
					list.Remove(itemVolume);
					list2.RemoveAt(i);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		foreach (ItemVolume itemVolume2 in itemManager.itemVolumes)
		{
			Object.Destroy(itemVolume2.gameObject);
		}
	}

	private void SpawnItem(Item item, ItemVolume volume)
	{
		ShopManager.instance.itemRotateHelper.transform.parent = volume.transform;
		ShopManager.instance.itemRotateHelper.transform.localRotation = item.spawnRotationOffset;
		Quaternion rotation = ShopManager.instance.itemRotateHelper.transform.rotation;
		ShopManager.instance.itemRotateHelper.transform.parent = ShopManager.instance.transform;
		if (SemiFunc.IsMasterClient())
		{
			PhotonNetwork.InstantiateRoomObject("Items/" + item.prefab.name, volume.transform.position, rotation, 0);
		}
		else if (!SemiFunc.IsMultiplayer())
		{
			Object.Instantiate(item.prefab, volume.transform.position, rotation);
		}
	}

	public void AddingItem(string itemName, int index, int photonViewID, ItemAttributes itemAttributes)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("AddingItemRPC", RpcTarget.All, itemName, index, photonViewID);
		}
		else
		{
			AddingItemLOGIC(itemName, index, photonViewID, itemAttributes);
		}
	}

	private void AddingItemLOGIC(string itemName, int index, int photonViewID, ItemAttributes itemAttributes = null)
	{
		if (!StatsManager.instance.item.ContainsKey(itemName))
		{
			StatsManager.instance.item.Add(itemName, index);
			StatsManager.instance.itemStatBattery.Add(itemName, 100);
			StatsManager.instance.takenItemNames.Add(itemName);
		}
		else
		{
			Debug.LogWarning("Item " + itemName + " already exists in the dictionary");
		}
		SetItemNameLOGIC(itemName, photonViewID, itemAttributes);
	}

	[PunRPC]
	public void AddingItemRPC(string itemName, int index, int photonViewID)
	{
		AddingItemLOGIC(itemName, index, photonViewID);
	}

	public void UpdateStat(string dictionaryName, string key, int value)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("UpdateStatRPC", RpcTarget.All, dictionaryName, key, value);
		}
		else
		{
			UpdateStatRPC(dictionaryName, key, value);
		}
	}

	[PunRPC]
	public void UpdateStatRPC(string dictionaryName, string key, int value)
	{
		StatsManager.instance.DictionaryUpdateValue(dictionaryName, key, value);
	}

	public int SetRunStatSet(string statName, int value)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				statsManager.runStats[statName] = value;
				photonView.RPC("SetRunStatRPC", RpcTarget.Others, statName, value);
			}
			else
			{
				statsManager.runStats[statName] = value;
			}
		}
		return statsManager.runStats[statName];
	}

	[PunRPC]
	public void SetRunStatRPC(string statName, int value)
	{
		statsManager.runStats[statName] = value;
	}

	public int UpgradeItemBattery(string itemName)
	{
		statsManager.itemBatteryUpgrades[itemName]++;
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradeItemBatteryRPC", RpcTarget.Others, itemName, statsManager.itemBatteryUpgrades[itemName]);
		}
		return statsManager.itemBatteryUpgrades[itemName];
	}

	[PunRPC]
	public void UpgradeItemBatteryRPC(string itemName, int value)
	{
		statsManager.itemBatteryUpgrades[itemName] = value;
	}

	public int UpgradePlayerHealth(string playerName)
	{
		statsManager.playerUpgradeHealth[playerName]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateHealthRightAway(playerName);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerHealthRPC", RpcTarget.Others, playerName, statsManager.playerUpgradeHealth[playerName]);
		}
		return statsManager.playerUpgradeHealth[playerName];
	}

	[PunRPC]
	public void UpgradePlayerHealthRPC(string playerName, int value)
	{
		statsManager.playerUpgradeHealth[playerName] = value;
		UpdateHealthRightAway(playerName);
	}

	private void UpdateHealthRightAway(string playerName)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(playerName);
		if (playerAvatar == SemiFunc.PlayerAvatarLocal())
		{
			playerAvatar.playerHealth.maxHealth += 20;
			playerAvatar.playerHealth.Heal(20, effect: false);
		}
	}

	public int UpgradePlayerEnergy(string _steamID)
	{
		statsManager.playerUpgradeStamina[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateEnergyRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerEnergyRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeStamina[_steamID]);
		}
		return statsManager.playerUpgradeStamina[_steamID];
	}

	[PunRPC]
	public void UpgradePlayerEnergyRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeStamina[_steamID] = value;
		UpdateEnergyRightAway(_steamID);
	}

	private void UpdateEnergyRightAway(string _steamID)
	{
		if (SemiFunc.PlayerAvatarGetFromSteamID(_steamID) == SemiFunc.PlayerAvatarLocal())
		{
			PlayerController.instance.EnergyStart += 10f;
			PlayerController.instance.EnergyCurrent = PlayerController.instance.EnergyStart;
		}
	}

	public int UpgradePlayerExtraJump(string _steamID)
	{
		statsManager.playerUpgradeExtraJump[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateExtraJumpRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerExtraJumpRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeExtraJump[_steamID]);
		}
		return statsManager.playerUpgradeExtraJump[_steamID];
	}

	[PunRPC]
	public void UpgradePlayerExtraJumpRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeExtraJump[_steamID] = value;
		UpdateExtraJumpRightAway(_steamID);
	}

	private void UpdateExtraJumpRightAway(string _steamID)
	{
		if (SemiFunc.PlayerAvatarGetFromSteamID(_steamID) == SemiFunc.PlayerAvatarLocal())
		{
			PlayerController.instance.JumpExtra++;
		}
	}

	public int UpgradeMapPlayerCount(string _steamID)
	{
		statsManager.playerUpgradeMapPlayerCount[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateMapPlayerCountRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradeMapPlayerCountRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeMapPlayerCount[_steamID]);
		}
		return statsManager.playerUpgradeMapPlayerCount[_steamID];
	}

	[PunRPC]
	public void UpgradeMapPlayerCountRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeMapPlayerCount[_steamID] = value;
		UpdateMapPlayerCountRightAway(_steamID);
	}

	private void UpdateMapPlayerCountRightAway(string _steamID)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if (playerAvatar == SemiFunc.PlayerAvatarLocal())
		{
			playerAvatar.upgradeMapPlayerCount++;
		}
	}

	public int UpgradePlayerTumbleLaunch(string _steamID)
	{
		statsManager.playerUpgradeLaunch[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateTumbleLaunchRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerTumbleLaunchRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeLaunch[_steamID]);
		}
		return statsManager.playerUpgradeLaunch[_steamID];
	}

	[PunRPC]
	public void UpgradePlayerTumbleLaunchRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeLaunch[_steamID] = value;
		UpdateTumbleLaunchRightAway(_steamID);
	}

	private void UpdateTumbleLaunchRightAway(string _steamID)
	{
		SemiFunc.PlayerAvatarGetFromSteamID(_steamID).tumble.tumbleLaunch++;
	}

	public int UpgradePlayerSprintSpeed(string _steamID)
	{
		statsManager.playerUpgradeSpeed[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateSprintSpeedRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerSprintSpeedRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeSpeed[_steamID]);
		}
		return statsManager.playerUpgradeSpeed[_steamID];
	}

	[PunRPC]
	public void UpgradePlayerSprintSpeedRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeSpeed[_steamID] = value;
		UpdateSprintSpeedRightAway(_steamID);
	}

	private void UpdateSprintSpeedRightAway(string _steamID)
	{
		if (SemiFunc.PlayerAvatarGetFromSteamID(_steamID) == SemiFunc.PlayerAvatarLocal())
		{
			PlayerController.instance.SprintSpeed += 1f;
			PlayerController.instance.SprintSpeedUpgrades += 1f;
		}
	}

	public int UpgradePlayerGrabStrength(string _steamID)
	{
		statsManager.playerUpgradeStrength[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateGrabStrengthRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerGrabStrengthRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeStrength[_steamID]);
		}
		return statsManager.playerUpgradeStrength[_steamID];
	}

	[PunRPC]
	public void UpgradePlayerGrabStrengthRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeStrength[_steamID] = value;
		UpdateGrabStrengthRightAway(_steamID);
	}

	private void UpdateGrabStrengthRightAway(string _steamID)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.physGrabber.grabStrength += 0.2f;
		}
	}

	public int UpgradePlayerThrowStrength(string _steamID)
	{
		statsManager.playerUpgradeThrow[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateThrowStrengthRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerThrowStrengthRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeThrow[_steamID]);
		}
		return statsManager.playerUpgradeThrow[_steamID];
	}

	[PunRPC]
	public void UpgradePlayerThrowStrengthRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeThrow[_steamID] = value;
		UpdateGrabStrengthRightAway(_steamID);
	}

	private void UpdateThrowStrengthRightAway(string _steamID)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.physGrabber.throwStrength += 0.3f;
		}
	}

	public int UpgradePlayerGrabRange(string _steamID)
	{
		statsManager.playerUpgradeRange[_steamID]++;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateGrabRangeRightAway(_steamID);
		}
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("UpgradePlayerGrabRangeRPC", RpcTarget.Others, _steamID, statsManager.playerUpgradeRange[_steamID]);
		}
		return statsManager.playerUpgradeRange[_steamID];
	}

	[PunRPC]
	public void UpgradePlayerGrabRangeRPC(string _steamID, int value)
	{
		statsManager.playerUpgradeRange[_steamID] = value;
		UpdateGrabRangeRightAway(_steamID);
	}

	private void UpdateGrabRangeRightAway(string _steamID)
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			playerAvatar.physGrabber.grabRange += 1f;
		}
	}

	public void SyncAllDictionaries()
	{
		StatsManager.instance.statsSynced = true;
		if (!SemiFunc.IsMultiplayer() || !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		syncData.Clear();
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in statsManager.dictionaryOfDictionaries)
		{
			string key = dictionaryOfDictionary.Key;
			hashtable.Add(key, ConvertToHashtable(dictionaryOfDictionary.Value));
			num++;
			num2++;
			num3++;
			list.Add(key);
			if (num > 3 || num2 == statsManager.dictionaryOfDictionaries.Count)
			{
				syncData.Add(hashtable);
				list.Clear();
				num = 0;
			}
		}
		for (int i = 0; i < syncData.Count; i++)
		{
			bool flag = i == syncData.Count - 1;
			ExitGames.Client.Photon.Hashtable hashtable2 = syncData[i];
			photonView.RPC("ReceiveSyncData", RpcTarget.Others, hashtable2, flag);
		}
		syncData.Clear();
	}

	private ExitGames.Client.Photon.Hashtable ConvertToHashtable(Dictionary<string, int> dictionary)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			hashtable.Add(item.Key, item.Value);
		}
		return hashtable;
	}

	private Dictionary<K, V> ConvertToDictionary<K, V>(ExitGames.Client.Photon.Hashtable hashtable)
	{
		Dictionary<K, V> dictionary = new Dictionary<K, V>();
		foreach (DictionaryEntry item in hashtable)
		{
			dictionary.Add((K)item.Key, (V)item.Value);
		}
		return dictionary;
	}

	[PunRPC]
	public void ReceiveSyncData(ExitGames.Client.Photon.Hashtable data, bool finalChunk)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Dictionary<string, int>> dictionaryOfDictionary in statsManager.dictionaryOfDictionaries)
		{
			string key = dictionaryOfDictionary.Key;
			if (data.ContainsKey(key))
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			Dictionary<string, int> dictionary = statsManager.dictionaryOfDictionaries[item];
			foreach (DictionaryEntry item2 in (ExitGames.Client.Photon.Hashtable)data[item])
			{
				string key2 = (string)item2.Key;
				int value = (int)item2.Value;
				dictionary[key2] = value;
			}
		}
		if (finalChunk)
		{
			StatsManager.instance.statsSynced = true;
		}
	}
}
