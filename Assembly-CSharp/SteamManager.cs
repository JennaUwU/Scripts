using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
	[Serializable]
	public class Developer
	{
		public string name;

		public string steamID;
	}

	public static SteamManager instance;

	internal Lobby currentLobby;

	internal Lobby noLobby;

	internal bool joinLobby;

	public GameObject networkConnectPrefab;

	internal AuthTicket steamAuthTicket;

	[Space]
	public List<Developer> developerList;

	internal bool developerMode;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			try
			{
				SteamClient.Init(3241660u);
			}
			catch (Exception ex)
			{
				Debug.LogError("Steamworks failed to initialize. Error: " + ex.Message);
			}
			Debug.Log("STEAM ID: " + SteamClient.SteamId.ToString());
			if (!Debug.isDebugBuild)
			{
				return;
			}
			{
				foreach (Developer developer in developerList)
				{
					if (SteamClient.SteamId.ToString() == developer.steamID)
					{
						Debug.Log("DEVELOPER MODE: " + developer.name.ToUpper());
						developerMode = true;
					}
				}
				return;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnEnable()
	{
		SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
		SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
		SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeft;
		SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
		SteamFriends.OnGameOverlayActivated += OnGameOverlayActivated;
	}

	private void Start()
	{
		GetSteamAuthTicket(out steamAuthTicket);
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs.Length < 2)
		{
			return;
		}
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (commandLineArgs[i].ToLower() == "+connect_lobby")
			{
				if (ulong.TryParse(commandLineArgs[i + 1], out var result) && result != 0)
				{
					Debug.Log("Auto-Connecting to lobby: " + result);
					OnGameLobbyJoinRequested(new Lobby(result), SteamClient.SteamId);
				}
				break;
			}
		}
	}

	private void OnLobbyMemberJoined(Lobby _lobby, Friend _friend)
	{
		Debug.Log("Steam: Lobby member joined: " + _friend.Name);
		MenuPageLobby.instance.JoiningPlayer(_friend.Name);
	}

	private void OnLobbyMemberLeft(Lobby _lobby, Friend _friend)
	{
		Debug.Log("Steam: Lobby member left: " + _friend.Name);
	}

	private void OnLobbyMemberDataChanged(Lobby _lobby, Friend _friend)
	{
		Debug.Log(" ");
		Debug.Log("Steam: Lobby member data changed for: " + _friend.Name);
		Debug.Log("I am " + SteamClient.Name);
		Debug.Log("Current Owner: " + _lobby.Owner.Name);
		if (PhotonNetwork.IsMasterClient && RunManager.instance.masterSwitched && (ulong)SteamClient.SteamId == (ulong)_lobby.Owner.Id)
		{
			Debug.Log("I am the new owner and i am locking the lobby.");
			LockLobby();
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			CancelSteamAuthTicket();
			SteamClient.Shutdown();
		}
	}

	private async void OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamID)
	{
		if ((ulong)_lobby.Id == (ulong)currentLobby.Id)
		{
			Debug.Log("Steam: Already in this lobby.");
			return;
		}
		Debug.Log("Steam: Game lobby join requested: " + _lobby.Id.ToString());
		await SteamMatchmaking.JoinLobbyAsync(_lobby.Id);
		if (RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu)
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				player.OutroStartRPC();
			}
			RunManager.instance.lobbyJoin = true;
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
		}
		joinLobby = true;
	}

	private void OnLobbyEntered(Lobby _lobby)
	{
		currentLobby.Leave();
		currentLobby = _lobby;
		Debug.Log("Steam: Lobby entered with ID: " + _lobby.Id.ToString());
		Debug.Log("Steam: Region: " + _lobby.GetData("Region"));
	}

	private void OnLobbyCreated(Result _result, Lobby _lobby)
	{
		if (_result == Result.OK)
		{
			Debug.Log("Steam: Lobby created with ID: " + _lobby.Id.ToString());
			return;
		}
		Debug.LogError("Steam: Failed to create lobby. Error: " + _result);
		NetworkManager.instance.LeavePhotonRoom();
	}

	public async void HostLobby()
	{
		Debug.Log("Steam: Hosting lobby...");
		Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(6);
		if (!lobby.HasValue)
		{
			Debug.LogError("Lobby created but not correctly instantiated.");
			return;
		}
		lobby.Value.SetPrivate();
		lobby.Value.SetFriendsOnly();
		lobby.Value.SetJoinable(b: false);
	}

	public void LeaveLobby()
	{
		if (currentLobby.IsOwnedBy(SteamClient.SteamId))
		{
			Debug.Log("Steam: Leaving lobby... and ruining it for others.");
			currentLobby.SetData("BuildName", "");
		}
		else
		{
			Debug.Log("Steam: Leaving lobby...");
		}
		CancelSteamAuthTicket();
		currentLobby.Leave();
		currentLobby = noLobby;
	}

	public void UnlockLobby()
	{
		Debug.Log("Steam: Unlocking lobby...");
		currentLobby.SetPrivate();
		currentLobby.SetFriendsOnly();
		currentLobby.SetJoinable(b: true);
	}

	public void LockLobby()
	{
		Debug.Log("Steam: Locking lobby...");
		currentLobby.SetPrivate();
		currentLobby.SetFriendsOnly();
		currentLobby.SetJoinable(b: false);
	}

	public void SetLobbyData()
	{
		Debug.Log("Steam: Setting lobby data...");
		currentLobby.SetData("Region", PhotonNetwork.CloudRegion);
		currentLobby.SetData("BuildName", BuildManager.instance.version.title);
	}

	public void SendSteamAuthTicket()
	{
		Debug.Log("Sending Steam Auth Ticket...");
		string value = GetSteamAuthTicket(out steamAuthTicket);
		PhotonNetwork.AuthValues = new AuthenticationValues();
		PhotonNetwork.AuthValues.UserId = SteamClient.SteamId.ToString();
		PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Steam;
		PhotonNetwork.AuthValues.AddAuthParameter("ticket", value);
	}

	private string GetSteamAuthTicket(out AuthTicket ticket)
	{
		Debug.Log("Getting Steam Auth Ticket...");
		ticket = SteamUser.GetAuthSessionTicket();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ticket.Data.Length; i++)
		{
			stringBuilder.AppendFormat("{0:x2}", ticket.Data[i]);
		}
		return stringBuilder.ToString();
	}

	public void CancelSteamAuthTicket()
	{
		Debug.Log("Cancelling Steam Auth Ticket...");
		if (steamAuthTicket != null)
		{
			steamAuthTicket.Cancel();
		}
	}

	public void OpenSteamOverlayToLobby()
	{
		SteamFriends.OpenOverlay("friends");
	}

	private void OnGameOverlayActivated(bool obj)
	{
		InputManager.instance.ResetInput();
	}
}
