using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks, IPunObservable
{
	public static NetworkManager instance;

	public float gameTime;

	private float syncInterval = 0.5f;

	private float lastSyncTime;

	public GameObject playerAvatarPrefab;

	private int instantiatedPlayerAvatars;

	private bool LoadingDone;

	internal bool leavePhotonRoom;

	private void Start()
	{
		instance = this;
		if (PhotonNetwork.IsMasterClient)
		{
			lastSyncTime = 0f;
		}
		if (GameManager.instance.gameMode != 1)
		{
			return;
		}
		PhotonNetwork.Instantiate(playerAvatarPrefab.name, Vector3.zero, Quaternion.identity, 0);
		PhotonNetwork.SerializationRate = 25;
		PhotonNetwork.SendRate = 25;
		bool flag = true;
		PhotonVoiceView[] array = Object.FindObjectsByType<PhotonVoiceView>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			PhotonNetwork.Instantiate("Voice", Vector3.zero, Quaternion.identity, 0);
		}
		base.photonView.RPC("PlayerSpawnedRPC", RpcTarget.All);
	}

	[PunRPC]
	public void PlayerSpawnedRPC()
	{
		instantiatedPlayerAvatars++;
	}

	[PunRPC]
	public void AllPlayerSpawnedRPC()
	{
		LevelGenerator.Instance.AllPlayersReady = true;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(lastSyncTime);
			stream.SendNext(instantiatedPlayerAvatars);
		}
		else
		{
			gameTime = (float)stream.ReceiveNext();
			instantiatedPlayerAvatars = (int)stream.ReceiveNext();
		}
	}

	private void Update()
	{
		if (GameManager.instance.gameMode != 1)
		{
			return;
		}
		if (PhotonNetwork.IsMasterClient)
		{
			if (!LoadingDone && instantiatedPlayerAvatars == PhotonNetwork.CurrentRoom.PlayerCount)
			{
				base.photonView.RPC("AllPlayerSpawnedRPC", RpcTarget.AllBuffered);
				LoadingDone = true;
			}
			gameTime += Time.deltaTime;
			if (Time.time - lastSyncTime > syncInterval)
			{
				lastSyncTime = gameTime;
			}
		}
		else
		{
			gameTime += Time.deltaTime;
		}
	}

	public void LeavePhotonRoom()
	{
		Debug.Log("Leave Photon");
		PhotonNetwork.Disconnect();
		SteamManager.instance.LeaveLobby();
		GameManager.instance.SetGameMode(0);
		leavePhotonRoom = false;
		if (RunManager.instance.levelCurrent == RunManager.instance.levelTutorial)
		{
			TutorialDirector.instance.EndTutorial();
		}
		StartCoroutine(RunManager.instance.LeaveToMainMenu());
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		Debug.Log("Player left room: " + otherPlayer.NickName);
	}

	public override void OnMasterClientSwitched(Player _newMasterClient)
	{
		Debug.Log("Master client left...");
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			player.OutroStartRPC();
		}
		MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "Cause: Host disconnected", "Ok Dang");
		leavePhotonRoom = true;
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log($"Disconnected from server for reason {cause}");
		if (cause == DisconnectCause.DisconnectByClientLogic || cause == DisconnectCause.DisconnectByServerLogic)
		{
			return;
		}
		MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "Cause: " + cause, "Ok Dang");
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			player.OutroStartRPC();
		}
		leavePhotonRoom = true;
	}

	public void DestroyAll()
	{
		if (SemiFunc.IsMultiplayer())
		{
			Debug.Log("Destroyed all network objects.");
			PhotonNetwork.DestroyAll();
		}
	}
}
