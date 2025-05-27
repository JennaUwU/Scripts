using Photon.Pun;
using UnityEngine;

public class LobbyMenuOpen : MonoBehaviour
{
	public static LobbyMenuOpen instance;

	public float timer = 2f;

	private bool opened;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (opened)
		{
			return;
		}
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			GameDirector.instance.CameraShake.Shake(0.25f, 0.05f);
			GameDirector.instance.CameraImpact.Shake(0.25f, 0.05f);
			MenuManager.instance.PageOpen(MenuPageIndex.Lobby);
			if (SemiFunc.IsMasterClient())
			{
				PhotonNetwork.CurrentRoom.IsOpen = true;
				SteamManager.instance.UnlockLobby();
			}
			opened = true;
		}
	}
}
