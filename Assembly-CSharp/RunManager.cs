using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
	public enum ChangeLevelType
	{
		Normal = 0,
		RunLevel = 1,
		Tutorial = 2,
		LobbyMenu = 3,
		MainMenu = 4,
		Shop = 5,
		Recording = 6
	}

	public enum SaveLevel
	{
		Lobby = 0,
		Shop = 1
	}

	public static RunManager instance;

	internal int saveLevel;

	internal int loadLevel;

	internal Level debugLevel;

	internal bool skipMainMenu;

	internal bool localMultiplayerTest;

	internal bool runStarted;

	internal RunManagerPUN runManagerPUN;

	public int levelsCompleted;

	public Level levelCurrent;

	internal Level levelPrevious;

	private Level previousRunLevel;

	internal bool restarting;

	internal bool restartingDone;

	internal int levelsMax = 10;

	[Space]
	public Level levelMainMenu;

	public Level levelLobbyMenu;

	public Level levelLobby;

	public Level levelShop;

	public Level levelTutorial;

	public Level levelRecording;

	public Level levelArena;

	public List<Level> levels;

	internal int runLives = 3;

	internal bool levelFailed;

	internal bool waitToChangeScene;

	internal bool lobbyJoin;

	internal bool masterSwitched;

	internal bool gameOver;

	internal bool allPlayersDead;

	[Space]
	public List<EnemySetup> enemiesSpawned;

	private List<EnemySetup> enemiesSpawnedToDelete = new List<EnemySetup>();

	internal bool skipLoadingUI = true;

	internal Color loadingFadeColor = Color.black;

	internal float loadingAnimationTime;

	internal List<PlayerVoiceChat> voiceChats = new List<PlayerVoiceChat>();

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		levelPrevious = levelCurrent;
	}

	private void Update()
	{
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (LevelGenerator.Instance.Generated && !SteamClient.IsValid && !SteamManager.instance.enabled)
		{
			Debug.LogError("Steam not initialized. Quitting game.");
			Application.Quit();
		}
		if (SemiFunc.DebugDev())
		{
			if (Input.GetKeyDown(KeyCode.F3))
			{
				if (SemiFunc.RunIsArena())
				{
					ChangeLevel(_completedLevel: true, _levelFailed: true);
				}
				else
				{
					ChangeLevel(_completedLevel: true, _levelFailed: false);
				}
			}
			if (!restarting && (bool)ChatManager.instance && !ChatManager.instance.chatActive && Input.GetKeyDown(KeyCode.Backspace))
			{
				ResetProgress();
				RestartScene();
				if (levelCurrent != levelTutorial)
				{
					SemiFunc.OnSceneSwitch(gameOver, _leaveGame: false);
				}
			}
		}
		if (restarting)
		{
			RestartScene();
		}
		if (restarting || !runStarted || GameDirector.instance.PlayerList.Count <= 0 || SemiFunc.RunIsArena() || GameDirector.instance.currentState != GameDirector.gameState.Main)
		{
			return;
		}
		bool flag = true;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (!player.isDisabled)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			allPlayersDead = true;
			if ((bool)SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Normal))
			{
				ChangeLevel(_completedLevel: false, _levelFailed: true);
			}
		}
	}

	private void OnApplicationQuit()
	{
		DataDirector.instance.SaveDeleteCheck(_leaveGame: true);
	}

	public void ChangeLevel(bool _completedLevel, bool _levelFailed, ChangeLevelType _changeLevelType = ChangeLevelType.Normal)
	{
		if ((!SemiFunc.MenuLevel() && !SemiFunc.IsMasterClientOrSingleplayer()) || restarting)
		{
			return;
		}
		gameOver = false;
		if (_levelFailed && levelCurrent != levelLobby && levelCurrent != levelShop)
		{
			if (levelCurrent == levelArena)
			{
				ResetProgress();
				if (SemiFunc.IsMultiplayer())
				{
					levelCurrent = levelLobbyMenu;
				}
				else
				{
					SetRunLevel();
				}
				gameOver = true;
			}
			else
			{
				levelCurrent = levelArena;
			}
		}
		if (!gameOver && levelCurrent != levelArena)
		{
			switch (_changeLevelType)
			{
			case ChangeLevelType.RunLevel:
				SetRunLevel();
				break;
			case ChangeLevelType.LobbyMenu:
				levelCurrent = levelLobbyMenu;
				break;
			case ChangeLevelType.MainMenu:
				levelCurrent = levelMainMenu;
				break;
			case ChangeLevelType.Tutorial:
				levelCurrent = levelTutorial;
				break;
			case ChangeLevelType.Recording:
				levelCurrent = levelRecording;
				break;
			case ChangeLevelType.Shop:
				levelCurrent = levelShop;
				break;
			default:
				if (levelCurrent == levelMainMenu || levelCurrent == levelLobbyMenu)
				{
					levelCurrent = levelLobby;
				}
				else if (_completedLevel && levelCurrent != levelLobby && levelCurrent != levelShop)
				{
					previousRunLevel = levelCurrent;
					levelsCompleted++;
					SemiFunc.StatSetRunLevel(levelsCompleted);
					SemiFunc.LevelSuccessful();
					levelCurrent = levelShop;
				}
				else if (levelCurrent == levelLobby)
				{
					SetRunLevel();
				}
				else if (levelCurrent == levelShop)
				{
					levelCurrent = levelLobby;
				}
				break;
			}
		}
		if ((bool)debugLevel && levelCurrent != levelMainMenu && levelCurrent != levelLobbyMenu)
		{
			levelCurrent = debugLevel;
		}
		if (GameManager.Multiplayer())
		{
			runManagerPUN.photonView.RPC("UpdateLevelRPC", RpcTarget.OthersBuffered, levelCurrent.name, levelsCompleted, gameOver);
		}
		Debug.Log("Changed level to: " + levelCurrent.name);
		if (levelCurrent == levelShop)
		{
			saveLevel = 1;
		}
		else
		{
			saveLevel = 0;
		}
		SemiFunc.StatSetSaveLevel(saveLevel);
		RestartScene();
		if (_changeLevelType != ChangeLevelType.Tutorial)
		{
			SemiFunc.OnSceneSwitch(gameOver, _leaveGame: false);
		}
	}

	public void RestartScene()
	{
		if (!restarting)
		{
			restarting = true;
			if (!GameDirector.instance)
			{
				return;
			}
			{
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
				{
					player.OutroStart();
				}
				return;
			}
		}
		if (restartingDone)
		{
			return;
		}
		bool flag = true;
		if (!GameDirector.instance)
		{
			flag = false;
		}
		else
		{
			foreach (PlayerAvatar player2 in GameDirector.instance.PlayerList)
			{
				if (!player2.outroDone)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		if (gameOver)
		{
			NetworkManager.instance.DestroyAll();
			gameOver = false;
		}
		if (lobbyJoin)
		{
			lobbyJoin = false;
			restartingDone = true;
			SceneManager.LoadSceneAsync("LobbyJoin");
		}
		else if (!waitToChangeScene)
		{
			restartingDone = true;
			if (!GameManager.Multiplayer())
			{
				SceneManager.LoadSceneAsync("Main");
			}
			else if (PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.LoadLevel("Reload");
			}
		}
	}

	public void UpdateLevel(string _levelName, int _levelsCompleted, bool _gameOver)
	{
		if ((bool)LobbyMenuOpen.instance)
		{
			DataDirector.instance.RunsPlayedAdd();
		}
		SemiFunc.OnSceneSwitch(_gameOver, _leaveGame: false);
		levelsCompleted = _levelsCompleted;
		SemiFunc.StatSetRunLevel(levelsCompleted);
		if (_levelName == levelLobbyMenu.name)
		{
			levelCurrent = levelLobbyMenu;
		}
		else if (_levelName == levelLobby.name)
		{
			levelCurrent = levelLobby;
		}
		else if (_levelName == levelShop.name)
		{
			levelCurrent = levelShop;
		}
		else if (_levelName == levelArena.name)
		{
			levelCurrent = levelArena;
		}
		else if (_levelName == levelRecording.name)
		{
			levelCurrent = levelRecording;
		}
		else
		{
			foreach (Level level in levels)
			{
				if (level.name == _levelName)
				{
					levelCurrent = level;
					break;
				}
			}
		}
		Debug.Log("updated level to: " + levelCurrent.name);
	}

	public void ResetProgress()
	{
		if ((bool)StatsManager.instance)
		{
			StatsManager.instance.ResetAllStats();
		}
		levelsCompleted = 0;
		loadLevel = 0;
	}

	public void EnemiesSpawnedRemoveStart()
	{
		enemiesSpawnedToDelete.Clear();
		foreach (EnemySetup item in enemiesSpawned)
		{
			bool flag = false;
			foreach (EnemySetup item2 in enemiesSpawnedToDelete)
			{
				if (item == item2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				enemiesSpawnedToDelete.Add(item);
			}
		}
	}

	public void EnemiesSpawnedRemoveEnd()
	{
		foreach (EnemySetup item in enemiesSpawnedToDelete)
		{
			enemiesSpawned.Remove(item);
		}
	}

	public void SetRunLevel()
	{
		levelCurrent = previousRunLevel;
		while (levelCurrent == previousRunLevel)
		{
			levelCurrent = levels[Random.Range(0, levels.Count)];
		}
	}

	public IEnumerator LeaveToMainMenu()
	{
		while (PhotonNetwork.NetworkingClient.State != ClientState.Disconnected && PhotonNetwork.NetworkingClient.State != ClientState.PeerCreated)
		{
			yield return null;
		}
		Debug.Log("Leave to Main Menu");
		SemiFunc.OnSceneSwitch(_gameOver: false, _leaveGame: true);
		levelCurrent = levelMainMenu;
		SceneManager.LoadSceneAsync("Reload");
		yield return null;
	}
}
