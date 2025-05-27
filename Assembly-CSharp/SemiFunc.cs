using System;
using System.Collections.Generic;
using System.Globalization;
using Photon.Pun;
using Steamworks;
using UnityEngine;
using UnityEngine.AI;

public static class SemiFunc
{
	public enum emojiIcon
	{
		drone_heal = 0,
		drone_zero_gravity = 1,
		drone_indestructible = 2,
		drone_feather = 3,
		drone_torque = 4,
		drone_battery = 5,
		orb_heal = 6,
		orb_zero_gravity = 7,
		orb_indestructible = 8,
		orb_feather = 9,
		orb_torque = 10,
		orb_battery = 11,
		orb_magnet = 12,
		grenade_explosive = 13,
		grenade_stun = 14,
		weapon_baseball_bat = 15,
		weapon_sledgehammer = 16,
		weapon_frying_pan = 17,
		weapon_sword = 18,
		weapon_inflatable_hammer = 19,
		item_health_pack_S = 20,
		item_health_pack_M = 21,
		item_health_pack_L = 22,
		item_gun_handgun = 23,
		item_gun_shotgun = 24,
		item_gun_tranq = 25,
		item_valuable_tracker = 26,
		item_extraction_tracker = 27,
		item_grenade_human = 28,
		item_grenade_duct_taped = 29,
		item_rubber_duck = 30,
		item_mine_explosive = 31,
		item_grenade_shockwave = 32,
		item_mine_shockwave = 33,
		item_mine_stun = 34
	}

	public enum itemVolume
	{
		small = 0,
		medium = 1,
		large = 2,
		large_wide = 3,
		power_crystal = 4,
		large_high = 5,
		upgrade = 6,
		healthPack = 7,
		large_plus = 8
	}

	public enum itemType
	{
		drone = 0,
		orb = 1,
		cart = 2,
		item_upgrade = 3,
		player_upgrade = 4,
		power_crystal = 5,
		grenade = 6,
		melee = 7,
		healthPack = 8,
		gun = 9,
		tracker = 10,
		mine = 11,
		pocket_cart = 12
	}

	public enum itemSecretShopType
	{
		none = 0,
		shop_attic = 1
	}

	public enum User
	{
		Walter = 0,
		Axel = 1,
		Robin = 2,
		Jannek = 3,
		Ruben = 4,
		Builder = 5
	}

	public static void EnemyCartJumpReset(Enemy enemy)
	{
		if (enemy.HasJump)
		{
			enemy.Jump.CartJump(0f);
		}
	}

	public static void EnemyCartJump(Enemy enemy)
	{
		if (enemy.HasJump)
		{
			enemy.Jump.CartJump(0.1f);
		}
	}

	public static Vector3 EnemyGetNearestPhysObject(Enemy enemy)
	{
		PhysGrabObject physGrabObject = null;
		float num = 9999f;
		Collider[] array = Physics.OverlapSphere(enemy.CenterTransform.position, 3f, LayerMask.GetMask("PhysGrabObject"));
		for (int i = 0; i < array.Length; i++)
		{
			PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
			if ((bool)componentInParent && !componentInParent.GetComponent<EnemyRigidbody>())
			{
				float num2 = Vector3.Distance(enemy.CenterTransform.position, componentInParent.centerPoint);
				if (num2 < num)
				{
					num = num2;
					physGrabObject = componentInParent;
				}
			}
		}
		if ((bool)physGrabObject)
		{
			return physGrabObject.centerPoint;
		}
		return Vector3.zero;
	}

	public static bool EnemySpawn(Enemy enemy)
	{
		float minDistance = 18f;
		float maxDistance = 35f;
		if (EnemyDirector.instance.debugSpawnClose)
		{
			minDistance = 0f;
			maxDistance = 999f;
		}
		LevelPoint levelPoint = enemy.TeleportToPoint(minDistance, maxDistance);
		if ((bool)levelPoint)
		{
			bool flag = ((!enemy.HasRigidbody) ? (!EnemyPhysObjectSphereCheck(levelPoint.transform.position, 1f)) : (!EnemyPhysObjectBoundingBoxCheck(enemy.transform.position, levelPoint.transform.position, enemy.Rigidbody.rb)));
			enemy.EnemyParent.firstSpawnPointUsed = true;
			if (flag)
			{
				return true;
			}
		}
		enemy.EnemyParent.Despawn();
		enemy.EnemyParent.DespawnedTimerSet(UnityEngine.Random.Range(2f, 3f), _min: true);
		return false;
	}

	public static Camera MainCamera()
	{
		return GameDirector.instance.MainCamera;
	}

	public static bool EnemySpawnIdlePause()
	{
		if (EnemyDirector.instance.spawnIdlePauseTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public static bool EnemyForceLeave(Enemy enemy)
	{
		if (enemy.EnemyParent.forceLeave)
		{
			enemy.EnemyParent.forceLeave = false;
			return true;
		}
		return false;
	}

	public static bool OnGroundCheck(Vector3 _position, float _distance, PhysGrabObject _notMe = null)
	{
		RaycastHit[] array = Physics.RaycastAll(_position, Vector3.down, _distance, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"));
		foreach (RaycastHit raycastHit in array)
		{
			PhysGrabObject componentInParent = raycastHit.collider.GetComponentInParent<PhysGrabObject>();
			if (!componentInParent || componentInParent != _notMe)
			{
				return true;
			}
		}
		return false;
	}

	public static PlayerAvatar PlayerGetFromSteamID(string _steamID)
	{
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.steamID == _steamID)
			{
				return player;
			}
		}
		return null;
	}

	public static Transform PlayerGetFaceEyeTransform(PlayerAvatar _player)
	{
		if (!_player.isLocal)
		{
			return _player.playerAvatarVisuals.headLookAtTransform;
		}
		return _player.localCameraTransform;
	}

	public static PlayerAvatar PlayerGetFromName(string _name)
	{
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.playerName == _name)
			{
				return player;
			}
		}
		return null;
	}

	public static Color PlayerGetColorFromSteamID(string _steamID)
	{
		PlayerAvatar playerAvatar = PlayerGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			return playerAvatar.playerAvatarVisuals.color;
		}
		return Color.black;
	}

	public static void ItemAffectEnemyBatteryDrain(EnemyParent _enemyParent, ItemBattery _itemBattery, float tumbleEnemyTimer, float _deltaTime, float _multiplier = 1f)
	{
		if (!_enemyParent || !_itemBattery)
		{
			return;
		}
		Rigidbody componentInChildren = _enemyParent.GetComponentInChildren<Rigidbody>();
		if (!componentInChildren)
		{
			return;
		}
		Enemy componentInChildren2 = _enemyParent.GetComponentInChildren<Enemy>();
		if (!componentInChildren2)
		{
			return;
		}
		switch ((int)_enemyParent.difficulty)
		{
		case 0:
		{
			float value3 = componentInChildren.mass * 0.5f;
			value3 = Mathf.Clamp(value3, 3f, 4f) * _multiplier;
			_itemBattery.batteryLife -= value3 * _deltaTime;
			if (tumbleEnemyTimer > 1.5f && componentInChildren2.HasStateStunned)
			{
				componentInChildren2.StateStunned.Set(1f);
			}
			break;
		}
		case 1:
		{
			float value2 = componentInChildren.mass * 0.85f;
			value2 = Mathf.Clamp(value2, 5f, 6f) * _multiplier;
			_itemBattery.batteryLife -= value2 * _deltaTime;
			if (tumbleEnemyTimer > 3f && componentInChildren2.HasStateStunned)
			{
				componentInChildren2.StateStunned.Set(1f);
			}
			break;
		}
		case 2:
		{
			float value = componentInChildren.mass * 1f;
			value = Mathf.Clamp(value, 7f, 8f) * _multiplier;
			_itemBattery.batteryLife -= value * _deltaTime;
			if (tumbleEnemyTimer > 4f && componentInChildren2.HasStateStunned)
			{
				componentInChildren2.StateStunned.Set(1f);
			}
			break;
		}
		}
	}

	public static void EnemyInvestigate(Vector3 position, float range)
	{
		EnemyDirector.instance.SetInvestigate(position, range);
	}

	public static int EnemyGetIndex(Enemy _enemy)
	{
		int result = -1;
		if ((bool)_enemy)
		{
			foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
			{
				if (item.Enemy == _enemy)
				{
					result = EnemyDirector.instance.enemiesSpawned.IndexOf(item);
					break;
				}
			}
		}
		return result;
	}

	public static Enemy EnemyGetFromIndex(int _enemyIndex)
	{
		Enemy result = null;
		if (_enemyIndex == -1)
		{
			return result;
		}
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			if (EnemyDirector.instance.enemiesSpawned.IndexOf(item) == _enemyIndex)
			{
				result = item.Enemy;
				break;
			}
		}
		return result;
	}

	public static Enemy EnemyGetNearest(Vector3 _position, float _maxDistance, bool _raycast)
	{
		Enemy result = null;
		float num = _maxDistance;
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			if (item.DespawnedTimer > 0f)
			{
				continue;
			}
			Vector3 direction = item.Enemy.CenterTransform.position - _position;
			if (!(direction.magnitude < num))
			{
				continue;
			}
			if (_raycast)
			{
				bool flag = false;
				RaycastHit[] array = Physics.RaycastAll(_position, direction, direction.magnitude, LayerMaskGetVisionObstruct());
				foreach (RaycastHit raycastHit in array)
				{
					if (raycastHit.collider.gameObject.CompareTag("Wall"))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			num = direction.magnitude;
			result = item.Enemy;
		}
		return result;
	}

	public static bool EnemyPhysObjectSphereCheck(Vector3 _position, float _radius)
	{
		if (Physics.OverlapSphere(_position, _radius, LayerMaskGetPhysGrabObject()).Length != 0)
		{
			return true;
		}
		return false;
	}

	public static bool EnemyPhysObjectBoundingBoxCheck(Vector3 _currentPosition, Vector3 _checkPosition, Rigidbody _rigidbody)
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		Collider[] componentsInChildren = _rigidbody.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (bounds.size == Vector3.zero)
			{
				bounds = collider.bounds;
			}
			else
			{
				bounds.Encapsulate(collider.bounds);
			}
		}
		Vector3 vector = _currentPosition - _rigidbody.transform.position;
		Vector3 vector2 = bounds.center - _rigidbody.transform.position;
		bounds.center = _checkPosition - vector + vector2;
		bounds.size *= 1.2f;
		componentsInChildren = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, LayerMaskGetPhysGrabObject());
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetComponentInParent<Rigidbody>() != _rigidbody)
			{
				return true;
			}
		}
		return false;
	}

	public static void DebugDrawBounds(Bounds _bounds, Color _color, float _time)
	{
		Vector3 vector = new Vector3(_bounds.min.x, _bounds.min.y, _bounds.min.z);
		Vector3 vector2 = new Vector3(_bounds.max.x, _bounds.min.y, _bounds.min.z);
		Vector3 vector3 = new Vector3(_bounds.max.x, _bounds.min.y, _bounds.max.z);
		Vector3 vector4 = new Vector3(_bounds.min.x, _bounds.min.y, _bounds.max.z);
		Debug.DrawLine(vector, vector2, _color, _time);
		Debug.DrawLine(vector2, vector3, _color, _time);
		Debug.DrawLine(vector3, vector4, _color, _time);
		Debug.DrawLine(vector4, vector, _color, _time);
		Vector3 vector5 = new Vector3(_bounds.min.x, _bounds.max.y, _bounds.min.z);
		Vector3 vector6 = new Vector3(_bounds.max.x, _bounds.max.y, _bounds.min.z);
		Vector3 vector7 = new Vector3(_bounds.max.x, _bounds.max.y, _bounds.max.z);
		Vector3 vector8 = new Vector3(_bounds.min.x, _bounds.max.y, _bounds.max.z);
		Debug.DrawLine(vector5, vector6, _color, _time);
		Debug.DrawLine(vector6, vector7, _color, _time);
		Debug.DrawLine(vector7, vector8, _color, _time);
		Debug.DrawLine(vector8, vector5, _color, _time);
		Debug.DrawLine(vector, vector5, _color, _time);
		Debug.DrawLine(vector2, vector6, _color, _time);
		Debug.DrawLine(vector3, vector7, _color, _time);
		Debug.DrawLine(vector4, vector8, _color, _time);
	}

	public static bool DebugUser(User _user)
	{
		if (DebugDev() && (bool)DebugComputerCheck.instance && DebugComputerCheck.instance.DebugUser == _user)
		{
			return true;
		}
		return false;
	}

	public static bool Axel()
	{
		return DebugUser(User.Axel);
	}

	public static bool Jannek()
	{
		return DebugUser(User.Jannek);
	}

	public static bool Robin()
	{
		return DebugUser(User.Robin);
	}

	public static bool Ruben()
	{
		return DebugUser(User.Ruben);
	}

	public static bool Walter()
	{
		return DebugUser(User.Walter);
	}

	public static bool DebugKeyDown(User _user, KeyCode _input)
	{
		if (DebugUser(_user))
		{
			return Input.GetKeyUp(_input);
		}
		return false;
	}

	public static bool KeyDownAxel(KeyCode _input)
	{
		return DebugKeyDown(User.Axel, _input);
	}

	public static bool KeyDownJannek(KeyCode _input)
	{
		return DebugKeyDown(User.Jannek, _input);
	}

	public static bool KeyDownRobin(KeyCode _input)
	{
		return DebugKeyDown(User.Robin, _input);
	}

	public static bool KeyDownRuben(KeyCode _input)
	{
		return DebugKeyDown(User.Ruben, _input);
	}

	public static bool KeyDownWalter(KeyCode _input)
	{
		return DebugKeyDown(User.Walter, _input);
	}

	public static bool DebugKey(User _user, KeyCode _input)
	{
		if (DebugUser(_user))
		{
			return Input.GetKey(_input);
		}
		return false;
	}

	public static bool KeyAxel(KeyCode _input)
	{
		return DebugKey(User.Axel, _input);
	}

	public static bool KeyJannek(KeyCode _input)
	{
		return DebugKey(User.Jannek, _input);
	}

	public static bool KeyRobin(KeyCode _input)
	{
		return DebugKey(User.Robin, _input);
	}

	public static bool KeyRuben(KeyCode _input)
	{
		return DebugKey(User.Ruben, _input);
	}

	public static bool KeyWalter(KeyCode _input)
	{
		return DebugKey(User.Walter, _input);
	}

	public static bool DebugDev()
	{
		return SteamManager.instance.developerMode;
	}

	public static float UIMulti()
	{
		return HUDCanvas.instance.rect.sizeDelta.y;
	}

	public static string MessageGeneratedGetLeftBehind()
	{
		List<string> list = new List<string> { "You", "They", "My team", "Everyone", "My friends", "The squad", "The group", "All of them", "Those I trusted", "My companions" };
		List<string> list2 = new List<string> { "left", "abandoned", "betrayed", "forgot", "doomed", "deserted", "ditched", "dissed", "discarded", "forgot" };
		List<string> list3 = new List<string>
		{
			"me", "lil old me", "this lil robot", "my life", "my only hope", "my chance", "this poor robot", "my feelings", "our friendship", "my heart",
			"my trust"
		};
		List<string> list4 = new List<string> { "behind", "alone", "in the dark", "without a word", "without warning", "in silence", "without looking back", "with no remorse" };
		List<string> list5 = new List<string> { "I feel lost.", "Why didn't they wait?", "What did I do wrong?", "I can't believe it.", "How could they?", "This can't be happening.", "I'm on my own now.", "They were my only hope.", "I should have known.", "It's so unfair." };
		List<string> list6 = new List<string> { "{subject} {verb} {object}.", "{additional_phrase} {subject} {verb} {object}...", "{subject} {verb} lil me {adverb}.", "{additional_phrase}", "They {verb} me {adverb}...", "Now, {subject} {verb} {object}.", "In the end, {subject} {verb} {object}.", "I can't believe {subject} {verb} {object}...", "{subject} {verb} {object}. {additional_phrase}", "{additional_phrase} {subject} {verb} {object}." };
		int index = UnityEngine.Random.Range(0, list6.Count);
		return list6[index].Replace("{subject}", list[UnityEngine.Random.Range(0, list.Count)]).Replace("{verb}", list2[UnityEngine.Random.Range(0, list2.Count)]).Replace("{object}", list3[UnityEngine.Random.Range(0, list3.Count)])
			.Replace("{adverb}", list4[UnityEngine.Random.Range(0, list4.Count)])
			.Replace("{additional_phrase}", list5[UnityEngine.Random.Range(0, list5.Count)]);
	}

	public static bool MainMenuIsSingleplayer()
	{
		if (MainMenuOpen.instance.MainMenuGetState() == MainMenuOpen.MainMenuGameModeState.SinglePlayer)
		{
			return true;
		}
		return false;
	}

	public static void MenuActionSingleplayerGame(string saveFileName = null)
	{
		RunManager.instance.ResetProgress();
		if (saveFileName != null)
		{
			Debug.Log("Loading save");
			SaveFileLoad(saveFileName);
		}
		else
		{
			SaveFileCreate();
		}
		DataDirector.instance.RunsPlayedAdd();
		if (RunManager.instance.loadLevel == 0)
		{
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.RunLevel);
		}
		else
		{
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.Shop);
		}
	}

	public static void MenuActionHostGame(string saveFileName = null)
	{
		RunManager.instance.ResetProgress();
		if (saveFileName != null)
		{
			SaveFileLoad(saveFileName);
		}
		else
		{
			SaveFileCreate();
		}
		GameManager.instance.localTest = false;
		RunManager.instance.waitToChangeScene = true;
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
		MainMenuOpen.instance.NetworkConnect();
	}

	public static void SaveFileLoad(string saveFileName)
	{
		StatsManager.instance.LoadGame(saveFileName);
	}

	public static void SaveFileDelete(string saveFileName)
	{
		if (!string.IsNullOrEmpty(saveFileName))
		{
			StatsManager.instance.SaveFileDelete(saveFileName);
		}
	}

	public static List<string> SaveFileGetAll()
	{
		return StatsManager.instance.SaveFileGetAll();
	}

	public static void SaveFileCreate()
	{
		StatsManager.instance.SaveFileCreate();
	}

	public static void SaveFileSave()
	{
		StatsManager.instance.SaveFileSave();
	}

	public static bool MainMenuIsMultiplayer()
	{
		if (MainMenuOpen.instance.MainMenuGetState() == MainMenuOpen.MainMenuGameModeState.MultiPlayer)
		{
			return true;
		}
		return false;
	}

	public static void MainMenuSetSingleplayer()
	{
		MainMenuOpen.instance.MainMenuSetState(0);
	}

	public static void MainMenuSetMultiplayer()
	{
		MainMenuOpen.instance.MainMenuSetState(1);
	}

	public static List<PlayerAvatar> PlayerGetAllPlayerAvatarWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask))
	{
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.isDisabled)
			{
				continue;
			}
			Vector3 position2 = player.PlayerVisionTarget.VisionTransform.position;
			float num = Vector3.Distance(position, position2);
			if (num > range)
			{
				continue;
			}
			Vector3 direction = position2 - position;
			bool flag = false;
			if (doRaycastCheck)
			{
				RaycastHit[] array = Physics.RaycastAll(position, direction, num, layerMask);
				foreach (RaycastHit raycastHit in array)
				{
					if (raycastHit.collider.transform.CompareTag("Wall"))
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				list.Add(player);
			}
		}
		return list;
	}

	public static List<PlayerAvatar> PlayerGetAllPlayerAvatarWithinRangeAndVision(float range, Vector3 position, PhysGrabObject _thisPhysGrabObject = null)
	{
		LayerMask layerMask = LayerMaskGetVisionObstruct();
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.isDisabled)
			{
				continue;
			}
			Vector3 position2 = player.PlayerVisionTarget.VisionTransform.position;
			float num = Vector3.Distance(position, position2);
			if (num > range)
			{
				continue;
			}
			Vector3 direction = position2 - position;
			bool flag = false;
			RaycastHit[] array = Physics.RaycastAll(position, direction, num, layerMask);
			foreach (RaycastHit raycastHit in array)
			{
				if (!(raycastHit.transform.GetComponentInParent<PhysGrabObject>() == _thisPhysGrabObject))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(player);
			}
		}
		return list;
	}

	public static PlayerAvatar PlayerGetNearestPlayerAvatarWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask))
	{
		float num = range;
		PlayerAvatar result = null;
		List<PlayerAvatar> list = PlayerGetAllPlayerAvatarWithinRange(range, position, doRaycastCheck, layerMask);
		if (list.Count > 0)
		{
			foreach (PlayerAvatar item in list)
			{
				Vector3 position2 = item.PlayerVisionTarget.VisionTransform.position;
				float num2 = Vector3.Distance(position, position2);
				if (num2 < num)
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	public static float PlayerNearestDistance(Vector3 position)
	{
		float num = 999f;
		float result = 9f;
		List<PlayerAvatar> playerList = GameDirector.instance.PlayerList;
		if (playerList.Count > 0)
		{
			foreach (PlayerAvatar item in playerList)
			{
				Vector3 position2 = item.PlayerVisionTarget.VisionTransform.position;
				float num2 = Vector3.Distance(position, position2);
				if (num2 < num)
				{
					num = num2;
					result = num;
				}
			}
		}
		return result;
	}

	public static bool PlayerVisionCheck(Vector3 _position, float _range, PlayerAvatar _player, bool _previouslySeen)
	{
		return PlayerVisionCheckPosition(_position, _player.PlayerVisionTarget.VisionTransform.position, _range, _player, _previouslySeen);
	}

	public static bool PlayerVisionCheckPosition(Vector3 _startPosition, Vector3 _endPosition, float _range, PlayerAvatar _player, bool _previouslySeen)
	{
		if (_player.enemyVisionFreezeTimer > 0f)
		{
			return _previouslySeen;
		}
		LayerMask layerMask = LayerMaskGetVisionObstruct();
		Vector3 vector = _endPosition - _startPosition;
		if (vector.magnitude > _range)
		{
			return false;
		}
		if (vector.magnitude < _range)
		{
			_range = vector.magnitude;
		}
		RaycastHit[] array = Physics.RaycastAll(_startPosition, vector, _range, layerMask);
		PlayerAvatar playerAvatar = null;
		Transform transform = null;
		Transform transform2 = null;
		Vector3 vector2 = Vector3.zero;
		float num = 1000f;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			float num2 = Vector3.Distance(_startPosition, raycastHit.point);
			if (!(num2 < num))
			{
				continue;
			}
			num = num2;
			transform2 = raycastHit.transform;
			vector2 = raycastHit.point;
			PlayerAvatar playerAvatar2 = null;
			if (raycastHit.transform.CompareTag("Player"))
			{
				playerAvatar2 = raycastHit.transform.GetComponentInParent<PlayerAvatar>();
				if (!playerAvatar2)
				{
					PlayerController componentInParent = raycastHit.transform.GetComponentInParent<PlayerController>();
					if ((bool)componentInParent)
					{
						playerAvatar2 = componentInParent.playerAvatarScript;
					}
				}
			}
			else
			{
				PlayerTumble componentInParent2 = raycastHit.transform.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent2)
				{
					playerAvatar2 = componentInParent2.playerAvatar;
				}
			}
			if ((bool)playerAvatar2 && playerAvatar2 == _player)
			{
				playerAvatar = playerAvatar2;
				transform = raycastHit.transform;
			}
		}
		if ((bool)playerAvatar && transform == transform2)
		{
			Debug.DrawRay(_startPosition, vector, Color.green, 0.1f);
			return true;
		}
		Debug.DrawRay(_startPosition, vector2 - _startPosition, Color.red, 0.1f);
		return false;
	}

	public static void PlayerEyesOverride(PlayerAvatar _player, Vector3 _position, float _time, GameObject _obj)
	{
		_player.playerAvatarVisuals.playerEyes.Override(_position, _time, _obj);
	}

	public static void PlayerEyesOverrideSoft(Vector3 _position, float _time, GameObject _obj, float _radius)
	{
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (Vector3.Distance(_position, player.transform.position) < _radius)
			{
				player.playerAvatarVisuals.playerEyes.OverrideSoft(_position, _time, _obj);
			}
		}
	}

	public static Transform PlayerGetNearestTransformWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask))
	{
		float num = range;
		Transform result = null;
		List<PlayerAvatar> list = PlayerGetAllPlayerAvatarWithinRange(range, position, doRaycastCheck, layerMask);
		if (list.Count > 0)
		{
			foreach (PlayerAvatar item in list)
			{
				Vector3 position2 = item.PlayerVisionTarget.VisionTransform.position;
				float num2 = Vector3.Distance(position, position2);
				if (num2 < num)
				{
					num = num2;
					result = item.PlayerVisionTarget.VisionTransform;
				}
			}
		}
		return result;
	}

	public static List<PlayerAvatar> PlayerGetAll()
	{
		return GameDirector.instance.PlayerList;
	}

	public static bool PlayersAllInTruck()
	{
		foreach (PlayerAvatar item in PlayerGetList())
		{
			if (!item.isDisabled && !item.RoomVolumeCheck.inTruck)
			{
				return false;
			}
		}
		return true;
	}

	public static void Command(string _command)
	{
		string text = _command.ToLower();
		switch (text)
		{
		default:
		{
			if (text != null && _command.Length >= 4 && _command.Substring(0, 4) == "/fps" && DebugDev() && int.TryParse(_command.Substring(5), out var result))
			{
				GameDirector.instance.CommandSetFPS(result);
			}
			break;
		}
		case "/cinematic":
			GameDirector.instance.CommandRecordingDirectorToggle();
			break;
		case "/greenscreen":
			GameDirector.instance.CommandGreenScreenToggle();
			break;
		case "/enemy vision":
			if (DebugDev())
			{
				EnemyDirector.instance.debugNoVision = !EnemyDirector.instance.debugNoVision;
			}
			break;
		case "/slow":
			if (DebugDev())
			{
				PlayerController.instance.debugSlow = !PlayerController.instance.debugSlow;
			}
			break;
		case "/recording level":
			if (DebugDev())
			{
				RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.Recording);
			}
			break;
		case "/clear":
			Debug.ClearDeveloperConsole();
			break;
		}
	}

	public static void CursorUnlock(float _time)
	{
		CursorManager.instance.Unlock(_time);
	}

	public static bool OnValidateCheck()
	{
		return false;
	}

	public static void LightAdd(PropLight propLight)
	{
		if (!LightManager.instance.propLights.Contains(propLight))
		{
			LightManager.instance.propLights.Add(propLight);
		}
	}

	public static void LightRemove(PropLight propLight)
	{
		if (LightManager.instance.propLights.Contains(propLight))
		{
			LightManager.instance.propLights.Remove(propLight);
		}
	}

	public static Vector3 EnemyRoamFindPoint(Vector3 _position)
	{
		Vector3 result = Vector3.zero;
		LevelPoint levelPoint = LevelPointGet(_position, 10f, 25f);
		if (!levelPoint)
		{
			levelPoint = LevelPointGet(_position, 0f, 999f);
		}
		if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + UnityEngine.Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
		{
			result = hit.position;
		}
		return result;
	}

	public static Vector3 EnemyLeaveFindPoint(Vector3 _position)
	{
		Vector3 result = Vector3.zero;
		LevelPoint levelPoint = LevelPointGetPlayerDistance(_position, 30f, 50f);
		if (!levelPoint)
		{
			levelPoint = LevelPointGetFurthestFromPlayer(_position, 5f);
		}
		if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + UnityEngine.Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
		{
			result = hit.position;
		}
		return result;
	}

	public static List<LevelPoint> LevelPointGetWithinDistance(Vector3 pos, float minDist, float maxDist)
	{
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			float num = Vector3.Distance(levelPathPoint.transform.position, pos);
			if (num >= minDist && num <= maxDist)
			{
				list.Add(levelPathPoint);
			}
		}
		if (list.Count > 0)
		{
			return list;
		}
		return null;
	}

	public static List<LevelPoint> LevelPointsGetAll()
	{
		return LevelGenerator.Instance.LevelPathPoints;
	}

	public static LevelPoint LevelPointsGetClosestToPlayer()
	{
		List<PlayerAvatar> list = PlayerGetList();
		List<LevelPoint> list2 = LevelPointsGetAll();
		float num = 999f;
		LevelPoint result = null;
		foreach (PlayerAvatar item in list)
		{
			if (item.isDisabled)
			{
				continue;
			}
			Vector3 position = item.transform.position;
			foreach (LevelPoint item2 in list2)
			{
				float num2 = Vector3.Distance(position, item2.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = item2;
				}
			}
		}
		return result;
	}

	public static List<LevelPoint> LevelPointsGetAllCloseToPlayers()
	{
		List<PlayerAvatar> list = PlayerGetList();
		List<LevelPoint> list2 = LevelPointsGetAll();
		List<LevelPoint> list3 = new List<LevelPoint>();
		foreach (PlayerAvatar item2 in list)
		{
			float num = 999f;
			LevelPoint item = null;
			if (item2.isDisabled)
			{
				continue;
			}
			Vector3 position = item2.transform.position;
			foreach (LevelPoint item3 in list2)
			{
				float num2 = Vector3.Distance(position, item3.transform.position);
				if (num2 < num)
				{
					num = num2;
					item = item3;
				}
			}
			list3.Add(item);
		}
		return list3;
	}

	public static List<LevelPoint> LevelPointsGetInPlayerRooms()
	{
		List<PlayerAvatar> list = PlayerGetList();
		List<LevelPoint> list2 = LevelPointsGetAll();
		List<LevelPoint> list3 = new List<LevelPoint>();
		foreach (PlayerAvatar item in list)
		{
			if (item.isDisabled)
			{
				continue;
			}
			foreach (RoomVolume currentRoom in item.RoomVolumeCheck.CurrentRooms)
			{
				foreach (LevelPoint item2 in list2)
				{
					if (item2.Room == currentRoom)
					{
						list3.Add(item2);
					}
				}
			}
		}
		return list3;
	}

	public static List<LevelPoint> LevelPointsGetInStartRoom()
	{
		List<LevelPoint> list = LevelPointsGetAll();
		List<LevelPoint> list2 = new List<LevelPoint>();
		foreach (LevelPoint item in list)
		{
			if (item.inStartRoom)
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	public static LevelPoint LevelPointGetPlayerDistance(Vector3 _position, float _minDistance, float _maxDistance, bool _startRoomOnly = false)
	{
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if ((_startRoomOnly && !levelPathPoint.inStartRoom) || levelPathPoint.Room.Truck)
			{
				continue;
			}
			float num = 999f;
			bool flag = false;
			Vector3 position = levelPathPoint.transform.position;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (!player.isDisabled)
				{
					float num2 = Vector3.Distance(position, player.transform.position);
					if (num2 < num)
					{
						num = num2;
					}
					if (num2 < _maxDistance)
					{
						flag = true;
					}
				}
			}
			if (num > _minDistance && flag)
			{
				list.Add(levelPathPoint);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return null;
	}

	public static LevelPoint LevelPointGetFurthestFromPlayer(Vector3 _position, float _minDistance)
	{
		float num = 0f;
		LevelPoint result = null;
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if (levelPathPoint.Room.Truck)
			{
				continue;
			}
			float num2 = 999f;
			float num3 = 0f;
			Vector3 position = levelPathPoint.transform.position;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (!player.isDisabled)
				{
					float num4 = Vector3.Distance(position, player.transform.position);
					if (num4 < num2)
					{
						num2 = num4;
					}
					if (num4 > num3)
					{
						num3 = num4;
					}
				}
			}
			if (num2 > _minDistance && num3 > num)
			{
				num = num3;
				result = levelPathPoint;
			}
		}
		return result;
	}

	public static void PhysLookAtPositionWithForce(Rigidbody rb, Transform transform, Vector3 position, float forceMultiplier)
	{
		Vector3 normalized = (position - transform.position).normalized;
		Vector3 vector = Vector3.Cross(transform.forward, normalized);
		float magnitude = vector.magnitude;
		vector.Normalize();
		rb.AddTorque(vector * magnitude * forceMultiplier);
	}

	public static bool IsNotMasterClient()
	{
		if (GameManager.Multiplayer())
		{
			return !PhotonNetwork.IsMasterClient;
		}
		return false;
	}

	public static bool IsMasterClientOrSingleplayer()
	{
		if (!GameManager.Multiplayer() || !PhotonNetwork.IsMasterClient)
		{
			return !GameManager.Multiplayer();
		}
		return true;
	}

	public static bool IsMasterClient()
	{
		if (GameManager.Multiplayer())
		{
			return PhotonNetwork.IsMasterClient;
		}
		return false;
	}

	public static bool IsMainMenu()
	{
		return MainMenuOpen.instance;
	}

	public static bool IsMultiplayer()
	{
		return GameManager.instance.gameMode == 1;
	}

	public static bool MenuLevel()
	{
		if ((bool)MainMenuOpen.instance || (bool)LobbyMenuOpen.instance)
		{
			return true;
		}
		return false;
	}

	public static bool RunIsArena()
	{
		if (RunManager.instance.levelCurrent == RunManager.instance.levelArena)
		{
			return true;
		}
		return false;
	}

	public static void CameraShake(float strength, float duration)
	{
		GameDirector.instance.CameraShake.Shake(strength, duration);
	}

	public static void CameraShakeDistance(Vector3 position, float strength, float duration, float distanceMin, float distanceMax)
	{
		GameDirector.instance.CameraShake.ShakeDistance(strength, distanceMin, distanceMax, position, duration);
	}

	public static void CameraShakeImpact(float strength, float duration)
	{
		GameDirector.instance.CameraImpact.Shake(strength, duration);
	}

	public static void CameraShakeImpactDistance(Vector3 position, float strength, float duration, float distanceMin, float distanceMax)
	{
		GameDirector.instance.CameraImpact.ShakeDistance(strength, distanceMin, distanceMax, position, duration);
	}

	public static Color ColorDifficultyGet(float minValue, float maxValue, float _currentValue)
	{
		Color[] array = new Color[4]
		{
			new Color(0f, 1f, 0f),
			new Color(1f, 1f, 0f),
			new Color(1f, 0.5f, 0f),
			new Color(1f, 0f, 0f)
		};
		int num = Mathf.FloorToInt(Mathf.Lerp(0f, array.Length - 1, Mathf.InverseLerp(minValue, maxValue, _currentValue)));
		float t = Mathf.InverseLerp(minValue, maxValue, _currentValue) * (float)(array.Length - 1) - (float)num;
		Color color = array[Mathf.Clamp(num, 0, array.Length - 1)];
		Color color2 = array[Mathf.Clamp(num + 1, 0, array.Length - 1)];
		return Color.Lerp(color, color2, t);
	}

	public static string TimeToString(float time, bool fancy = false, Color numberColor = default(Color), Color unitColor = default(Color))
	{
		int num = (int)(time / 3600f);
		int num2 = (int)(time % 3600f / 60f);
		int num3 = (int)(time % 60f);
		string text = "h ";
		string text2 = "m ";
		string text3 = "s";
		if (fancy)
		{
			text = "</b></color><color=#" + ColorUtility.ToHtmlStringRGBA(unitColor) + ">h</color> ";
			text2 = "</b></color><color=#" + ColorUtility.ToHtmlStringRGBA(unitColor) + ">m</color> ";
			text3 = "</b></color><color=#" + ColorUtility.ToHtmlStringRGBA(unitColor) + ">s</color>";
		}
		string text4 = "";
		if (num > 0)
		{
			if (fancy)
			{
				text4 = text4 + "<color=#" + ColorUtility.ToHtmlStringRGBA(numberColor) + "><b>";
			}
			text4 = text4 + num + text;
		}
		if (num2 > 0 || num > 0)
		{
			if (fancy)
			{
				text4 = text4 + "<color=#" + ColorUtility.ToHtmlStringRGBA(numberColor) + "><b>";
			}
			text4 = text4 + num2 + text2;
		}
		if ((num == 0 && num2 == 0) || fancy)
		{
			if (fancy)
			{
				text4 = text4 + "<color=#" + ColorUtility.ToHtmlStringRGBA(numberColor) + "><b>";
			}
			text4 = text4 + num3 + text3;
		}
		return text4;
	}

	public static List<PhysGrabObject> PhysGrabObjectGetAllWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask), PhysGrabObject _thisPhysGrabObject = null)
	{
		List<PhysGrabObject> list = new List<PhysGrabObject>();
		Collider[] array = Physics.OverlapSphere(position, range, LayerMask.GetMask("PhysGrabObject"));
		for (int i = 0; i < array.Length; i++)
		{
			PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
			if (!(componentInParent != null))
			{
				continue;
			}
			bool flag = false;
			if (doRaycastCheck)
			{
				Vector3 normalized = (componentInParent.midPoint - position).normalized;
				RaycastHit[] array2 = Physics.RaycastAll(position, normalized, range, layerMask);
				foreach (RaycastHit raycastHit in array2)
				{
					PhysGrabObject componentInParent2 = raycastHit.collider.GetComponentInParent<PhysGrabObject>();
					if (!(componentInParent2 == _thisPhysGrabObject) && !(componentInParent2 == componentInParent) && (componentInParent2 == null || (componentInParent2 != null && componentInParent2 != componentInParent)))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				list.Add(componentInParent);
			}
		}
		return list;
	}

	public static bool LocalPlayerOverlapCheck(float range, Vector3 position, bool doRaycastCheck = false)
	{
		Collider[] array = Physics.OverlapSphere(position, range, LayerMaskGetVisionObstruct());
		foreach (Collider collider in array)
		{
			PlayerController playerController = null;
			if (collider.transform.CompareTag("Player"))
			{
				playerController = collider.GetComponentInParent<PlayerController>();
			}
			else
			{
				PlayerTumble componentInParent = collider.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent && componentInParent.playerAvatar.isLocal)
				{
					playerController = PlayerController.instance;
				}
			}
			if (!playerController)
			{
				continue;
			}
			bool flag = false;
			if (doRaycastCheck)
			{
				Vector3 normalized = (collider.transform.position - position).normalized;
				RaycastHit[] array2 = Physics.RaycastAll(position, normalized, range, LayerMask.GetMask("Default"));
				foreach (RaycastHit raycastHit in array2)
				{
					if (raycastHit.transform.CompareTag("Wall"))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public static Vector3 PhysFollowPosition(Vector3 _currentPosition, Vector3 _targetPosition, Vector3 _currentVelocity, float _maxSpeed)
	{
		return Vector3.ClampMagnitude((_targetPosition - _currentPosition) / Time.fixedDeltaTime, _maxSpeed) - _currentVelocity;
	}

	public static Vector3 PhysFollowRotation(Transform _transform, Quaternion _targetRotation, Rigidbody _rigidbody, float _maxSpeed)
	{
		_transform.rotation = Quaternion.RotateTowards(_targetRotation, _transform.rotation, 360f);
		(_targetRotation * Quaternion.Inverse(_transform.rotation)).ToAngleAxis(out var angle, out var axis);
		axis.Normalize();
		Vector3 direction = axis * angle * (MathF.PI / 180f) / Time.fixedDeltaTime;
		direction -= _rigidbody.angularVelocity;
		Vector3 vector = _transform.InverseTransformDirection(direction);
		vector = _rigidbody.inertiaTensorRotation * vector;
		vector.Scale(_rigidbody.inertiaTensor);
		Vector3 direction2 = Quaternion.Inverse(_rigidbody.inertiaTensorRotation) * vector;
		return Vector3.ClampMagnitude(_transform.TransformDirection(direction2), _maxSpeed);
	}

	public static Vector3 PhysFollowDirection(Transform _transform, Vector3 _targetDirection, Rigidbody _rigidbody, float _maxSpeed)
	{
		Vector3 normalized = Vector3.Cross(Vector3.up, _targetDirection).normalized;
		Quaternion rotation = _transform.rotation;
		_transform.Rotate(normalized * 100f, Space.World);
		Quaternion rotation2 = _transform.rotation;
		_transform.rotation = rotation;
		return PhysFollowRotation(_transform.transform, rotation2, _rigidbody, _maxSpeed);
	}

	public static LevelPoint LevelPointGet(Vector3 _position, float _minDistance, float _maxDistance)
	{
		LevelPoint result = null;
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if (!levelPathPoint.Room.Truck)
			{
				float num = Vector3.Distance(levelPathPoint.transform.position, _position);
				if (num >= _minDistance && num <= _maxDistance)
				{
					list.Add(levelPathPoint);
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		return result;
	}

	public static LevelPoint LevelPointInTargetRoomGet(RoomVolumeCheck _target, float _minDistance, float _maxDistance, LevelPoint ignorePoint = null)
	{
		LevelPoint result = null;
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			foreach (RoomVolume currentRoom in _target.CurrentRooms)
			{
				if (!(levelPathPoint == ignorePoint) && levelPathPoint.Room == currentRoom)
				{
					float num = Vector3.Distance(levelPathPoint.transform.position, _target.CheckPosition);
					if (num >= _minDistance && num <= _maxDistance)
					{
						list.Add(levelPathPoint);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		return result;
	}

	public static bool OnScreen(Vector3 position, float paddWidth, float paddHeight)
	{
		paddWidth = (float)Screen.width * paddWidth;
		paddHeight = (float)Screen.height * paddHeight;
		Vector3 vector = CameraUtils.Instance.MainCamera.WorldToScreenPoint(position);
		vector.x *= (float)Screen.width / RenderTextureMain.instance.textureWidth;
		vector.y *= (float)Screen.height / RenderTextureMain.instance.textureHeight;
		if (vector.z > 0f && vector.x > 0f - paddWidth && vector.x < (float)Screen.width + paddWidth && vector.y > 0f - paddHeight && vector.y < (float)Screen.height + paddHeight)
		{
			return true;
		}
		return false;
	}

	public static Quaternion ClampRotation(Quaternion _quaternion, Vector3 _bounds)
	{
		_quaternion.x /= _quaternion.w;
		_quaternion.y /= _quaternion.w;
		_quaternion.z /= _quaternion.w;
		_quaternion.w = 1f;
		float value = 114.59156f * Mathf.Atan(_quaternion.x);
		value = Mathf.Clamp(value, 0f - _bounds.x, _bounds.x);
		_quaternion.x = Mathf.Tan(MathF.PI / 360f * value);
		float value2 = 114.59156f * Mathf.Atan(_quaternion.y);
		value2 = Mathf.Clamp(value2, 0f - _bounds.y, _bounds.y);
		_quaternion.y = Mathf.Tan(MathF.PI / 360f * value2);
		float value3 = 114.59156f * Mathf.Atan(_quaternion.z);
		value3 = Mathf.Clamp(value3, 0f - _bounds.z, _bounds.z);
		_quaternion.z = Mathf.Tan(MathF.PI / 360f * value3);
		return _quaternion.normalized;
	}

	public static Vector3 ClampDirection(Vector3 _direction, Vector3 _forward, float _maxAngle)
	{
		Vector3 result = _direction;
		if (Vector3.Angle(_direction, _forward) > _maxAngle)
		{
			Vector3 axis = Vector3.Cross(_forward, _direction);
			result = Quaternion.AngleAxis(_maxAngle, axis) * _forward;
		}
		return result;
	}

	public static List<PlayerAvatar> PlayerGetList()
	{
		return GameDirector.instance.PlayerList;
	}

	public static int PhotonViewIDPlayerAvatarLocal()
	{
		return PlayerAvatar.instance.GetComponent<PhotonView>().ViewID;
	}

	public static string EmojiText(string inputText)
	{
		inputText = inputText.Replace("{", "<sprite name=");
		inputText = inputText.Replace("}", ">");
		return inputText;
	}

	public static string DollarGetString(int value)
	{
		return value.ToString("#,0", new CultureInfo("de-DE"));
	}

	public static PhysicMaterial PhysicMaterialSticky()
	{
		return AssetManager.instance.physicMaterialStickyExtreme;
	}

	public static PhysicMaterial PhysicMaterialSlippery()
	{
		return AssetManager.instance.physicMaterialSlipperyExtreme;
	}

	public static PhysicMaterial PhysicMaterialSlipperyPlus()
	{
		return AssetManager.instance.physicMaterialSlipperyPlus;
	}

	public static PhysicMaterial PhysicMaterialDefault()
	{
		return AssetManager.instance.physicMaterialDefault;
	}

	public static PhysicMaterial PhysicMaterialPhysGrabObject()
	{
		return AssetManager.instance.physicMaterialPhysGrabObject;
	}

	public static int RunGetLevelsMax()
	{
		return RunManager.instance.levelsMax;
	}

	public static int RunGetLevelsCompleted()
	{
		return RunManager.instance.levelsCompleted;
	}

	public static float RunGetDifficultyMultiplier()
	{
		return (float)RunManager.instance.levelsCompleted / (float)RunManager.instance.levelsMax;
	}

	public static bool PhysGrabObjectIsGrabbed(PhysGrabObject physGrabObject)
	{
		return physGrabObject.grabbed;
	}

	public static List<PhysGrabber> PhysGrabObjectGetPhysGrabbersGrabbing(PhysGrabObject physGrabObject)
	{
		return physGrabObject.playerGrabbing;
	}

	public static List<PlayerAvatar> PhysGrabObjectGetPlayerAvatarsGrabbing(PhysGrabObject physGrabObject)
	{
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			list.Add(item.playerAvatar);
		}
		return list;
	}

	public static bool PhysGrabberLocalIsGrabbing()
	{
		return PhysGrabber.instance.grabbed;
	}

	public static void PhysGrabberLocalForceDrop()
	{
		if (PhysGrabber.instance.grabbed)
		{
			PhysGrabber.instance.OverrideGrabRelease();
		}
	}

	public static void PhysGrabberLocalForceGrab(PhysGrabObject physGrabObject)
	{
		PhysGrabber.instance.OverrideGrab(physGrabObject);
	}

	public static PhysGrabObject PhysGrabberLocalGetGrabbedPhysGrabObject()
	{
		if (!PhysGrabber.instance.grabbed)
		{
			return null;
		}
		return PhysGrabber.instance.grabbedObject.GetComponent<PhysGrabObject>();
	}

	public static bool PhysGrabberIsGrabbing(PhysGrabber physGrabber)
	{
		return physGrabber.grabbed;
	}

	public static PhysGrabObject PhysGrabberGetGrabbedPhysGrabObject(PhysGrabber physGrabber)
	{
		if (!physGrabber.grabbed)
		{
			return null;
		}
		return physGrabber.grabbedObject.GetComponent<PhysGrabObject>();
	}

	public static void PhysGrabberForceDrop(PhysGrabber physGrabber)
	{
		if (physGrabber.grabbed)
		{
			physGrabber.OverrideGrabRelease();
		}
	}

	public static void PhysGrabberForceGrab(PhysGrabber physGrabber, PhysGrabObject physGrabObject)
	{
		physGrabber.OverrideGrab(physGrabObject);
	}

	public static void PhysGrabberLocalChangeAlpha(float alpha)
	{
		PhysGrabber.instance.ChangeBeamAlpha(alpha);
	}

	public static void LightManagerSetCullTargetTransform(Transform target)
	{
		LightManager.instance.lightCullTarget = target;
		LightManager.instance.UpdateInstant();
	}

	public static string MenuGetSelectableID(GameObject gameObject)
	{
		return "" + gameObject.GetInstanceID();
	}

	public static void MenuSelectionBoxTargetSet(MenuPage parentPage, RectTransform rectTransform, Vector2 customOffset = default(Vector2), Vector2 customScale = default(Vector2))
	{
		Vector2 vector = UIGetRectTransformPositionOnScreen(rectTransform, withScreenMultiplier: false);
		Vector2 vector2 = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		vector += new Vector2(vector2.x / 2f, vector2.y / 2f) + customOffset;
		MenuSelectableElement component = rectTransform.GetComponent<MenuSelectableElement>();
		MenuSelectionBox menuSelectionBox = parentPage.selectionBox;
		Vector2 vector3 = new Vector2(0f, 0f);
		bool isInScrollBox = false;
		if ((bool)component && component.isInScrollBox)
		{
			isInScrollBox = true;
			menuSelectionBox = component.menuScrollBox.menuSelectionBox;
			Transform parent = rectTransform.parent;
			int num = 30;
			while ((bool)parent && !parent.GetComponent<MenuPage>())
			{
				RectTransform component2 = parent.GetComponent<RectTransform>();
				if ((bool)component2 && !component2.GetComponent<MenuSelectableElement>())
				{
					vector3 -= new Vector2(component2.localPosition.x, component2.localPosition.y);
				}
				parent = parent.parent;
				num--;
				if (num <= 0)
				{
					Debug.LogError(rectTransform.name + " - Hover FAIL! Could not find a parent page ");
					break;
				}
			}
		}
		vector += vector3;
		MenuElementAnimations componentInParent = rectTransform.GetComponentInParent<MenuElementAnimations>();
		if ((bool)componentInParent)
		{
			_ = (float)Screen.width / (float)MenuManager.instance.screenUIWidth;
			_ = (float)Screen.height / (float)MenuManager.instance.screenUIHeight;
			componentInParent.GetComponent<RectTransform>();
		}
		menuSelectionBox.MenuSelectionBoxSetTarget(vector, vector2, component.parentPage, isInScrollBox, component.menuScrollBox, customScale);
	}

	public static float MenuGetPitchFromYPos(RectTransform rectTransform)
	{
		return Mathf.Lerp(0.5f, 2f, rectTransform.localPosition.y / (float)Screen.height);
	}

	public static Vector2 UIPositionToUIPosition(Vector3 position)
	{
		Vector3 vector = CameraOverlay.instance.overlayCamera.ScreenToViewportPoint(position) * UIMulti();
		vector.x *= 1.015f;
		vector.y *= 1.015f;
		vector.x /= Screen.width;
		vector.y /= Screen.height;
		float num = HUDCanvas.instance.rect.sizeDelta.x / HUDCanvas.instance.rect.sizeDelta.y;
		float num2 = HUDCanvas.instance.rect.sizeDelta.x * num / HUDCanvas.instance.rect.sizeDelta.y;
		vector.x *= HUDCanvas.instance.rect.sizeDelta.x * num2;
		vector.y *= HUDCanvas.instance.rect.sizeDelta.y * num;
		vector.x -= 18f;
		vector.y -= 15f;
		return new Vector2(vector.x, vector.y);
	}

	public static Vector2 UIMousePosToUIPos()
	{
		Vector3 vector = CameraOverlay.instance.overlayCamera.ScreenToViewportPoint(Input.mousePosition) * UIMulti();
		vector.x *= 1.015f;
		vector.y *= 1.015f;
		vector.x /= Screen.width;
		vector.y /= Screen.height;
		float num = HUDCanvas.instance.rect.sizeDelta.x / HUDCanvas.instance.rect.sizeDelta.y;
		float num2 = HUDCanvas.instance.rect.sizeDelta.x * num / HUDCanvas.instance.rect.sizeDelta.y;
		vector.x *= HUDCanvas.instance.rect.sizeDelta.x * num2;
		vector.y *= HUDCanvas.instance.rect.sizeDelta.y * num;
		return new Vector2(vector.x, vector.y);
	}

	public static Vector2 UIGetRectTransformPositionOnScreen(RectTransform rectTransform, bool withScreenMultiplier = true)
	{
		int num = 1;
		int num2 = 1;
		Vector3 position = rectTransform.position;
		Vector3 position2 = rectTransform.GetComponentInParent<MenuPage>().GetComponent<RectTransform>().position;
		Vector3 vector = position - position2;
		vector -= new Vector3(rectTransform.rect.width * rectTransform.pivot.x, rectTransform.rect.height * rectTransform.pivot.y, 0f);
		if (withScreenMultiplier)
		{
			vector = new Vector2(vector.x * (float)num, vector.y * (float)num2);
		}
		return vector;
	}

	public static Vector2 UIMouseGetLocalPositionWithinRectTransform(RectTransform rectTransform)
	{
		Vector2 vector = UIMousePosToUIPos();
		Vector2 vector2 = UIGetRectTransformPositionOnScreen(rectTransform, withScreenMultiplier: false);
		Vector2 vector3 = new Vector2(vector.x - vector2.x, vector.y - vector2.y);
		float num = rectTransform.rect.width * rectTransform.pivot.x;
		float num2 = rectTransform.rect.height * rectTransform.pivot.y;
		Vector3 lossyScale = rectTransform.lossyScale;
		float num3 = 1f;
		if (lossyScale.y < 1f)
		{
			num3 = 1f + (1f - lossyScale.y);
		}
		if (lossyScale.y > 1f)
		{
			num3 = 1f + (lossyScale.y - 1f);
		}
		return new Vector2((vector3.x + num) * num3, (vector3.y + num2) * num3);
	}

	public static bool UIMouseHover(MenuPage parentPage, RectTransform rectTransform, string menuID, float xPadding = 0f, float yPadding = 0f)
	{
		if ((bool)parentPage.parentPage && !parentPage.parentPage.pageActive)
		{
			return false;
		}
		Vector2 vector = UIMousePosToUIPos();
		if (MenuManager.instance.mouseHoldPosition != Vector2.zero)
		{
			vector = MenuManager.instance.mouseHoldPosition;
		}
		int num = 1;
		int num2 = 1;
		MenuScrollBox componentInParent = rectTransform.GetComponentInParent<MenuScrollBox>();
		if ((bool)componentInParent)
		{
			float num3 = (componentInParent.transform.position.y - 10f) * (float)num2;
			float num4 = (componentInParent.scrollerEndPosition + 32f) * (float)num2;
			if (vector.y > num4 || vector.y < num3)
			{
				return false;
			}
		}
		Vector2 vector2 = UIGetRectTransformPositionOnScreen(rectTransform, withScreenMultiplier: false);
		float num5 = (vector2.x + (rectTransform.rect.xMin - xPadding)) * (float)num;
		float num6 = (vector2.x + (rectTransform.rect.xMax + xPadding)) * (float)num;
		float num7 = (vector2.y + (rectTransform.rect.yMin - yPadding)) * (float)num2;
		float num8 = (vector2.y + (rectTransform.rect.yMax + yPadding)) * (float)num2;
		bool flag = false;
		if (vector.x >= num5 && vector.x <= num6 && vector.y >= num7 && vector.y <= num8)
		{
			flag = true;
			if (menuID != "-1")
			{
				if (MenuManager.instance.currentMenuID == menuID)
				{
					MenuManager.instance.MenuHover();
				}
				if (MenuManager.instance.currentMenuID == "")
				{
					MenuManager.instance.currentMenuID = menuID;
				}
			}
		}
		else
		{
			flag = false;
			if (menuID != "-1" && MenuManager.instance.currentMenuID == menuID)
			{
				MenuManager.instance.currentMenuID = "";
			}
		}
		if (menuID != "-1")
		{
			if (menuID == MenuManager.instance.currentMenuID)
			{
				return true;
			}
			return false;
		}
		return flag;
	}

	public static void UIHideAim()
	{
		Aim.instance.SetState(Aim.State.Hidden);
	}

	public static void UIHideTumble()
	{
		TumbleUI.instance.Hide();
	}

	public static void UIHideWorldSpace()
	{
		WorldSpaceUIParent.instance.Hide();
	}

	public static void UIHideValuableDiscover()
	{
		ValuableDiscover.instance.Hide();
	}

	public static void UIShowArrow(Vector3 startPosition, Vector3 endPosition, float rotation)
	{
		ArrowUI.instance.ArrowShow(startPosition, endPosition, rotation);
	}

	public static void UIShowArrowWorldPosition(Vector3 startPosition, Vector3 endPosition, float rotation)
	{
		ArrowUI.instance.ArrowShowWorldPos(startPosition, endPosition, rotation);
	}

	public static void UIBigMessage(string message, string emoji, float size, Color colorMain, Color colorFlash)
	{
		BigMessageUI.instance.BigMessage(message, emoji, size, colorMain, colorFlash);
	}

	public static void UIFocusText(string message, Color colorMain, Color colorFlash, float time = 3f)
	{
		MissionUI.instance.MissionText(message, colorMain, colorFlash, time);
	}

	public static void UIItemInfoText(ItemAttributes itemAttributes, string message)
	{
		ItemInfoUI.instance.ItemInfoText(itemAttributes, message);
	}

	public static void UIHideHealth()
	{
		HealthUI.instance.Hide();
	}

	public static void UIHideEnergy()
	{
		EnergyUI.instance.Hide();
	}

	public static void UIHideInventory()
	{
		InventoryUI.instance.Hide();
	}

	public static void UIHideHaul()
	{
		HaulUI.instance.Hide();
	}

	public static void UIHideGoal()
	{
		GoalUI.instance.Hide();
	}

	public static void UIHideCurrency()
	{
		CurrencyUI.instance.Hide();
	}

	public static void UIHideShopCost()
	{
		ShopCostUI.instance.Hide();
	}

	public static void UIShowSpectate()
	{
		if (IsMultiplayer() && (bool)SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Normal) && SpectateNameUI.instance.Text.text != "" && (!Arena.instance || Arena.instance.currentState != Arena.States.GameOver))
		{
			SpectateNameUI.instance.Show();
		}
	}

	public static Vector3 UIWorldToCanvasPosition(Vector3 _worldPosition)
	{
		RectTransform rect = HUDCanvas.instance.rect;
		if (OnScreen(_worldPosition, 0.5f, 0.5f))
		{
			Vector3 vector = AssetManager.instance.mainCamera.WorldToViewportPoint(_worldPosition);
			return new Vector3(vector.x * rect.sizeDelta.x - rect.sizeDelta.x * 0.5f, vector.y * rect.sizeDelta.y - rect.sizeDelta.y * 0.5f, vector.z);
		}
		return new Vector3((0f - rect.sizeDelta.x) * 2f, (0f - rect.sizeDelta.y) * 2f, 0f);
	}

	public static bool FPSImpulse1()
	{
		return GameDirector.instance.fpsImpulse1;
	}

	public static bool FPSImpulse5()
	{
		return GameDirector.instance.fpsImpulse5;
	}

	public static bool FPSImpulse15()
	{
		return GameDirector.instance.fpsImpulse15;
	}

	public static bool FPSImpulse30()
	{
		return GameDirector.instance.fpsImpulse30;
	}

	public static bool FPSImpulse60()
	{
		return GameDirector.instance.fpsImpulse60;
	}

	public static void LocalPlayerOverrideEnergyUnlimited()
	{
		PlayerController.instance.EnergyCurrent = 100f;
	}

	public static void HUDSpectateSetName(string name)
	{
		SpectateNameUI.instance.SetName(name);
	}

	public static int ValuableGetTotalNumber()
	{
		return ValuableDirector.instance.valuableSpawnAmount;
	}

	public static bool ValuableTrapActivatedDiceRoll(int rarityLevel)
	{
		if (rarityLevel == 1)
		{
			return UnityEngine.Random.Range(1, 3) == 1;
		}
		if (rarityLevel == 2)
		{
			return UnityEngine.Random.Range(1, 5) == 1;
		}
		if (rarityLevel > 2)
		{
			return UnityEngine.Random.Range(1, 10) == 1;
		}
		return false;
	}

	public static LayerMask LayerMaskGetVisionObstruct()
	{
		return LayerMask.GetMask("Default", "Player", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "StaticGrabObject");
	}

	public static LayerMask LayerMaskGetShouldHits()
	{
		return LayerMask.GetMask("Default", "Player", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "StaticGrabObject", "Enemy");
	}

	public static LayerMask LayerMaskGetPlayersAndPhysObjects()
	{
		return LayerMask.GetMask("Player", "PhysGrabObject");
	}

	public static LayerMask LayerMaskGetPhysGrabObject()
	{
		return LayerMask.GetMask("PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "StaticGrabObject");
	}

	public static float BatteryGetChargeRate(int chargeLevel)
	{
		if (chargeLevel == 1)
		{
			return 1f;
		}
		if (chargeLevel == 2)
		{
			return 2f;
		}
		if (chargeLevel >= 3)
		{
			return 5f;
		}
		return 0f;
	}

	public static bool BatteryChargeCondition(ItemBattery battery)
	{
		if ((bool)battery && ((battery.batteryLife < 100f && battery.batteryActive) || (battery.batteryLife < 99f && !battery.batteryActive)))
		{
			return !battery.isUnchargable;
		}
		return false;
	}

	public static bool InventoryAnyEquipButton()
	{
		if (!InputHold(InputKey.Inventory1) && !InputHold(InputKey.Inventory2))
		{
			return InputHold(InputKey.Inventory3);
		}
		return true;
	}

	public static bool InventoryAnyEquipButtonUp()
	{
		if (!InputUp(InputKey.Inventory1) && !InputUp(InputKey.Inventory2))
		{
			return InputUp(InputKey.Inventory3);
		}
		return true;
	}

	public static bool InventoryAnyEquipButtonDown()
	{
		if (!InputDown(InputKey.Inventory1) && !InputDown(InputKey.Inventory2))
		{
			return InputDown(InputKey.Inventory3);
		}
		return true;
	}

	public static bool LevelGenDone()
	{
		return LevelGenerator.Instance.Generated;
	}

	public static bool RunIsLobbyMenu()
	{
		return RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu;
	}

	public static bool RunIsShop()
	{
		return RunManager.instance.levelCurrent == RunManager.instance.levelShop;
	}

	public static bool RunIsLobby()
	{
		return RunManager.instance.levelCurrent == RunManager.instance.levelLobby;
	}

	public static bool RunIsTutorial()
	{
		return RunManager.instance.levelCurrent == RunManager.instance.levelTutorial;
	}

	public static bool RunIsRecording()
	{
		return RunManager.instance.levelCurrent == RunManager.instance.levelRecording;
	}

	public static bool RunIsLevel()
	{
		if (RunManager.instance.levelCurrent != RunManager.instance.levelShop && RunManager.instance.levelCurrent != RunManager.instance.levelLobby && RunManager.instance.levelCurrent != RunManager.instance.levelLobbyMenu && RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu && RunManager.instance.levelCurrent != RunManager.instance.levelTutorial)
		{
			return RunManager.instance.levelCurrent != RunManager.instance.levelArena;
		}
		return false;
	}

	public static T Singleton<T>(ref T instance, GameObject gameObject) where T : Component
	{
		Debug.Log("Singleton called for type " + typeof(T).Name + " on GameObject " + gameObject.name);
		if (instance == null)
		{
			Debug.Log("No existing instance found, setting up new instance of " + typeof(T).Name);
			instance = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
			Debug.Log("DontDestroyOnLoad called for " + gameObject.name);
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		else if (instance.gameObject != gameObject)
		{
			Debug.Log("Instance already exists for type " + typeof(T).Name + ", destroying game object " + gameObject.name);
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			Debug.Log("Instance matches the current gameObject " + gameObject.name + ", no action needed");
		}
		Debug.Log("Singleton setup completed for type " + typeof(T).Name + " on GameObject " + gameObject.name);
		return instance;
	}

	public static void StatSetBattery(string itemName, int value)
	{
		StatsManager.instance.ItemUpdateStatBattery(itemName, value);
	}

	public static int StatSetRunLives(int value)
	{
		return PunManager.instance.SetRunStatSet("lives", value);
	}

	public static int StatSetRunCurrency(int value)
	{
		return PunManager.instance.SetRunStatSet("currency", value);
	}

	public static int StatSetRunTotalHaul(int value)
	{
		return PunManager.instance.SetRunStatSet("totalHaul", value);
	}

	public static int StatSetRunLevel(int value)
	{
		return PunManager.instance.SetRunStatSet("level", value);
	}

	public static int StatSetSaveLevel(int value)
	{
		return PunManager.instance.SetRunStatSet("save level", value);
	}

	public static int StatSetRunFailures(int value)
	{
		return PunManager.instance.SetRunStatSet("failures", value);
	}

	public static int StatGetItemBattery(string itemName)
	{
		return StatsManager.instance.itemStatBattery[itemName];
	}

	public static int StatGetItemsPurchased(string itemName)
	{
		return StatsManager.instance.itemsPurchased[itemName];
	}

	public static int StatGetRunCurrency()
	{
		return StatsManager.instance.GetRunStatCurrency();
	}

	public static int StatGetRunTotalHaul()
	{
		return StatsManager.instance.GetRunStatTotalHaul();
	}

	public static int StatUpgradeItemBattery(string itemName)
	{
		return PunManager.instance.UpgradeItemBattery(itemName);
	}

	public static void StatSyncAll()
	{
		StatsManager.instance.statsSynced = false;
		PunManager.instance.SyncAllDictionaries();
	}

	public static bool StatsSynced()
	{
		return StatsManager.instance.statsSynced;
	}

	public static void ShopPopulateItemVolumes()
	{
		PunManager.instance.ShopPopulateItemVolumes();
	}

	public static int ShopGetTotalCost()
	{
		return ShopManager.instance.totalCost;
	}

	public static void ShopUpdateCost()
	{
		PunManager.instance.ShopUpdateCost();
	}

	public static void OnLevelGenDone()
	{
		ItemManager.instance.TurnOffIconLightsAgain();
		if (RunIsLobby())
		{
			TutorialDirector.instance.TipsShow();
		}
	}

	public static void OnSceneSwitch(bool _gameOver, bool _leaveGame)
	{
		ItemManager.instance.itemIconLights.SetActive(value: true);
		if (IsMultiplayer())
		{
			ChatManager.instance.ForceSendMessage(":o");
			ChatManager.instance.ClearAllChatBatches();
		}
		if (RunManager.instance.levelCurrent == RunManager.instance.levelLobby)
		{
			TutorialDirector instance = TutorialDirector.instance;
			if ((bool)instance)
			{
				instance.UpdateRoundEnd();
				instance.TipsStore();
			}
		}
		StatsManager.instance.StuffNeedingResetAtTheEndOfAScene();
		TutorialDirector.instance.TipCancel();
		ItemManager.instance.FetchLocalPlayersInventory();
		ItemManager.instance.powerCrystals.Clear();
		if ((bool)ChargingStation.instance && !_gameOver)
		{
			PunManager.instance.SetRunStatSet("chargingStationCharge", ChargingStation.instance.chargeInt);
		}
		if (IsMasterClientOrSingleplayer() && !_leaveGame && !_gameOver)
		{
			SaveFileSave();
		}
		DataDirector.instance.SaveDeleteCheck(_leaveGame);
		if (!_leaveGame)
		{
			StatSyncAll();
		}
		if (_leaveGame)
		{
			SessionManager.instance.Reset();
		}
	}

	public static PlayerAvatar PlayerAvatarGetFromPhotonID(int photonID)
	{
		PlayerAvatar result = null;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.photonView.ViewID == photonID)
			{
				result = player;
			}
		}
		return result;
	}

	public static PlayerAvatar PlayerAvatarGetFromSteamID(string _steamID)
	{
		PlayerAvatar result = null;
		if (IsMultiplayer())
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (player.steamID == _steamID)
				{
					result = player;
				}
			}
		}
		if (!IsMultiplayer())
		{
			result = PlayerAvatar.instance;
		}
		return result;
	}

	public static PlayerAvatar PlayerAvatarGetFromSteamIDshort(int _steamIDshort)
	{
		PlayerAvatar result = null;
		if (IsMultiplayer())
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (player.steamIDshort == _steamIDshort)
				{
					result = player;
				}
			}
		}
		if (!IsMultiplayer())
		{
			result = PlayerAvatar.instance;
		}
		return result;
	}

	public static PlayerAvatar PlayerAvatarLocal()
	{
		return PlayerAvatar.instance;
	}

	public static string PlayerGetName(PlayerAvatar player)
	{
		if (IsMultiplayer())
		{
			return player.photonView.Owner.NickName;
		}
		return SteamClient.Name;
	}

	public static string PlayerGetSteamID(PlayerAvatar player)
	{
		return player.steamID;
	}

	public static void TruckPopulateItemVolumes()
	{
		PunManager.instance.TruckPopulateItemVolumes();
	}

	public static void LevelSuccessful()
	{
	}

	public static Quaternion SpringQuaternionGet(SpringQuaternion _attributes, Quaternion _targetRotation, float _deltaTime = -1f)
	{
		if (_deltaTime == -1f)
		{
			_deltaTime = Time.deltaTime;
		}
		if (!_attributes.setup)
		{
			_attributes.lastRotation = _targetRotation;
			_attributes.setup = true;
		}
		if (float.IsNaN(_attributes.springVelocity.x))
		{
			_attributes.springVelocity = Vector3.zero;
			_attributes.lastRotation = _targetRotation;
		}
		_targetRotation = Quaternion.RotateTowards(_attributes.lastRotation, _targetRotation, 360f);
		Quaternion quaternion = _targetRotation;
		Quaternion currentX = _attributes.lastRotation * Conjugate(quaternion);
		Vector3 zero = Vector3.zero;
		DampedSpringGeneralSolution(out var _newX, out var _newV, currentX, _attributes.springVelocity - zero, _deltaTime, _attributes.damping, _attributes.speed);
		float magnitude = _newV.magnitude;
		if (magnitude * Time.deltaTime > MathF.PI)
		{
			_newV *= MathF.PI / magnitude;
		}
		_attributes.springVelocity = _newV + zero;
		_attributes.lastRotation = _newX * quaternion;
		if (_attributes.clamp && Quaternion.Angle(_attributes.lastRotation, _targetRotation) > _attributes.maxAngle)
		{
			_attributes.lastRotation = Quaternion.RotateTowards(_targetRotation, _attributes.lastRotation, _attributes.maxAngle);
		}
		return _attributes.lastRotation;
	}

	public static float SpringFloatGet(SpringFloat _attributes, float _targetFloat, float _deltaTime = -1f)
	{
		if (_deltaTime == -1f)
		{
			_deltaTime = Time.deltaTime;
		}
		float currentX = _attributes.lastPosition - _targetFloat;
		DampedSpringGeneralSolution(out var _newX, out var _newV, currentX, _attributes.springVelocity, _deltaTime, _attributes.damping, _attributes.speed);
		float num = _newX;
		_attributes.springVelocity = _newV;
		_attributes.lastPosition = _targetFloat + num;
		if (_attributes.clamp)
		{
			float lastPosition = _attributes.lastPosition;
			_attributes.lastPosition = Mathf.Clamp(_attributes.lastPosition, _attributes.min, _attributes.max);
			if (lastPosition != _attributes.lastPosition)
			{
				_attributes.springVelocity *= -1f;
			}
		}
		return _attributes.lastPosition;
	}

	public static Vector3 SpringVector3Get(SpringVector3 _attributes, Vector3 _targetPosition, float _deltaTime = -1f)
	{
		if (_deltaTime == -1f)
		{
			_deltaTime = Time.deltaTime;
		}
		Vector3 vector = _attributes.lastPosition - _targetPosition;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < 3; i++)
		{
			DampedSpringGeneralSolution(out var _newX, out var _newV, vector[i], _attributes.springVelocity[i], _deltaTime, _attributes.damping, _attributes.speed);
			zero[i] = _newX;
			_attributes.springVelocity[i] = _newV;
		}
		_attributes.lastPosition = _targetPosition + zero;
		if (_attributes.clamp && Vector3.Distance(_attributes.lastPosition, _targetPosition) > _attributes.maxDistance)
		{
			_attributes.lastPosition = _targetPosition + (_attributes.lastPosition - _targetPosition).normalized * _attributes.maxDistance;
		}
		return _targetPosition + zero;
	}

	public static void DampedSpringGeneralSolution(out float _newX, out float _newV, float _currentX, float _currentV, float _time, float _criticality, float _naturalFrequency)
	{
		if (_criticality < 0f)
		{
			_criticality = 0f;
		}
		if (_naturalFrequency <= 0f)
		{
			_naturalFrequency = 1f;
		}
		if (_criticality == 1f)
		{
			float num = _naturalFrequency * _time;
			float num2 = Mathf.Exp(0f - num);
			float num3 = _currentV + _naturalFrequency * _currentX;
			_newX = num2 * (_currentX + num3 * _time);
			_newV = num2 * (num3 * (1f - num) - _naturalFrequency * _currentX);
		}
		else if (_criticality < 1f)
		{
			float num4 = _naturalFrequency * Mathf.Sqrt(1f - _criticality * _criticality);
			float num5 = _criticality * _naturalFrequency;
			float num6 = 1f / num4 * (num5 * _currentX + _currentV);
			float num7 = Mathf.Exp((0f - num5) * _time);
			float f = num4 * _time;
			float num8 = Mathf.Cos(f);
			float num9 = Mathf.Sin(f);
			_newX = num7 * (_currentX * num8 + num6 * num9);
			_newV = num7 * (num8 * (num6 * num4 - num5 * _currentX) - num9 * (_currentX * num4 + num6 * num5));
		}
		else
		{
			float num10 = Mathf.Sqrt(_criticality * _criticality - 1f);
			float num11 = _naturalFrequency * (num10 - _criticality);
			float num12 = (0f - _naturalFrequency) * (num10 + _criticality);
			float num13 = (num11 * _currentX - _currentV) / (num11 - num12);
			float num14 = _currentX - num13;
			float num15 = Mathf.Exp(num11 * _time);
			float num16 = Mathf.Exp(num12 * _time);
			float num17 = num14 * num15;
			float num18 = num13 * num16;
			_newX = num17 + num18;
			_newV = num11 * num17 + num12 * num18;
		}
	}

	public static void DampedSpringGeneralSolution(out Quaternion _newX, out Vector3 _newV, Quaternion _currentX, Vector3 _currentV, float _time, float _criticality, float _naturalFrequency)
	{
		if (_criticality < 0f)
		{
			_criticality = 0f;
		}
		if (_naturalFrequency <= 0f)
		{
			_naturalFrequency = 1f;
		}
		if (_criticality == 1f)
		{
			float num = _naturalFrequency * _time;
			float num2 = Mathf.Exp(0f - num);
			Vector3 vector = _currentV + ToAngularVelocity(_currentX, 1f / _naturalFrequency);
			_newX = QuaternionScale(ToQuaternionFromAngularVelocityAndTime(vector, _time) * _currentX, num2);
			_newV = num2 * (vector * (1f - num) - ToAngularVelocity(_currentX, 1f / _naturalFrequency));
		}
		else if (_criticality < 1f)
		{
			float num3 = _naturalFrequency * Mathf.Sqrt(1f - _criticality * _criticality);
			float num4 = _criticality * _naturalFrequency;
			Vector3 vector2 = 1f / num3 * (ToAngularVelocity(_currentX, 1f / num4) + _currentV);
			float num5 = Mathf.Exp((0f - num4) * _time);
			float f = num3 * _time;
			float num6 = Mathf.Cos(f);
			float num7 = Mathf.Sin(f);
			_newX = QuaternionScale(ToQuaternionFromAngularVelocityAndTime(vector2, num7) * QuaternionScale(_currentX, num6), num5);
			_newV = num5 * (num6 * (vector2 * num3 - ToAngularVelocity(_currentX, 1f / num4)) - num7 * (ToAngularVelocity(_currentX, 1f / num3) + vector2 * num4));
		}
		else
		{
			float num8 = Mathf.Sqrt(_criticality * _criticality - 1f);
			float num9 = _naturalFrequency * (num8 - _criticality);
			float num10 = (0f - _naturalFrequency) * (num8 + _criticality);
			Vector3 vector3 = (ToAngularVelocity(_currentX, 1f / num9) - _currentV) / (num9 - num10);
			Quaternion quaternion = _currentX * Conjugate(ToQuaternionFromAngularVelocityAndTime(vector3, 1f));
			float num11 = Mathf.Exp(num9 * _time);
			float num12 = Mathf.Exp(num10 * _time);
			Quaternion quaternion2 = QuaternionScale(quaternion, num11);
			Vector3 vector4 = ToAngularVelocity(quaternion, 1f / num11);
			Vector3 vector5 = vector3 * num12;
			Quaternion quaternion3 = ToQuaternionFromAngularVelocityAndTime(vector3, num12);
			_newX = quaternion3 * quaternion2;
			_newV = num9 * vector4 + num10 * vector5;
		}
	}

	public static Vector3 ToAngularVelocity(Quaternion _dQ, float _dT)
	{
		ToAngleAndAxis(out var _angleRadians, out var _axis, _dQ);
		return _angleRadians / _dT * _axis;
	}

	public static void ToAngleAndAxis(out float _angleRadians, out Vector3 _axis, Quaternion _Q)
	{
		float num = Mathf.Sqrt(Quaternion.Dot(_Q, _Q));
		_Q.x /= num;
		_Q.y /= num;
		_Q.z /= num;
		_Q.w /= num;
		_axis = new Vector3(_Q.x, _Q.y, _Q.z);
		float magnitude = _axis.magnitude;
		if (Mathf.Abs(_Q.w) > 0.99f)
		{
			_angleRadians = 2f * Mathf.Asin(magnitude);
			if (magnitude == 0f)
			{
				_axis = new Vector3(1f, 0f, 0f);
			}
			else
			{
				_axis /= magnitude;
			}
		}
		else
		{
			_angleRadians = 2f * Mathf.Acos(_Q.w);
			_axis /= magnitude;
		}
	}

	public static Quaternion Conjugate(Quaternion q)
	{
		return new Quaternion(0f - q.x, 0f - q.y, 0f - q.z, q.w);
	}

	public static Quaternion QuaternionScale(Quaternion _Q, float _power)
	{
		ToAngleAndAxis(out var _angleRadians, out var _axis, _Q);
		return ToQuaternion(_angleRadians * _power, _axis);
	}

	public static Quaternion ToQuaternion(float _angleRadians, Vector3 _axis)
	{
		Vector3 normalized = _axis.normalized;
		float num = Mathf.Sin(_angleRadians * 0.5f);
		return new Quaternion(normalized.x * num, normalized.y * num, normalized.z * num, Mathf.Cos(_angleRadians * 0.5f));
	}

	public static Quaternion ToQuaternionFromAngularVelocityAndTime(Vector3 _omega, float _time)
	{
		float num = _omega.magnitude * _time;
		if (Mathf.Abs(num) > 1E-15f)
		{
			Vector3 normalized = _omega.normalized;
			float num2 = Mathf.Sin(num * 0.5f);
			return new Quaternion(normalized.x * num2, normalized.y * num2, normalized.z * num2, Mathf.Cos(num * 0.5f));
		}
		return Quaternion.identity;
	}

	public static void Log(object message, GameObject gameObject, Color? color = null)
	{
	}

	public static void DoNotLookEffect(GameObject _gameObject, bool _vignette = true, bool _zoom = true, bool _saturation = true, bool _contrast = true, bool _shake = true, bool _glitch = true)
	{
		float speedIn = 3f;
		float speedOut = 1f;
		if (_vignette)
		{
			PostProcessing.Instance.VignetteOverride(new Color(0.16f, 0.2f, 0.26f), 0.5f, 1f, speedIn, speedOut, 0.1f, _gameObject);
		}
		if (_zoom)
		{
			CameraZoom.Instance.OverrideZoomSet(65f, 0.1f, speedIn, speedOut, _gameObject, 150);
		}
		if (_saturation)
		{
			PostProcessing.Instance.SaturationOverride(-25f, speedIn, speedOut, 0.1f, _gameObject);
		}
		if (_contrast)
		{
			PostProcessing.Instance.ContrastOverride(10f, speedIn, speedOut, 0.1f, _gameObject);
		}
		if (_shake)
		{
			GameDirector.instance.CameraImpact.Shake(15f * Time.deltaTime, 0.1f);
			GameDirector.instance.CameraShake.Shake(15f * Time.deltaTime, 1f);
		}
		if (_glitch)
		{
			CameraGlitch.Instance.DoNotLookEffectSet();
		}
	}

	public static void CameraOverrideStopAim()
	{
		CameraAim.Instance.OverrideAimStop();
	}

	public static ExtractionPoint ExtractionPointGetNearest(Vector3 position)
	{
		ExtractionPoint result = null;
		float num = float.PositiveInfinity;
		foreach (GameObject extractionPoint in RoundDirector.instance.extractionPointList)
		{
			float num2 = Vector3.Distance(position, extractionPoint.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = extractionPoint.GetComponent<ExtractionPoint>();
			}
		}
		return result;
	}

	public static ExtractionPoint ExtractionPointGetNearestNotActivated(Vector3 position)
	{
		ExtractionPoint result = null;
		float num = float.PositiveInfinity;
		foreach (GameObject extractionPoint in RoundDirector.instance.extractionPointList)
		{
			if (extractionPoint.GetComponent<ExtractionPoint>().currentState == ExtractionPoint.State.Idle)
			{
				float num2 = Vector3.Distance(position, extractionPoint.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = extractionPoint.GetComponent<ExtractionPoint>();
				}
			}
		}
		return result;
	}

	public static float Remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
	{
		float t = Mathf.InverseLerp(origFrom, origTo, value);
		return Mathf.Lerp(targetFrom, targetTo, t);
	}

	public static bool InputDown(InputKey key)
	{
		if (Application.isEditor && (key == InputKey.Back || key == InputKey.Menu))
		{
			key = InputKey.BackEditor;
		}
		return InputManager.instance.KeyDown(key);
	}

	public static bool InputUp(InputKey key)
	{
		return InputManager.instance.KeyUp(key);
	}

	public static bool InputHold(InputKey key)
	{
		return InputManager.instance.KeyHold(key);
	}

	public static Vector2 InputMousePosition()
	{
		return InputManager.instance.GetMousePosition();
	}

	public static Vector2 InputMovement()
	{
		return InputManager.instance.GetMovement();
	}

	public static float InputMovementX()
	{
		return InputManager.instance.GetMovementX();
	}

	public static float InputMovementY()
	{
		return InputManager.instance.GetMovementY();
	}

	public static float InputScrollY()
	{
		return InputManager.instance.GetScrollY();
	}

	public static float InputMouseX()
	{
		return InputManager.instance.GetMouseX();
	}

	public static float InputMouseY()
	{
		return InputManager.instance.GetMouseY();
	}

	public static void InputDisableMovement()
	{
		InputManager.instance.DisableMovement();
	}

	public static void InputDisableAiming()
	{
		InputManager.instance.DisableAiming();
	}
}
