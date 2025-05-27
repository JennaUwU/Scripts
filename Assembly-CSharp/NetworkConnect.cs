using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkConnect : MonoBehaviourPunCallbacks
{
	public static NetworkConnect instance;

	private bool joinedRoom;

	private string RoomName;

	private bool ConnectedToMasterServer;

	public GameObject punVoiceClient;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		PhotonNetwork.NickName = SteamClient.Name;
		PhotonNetwork.AutomaticallySyncScene = false;
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";
		Object.Instantiate(punVoiceClient, Vector3.zero, Quaternion.identity);
		PhotonNetwork.Disconnect();
		StartCoroutine(CreateLobby());
	}

	private IEnumerator CreateLobby()
	{
		while (PhotonNetwork.NetworkingClient.State != ClientState.Disconnected && PhotonNetwork.NetworkingClient.State != ClientState.PeerCreated)
		{
			yield return null;
		}
		if (!GameManager.instance.localTest)
		{
			if (SteamManager.instance.currentLobby.Id.IsValid)
			{
				RoomName = SteamManager.instance.currentLobby.Id.ToString();
				PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = SteamManager.instance.currentLobby.GetData("Region");
				string data = SteamManager.instance.currentLobby.GetData("BuildName");
				if (data != BuildManager.instance.version.title)
				{
					if (data != "")
					{
						Debug.Log("Build name mismatch. Leaving lobby. Build name is ''" + data + "''");
						string bodyText = "Game lobby is using version\n<color=#FDFF00><b>" + data + "</b>";
						MenuManager.instance.PagePopUpScheduled("Wrong Game Version", Color.red, bodyText, "Ok Dang");
					}
					else
					{
						Debug.Log("Lobby closed. Leaving lobby.");
						MenuManager.instance.PagePopUpScheduled("Lobby Closed", Color.red, "The lobby has closed.", "Ok Dang");
					}
					PhotonNetwork.Disconnect();
					SteamManager.instance.LeaveLobby();
					GameManager.instance.SetGameMode(0);
					RunManager.instance.levelCurrent = RunManager.instance.levelMainMenu;
					SceneManager.LoadSceneAsync("Reload");
					yield break;
				}
				Debug.Log("Already in lobby on Network Connect. Connecting to region: " + PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion);
			}
			else
			{
				Debug.Log("Created lobby on Network Connect.");
				PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";
				SteamManager.instance.HostLobby();
				while (!SteamManager.instance.currentLobby.Id.IsValid)
				{
					yield return null;
				}
				RoomName = SteamManager.instance.currentLobby.Id.ToString();
			}
			SteamManager.instance.SendSteamAuthTicket();
		}
		else
		{
			Debug.Log("Local test mode.");
			RunManager.instance.ResetProgress();
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
			RoomName = SteamClient.Name;
		}
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master Server");
		if (!GameManager.instance.localTest && SteamManager.instance.currentLobby.Id.IsValid && SteamManager.instance.currentLobby.IsOwnedBy(SteamClient.SteamId))
		{
			Debug.Log("I am the owner.");
			SteamManager.instance.SetLobbyData();
			TryJoiningRoom();
		}
		else
		{
			Debug.Log("I am not the owner.");
			TryJoiningRoom();
		}
	}

	private void TryJoiningRoom()
	{
		Debug.Log("Trying to join room: " + RoomName);
		PhotonNetwork.JoinOrCreateRoom(RoomName, new RoomOptions
		{
			MaxPlayers = 6,
			IsVisible = false
		}, TypedLobby.Default);
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("Created room successfully: " + PhotonNetwork.CurrentRoom.Name);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CloudRegion);
		joinedRoom = true;
		PhotonNetwork.AutomaticallySyncScene = true;
		RunManager.instance.waitToChangeScene = false;
		if (GameManager.instance.localTest && PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.LoadLevel("Reload");
		}
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.LogError("Failed to create room: " + message);
		MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "Cause: " + message, "Ok Dang");
		PhotonNetwork.Disconnect();
		SteamManager.instance.LeaveLobby();
		GameManager.instance.SetGameMode(0);
		StartCoroutine(RunManager.instance.LeaveToMainMenu());
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogError("Failed to join room: " + message);
		MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "Cause: " + message, "Ok Dang");
		PhotonNetwork.Disconnect();
		SteamManager.instance.LeaveLobby();
		GameManager.instance.SetGameMode(0);
		StartCoroutine(RunManager.instance.LeaveToMainMenu());
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log($"Disconnected from server for reason {cause}");
		if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.DisconnectByServerLogic)
		{
			MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "Cause: " + cause, "Ok Dang");
			PhotonNetwork.Disconnect();
			SteamManager.instance.LeaveLobby();
			GameManager.instance.SetGameMode(0);
			StartCoroutine(RunManager.instance.LeaveToMainMenu());
		}
	}

	private void OnDestroy()
	{
		if (joinedRoom)
		{
			Debug.Log("Game Mode: Multiplayer");
			GameManager.instance.SetGameMode(1);
		}
		Debug.Log("NetworkConnect destroyed.");
		RunManager.instance.waitToChangeScene = false;
	}
}
