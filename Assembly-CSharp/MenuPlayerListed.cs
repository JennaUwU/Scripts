using TMPro;
using UnityEngine;

public class MenuPlayerListed : MonoBehaviour
{
	internal PlayerAvatar playerAvatar;

	internal int listSpot;

	public TextMeshProUGUI playerName;

	public MenuPlayerHead playerHead;

	private RectTransform parentTransform;

	private Vector3 midScreenFocus;

	public TextMeshProUGUI pingText;

	private bool localFetch;

	internal bool isLocal;

	public bool isSpectate = true;

	public GameObject leftCrown;

	public GameObject rightCrown;

	private bool fetchCrown;

	public bool forceCrown;

	private bool crownSetterWasHere;

	private void Start()
	{
		parentTransform = base.transform.parent.GetComponent<RectTransform>();
		playerHead.focusPoint.SetParent(parentTransform);
		playerHead.myFocusPoint.SetParent(parentTransform);
		midScreenFocus = new Vector3(MenuManager.instance.screenUIWidth / 2, MenuManager.instance.screenUIHeight / 2, 0f) - parentTransform.localPosition - parentTransform.parent.GetComponent<RectTransform>().localPosition;
		if (forceCrown)
		{
			leftCrown.SetActive(value: true);
			rightCrown.SetActive(value: true);
			ForcePlayer(Arena.instance.winnerPlayer);
			TextMeshProUGUI componentInChildren = GetComponentInChildren<TextMeshProUGUI>();
			if ((bool)componentInChildren && (bool)playerAvatar)
			{
				componentInChildren.text = playerAvatar.playerName;
			}
		}
	}

	public void ForcePlayer(PlayerAvatar _playerAvatar)
	{
		playerHead.SetPlayer(_playerAvatar);
		playerAvatar = _playerAvatar;
		localFetch = false;
	}

	private void Update()
	{
		if (SemiFunc.FPSImpulse5() && !crownSetterWasHere && (bool)PlayerCrownSet.instance && PlayerCrownSet.instance.crownOwnerFetched)
		{
			if ((bool)playerAvatar && PlayerCrownSet.instance.crownOwnerSteamID == playerAvatar.steamID)
			{
				leftCrown.SetActive(value: true);
				rightCrown.SetActive(value: true);
			}
			crownSetterWasHere = true;
		}
		if (!localFetch)
		{
			if ((bool)playerAvatar)
			{
				isLocal = playerAvatar.isLocal;
			}
			localFetch = true;
		}
		if (!forceCrown && playerHead.myFocusPoint.localPosition != midScreenFocus)
		{
			playerHead.myFocusPoint.localPosition = midScreenFocus;
		}
		if ((bool)playerAvatar)
		{
			if (!fetchCrown)
			{
				if (SessionManager.instance.CrownedPlayerGet() == playerAvatar)
				{
					leftCrown.SetActive(value: true);
					rightCrown.SetActive(value: true);
				}
				fetchCrown = true;
			}
			if (isSpectate && playerName.text != playerAvatar.playerName)
			{
				playerName.text = playerAvatar.playerName;
			}
			if (playerAvatar.voiceChatFetched && playerAvatar.voiceChat.isTalking)
			{
				Color color = new Color(0.6f, 0.6f, 0.4f);
				playerName.color = Color.Lerp(playerName.color, color, Time.deltaTime * 10f);
			}
			else
			{
				Color color2 = new Color(0.2f, 0.2f, 0.2f);
				playerName.color = Color.Lerp(playerName.color, color2, Time.deltaTime * 10f);
			}
		}
		if (!forceCrown)
		{
			if (RunManager.instance.levelCurrent != RunManager.instance.levelLobbyMenu)
			{
				base.transform.localPosition = new Vector3(-23f, -listSpot * 22, 0f);
			}
			else
			{
				base.transform.localPosition = new Vector3(0f, -listSpot * 32, 0f);
			}
		}
	}

	public void MenuPlayerListedOutro()
	{
		Object.Destroy(base.gameObject);
	}
}
