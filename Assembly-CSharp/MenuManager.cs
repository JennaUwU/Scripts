using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	[Serializable]
	public class MenuPages
	{
		public MenuPageIndex menuPageIndex;

		public GameObject menuPage;
	}

	public enum MenuState
	{
		Open = 0,
		Closed = 1
	}

	public enum MenuClickEffectType
	{
		Action = 0,
		Confirm = 1,
		Deny = 2,
		Dud = 3,
		Tick = 4
	}

	public static MenuManager instance;

	internal MenuSelectionBox selectionBox;

	internal MenuSelectionBox activeSelectionBox;

	internal List<MenuPlayerHead> playerHeads = new List<MenuPlayerHead>();

	private List<MenuSelectionBox> selectionBoxes = new List<MenuSelectionBox>();

	internal string currentMenuID = "";

	public List<MenuPages> menuPages;

	internal MenuPageIndex currentMenuPageIndex;

	internal MenuPage currentMenuPage;

	internal int currentMenuState;

	private bool stateStart;

	internal MenuButton currentButton;

	internal int fetchSetting;

	private Vector3 soundPosition;

	private float menuHover;

	public Sound soundAction;

	public Sound soundConfirm;

	public Sound soundDeny;

	public Sound soundDud;

	public Sound soundTick;

	public Sound soundHover;

	public Sound soundPageIntro;

	public Sound soundPageOutro;

	public Sound soundWindowPopUp;

	public Sound soundWindowPopUpClose;

	public Sound soundMove;

	internal Vector2 mouseHoldPosition;

	internal int screenUIWidth = 720;

	internal int screenUIHeight = 405;

	internal List<MenuPage> allPages = new List<MenuPage>();

	internal List<MenuPage> inactivePages = new List<MenuPage>();

	internal List<MenuPage> addedPagesOnTop = new List<MenuPage>();

	private bool pagePopUpScheduled;

	private string pagePopUpScheduledHeaderText;

	private Color pagePopUpScheduledHeaderColor;

	private string pagePopUpScheduledBodyText;

	private string pagePopUpScheduledButtonText;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		StateSet(MenuState.Closed);
	}

	private void Update()
	{
		if ((bool)PlayerController.instance)
		{
			soundPosition = PlayerController.instance.transform.position;
		}
		else
		{
			soundPosition = base.transform.position;
		}
		switch (currentMenuState)
		{
		case 0:
			StateOpen();
			stateStart = false;
			break;
		case 1:
			StateClosed();
			stateStart = false;
			break;
		}
		if (Input.GetMouseButton(0))
		{
			if (mouseHoldPosition == Vector2.zero)
			{
				mouseHoldPosition = SemiFunc.UIMousePosToUIPos();
			}
		}
		else
		{
			mouseHoldPosition = Vector2.zero;
		}
	}

	private void FixedUpdate()
	{
		if (menuHover > 0f)
		{
			menuHover -= Time.fixedDeltaTime;
		}
		else
		{
			currentMenuID = "";
		}
	}

	public void SetState(int state)
	{
		currentMenuState = state;
		stateStart = true;
	}

	public void MenuEffectHover(float pitch = -1f, float volume = -1f)
	{
		if (pitch != -1f)
		{
			soundHover.Pitch = pitch;
		}
		if (volume != -1f)
		{
			soundHover.Volume = volume;
		}
		soundHover.Play(base.transform.position);
	}

	public void MenuEffectClick(MenuClickEffectType effectType, MenuPage parentPage = null, float pitch = -1f, float volume = -1f, bool soundOnly = false)
	{
		switch (effectType)
		{
		case MenuClickEffectType.Action:
			if (!soundOnly && (bool)activeSelectionBox)
			{
				activeSelectionBox.SetClick(AssetManager.instance.colorYellow);
			}
			if (pitch != -1f)
			{
				soundAction.Pitch = pitch;
			}
			if (volume != -1f)
			{
				soundAction.Volume = volume;
			}
			soundAction.Play(soundPosition);
			break;
		case MenuClickEffectType.Confirm:
			if (!soundOnly && (bool)activeSelectionBox)
			{
				activeSelectionBox.SetClick(Color.green);
			}
			if (pitch != -1f)
			{
				soundConfirm.Pitch = pitch;
			}
			if (volume != -1f)
			{
				soundConfirm.Volume = volume;
			}
			soundConfirm.Play(soundPosition);
			break;
		case MenuClickEffectType.Deny:
			if (!soundOnly && (bool)activeSelectionBox)
			{
				activeSelectionBox.SetClick(Color.red);
			}
			if (pitch != -1f)
			{
				soundDeny.Pitch = pitch;
			}
			if (volume != -1f)
			{
				soundDeny.Volume = volume;
			}
			soundDeny.Play(soundPosition);
			break;
		case MenuClickEffectType.Dud:
			if (!soundOnly)
			{
				activeSelectionBox.SetClick(Color.gray);
			}
			if (pitch != -1f)
			{
				soundDud.Pitch = pitch;
			}
			if (volume != -1f)
			{
				soundDud.Volume = volume;
			}
			soundDud.Play(soundPosition);
			break;
		case MenuClickEffectType.Tick:
			if (!soundOnly)
			{
				Color click = new Color(0f, 0.5f, 1f, 1f);
				if (!parentPage)
				{
					if ((bool)MenuSelectionBox.instance)
					{
						MenuSelectionBox.instance.SetClick(click);
					}
				}
				else if ((bool)parentPage.selectionBox)
				{
					parentPage.selectionBox.SetClick(click);
				}
			}
			if (pitch != -1f)
			{
				soundTick.Pitch = pitch;
			}
			if (volume != -1f)
			{
				soundTick.Volume = volume;
			}
			soundTick.Play(soundPosition);
			break;
		}
	}

	public void MenuEffectPopUpOpen()
	{
		soundWindowPopUp.Play(soundPosition);
	}

	public void MenuEffectPopUpClose()
	{
		soundWindowPopUpClose.Play(soundPosition);
	}

	public void MenuEffectPageIntro()
	{
		soundPageIntro.Play(soundPosition);
	}

	public void MenuEffectPageOutro()
	{
		soundPageOutro.Play(soundPosition);
	}

	public void MenuEffectMove()
	{
		soundMove.Play(soundPosition);
	}

	private void StateOpen()
	{
		_ = stateStart;
		SemiFunc.CursorUnlock(0.1f);
		PlayerController.instance.InputDisableTimer = 0.1f;
		if (!currentMenuPage)
		{
			StateSet(MenuState.Closed);
		}
	}

	private void StateClosed()
	{
		_ = stateStart;
		if ((bool)currentMenuPage)
		{
			StateSet(MenuState.Open);
		}
	}

	public void PageAdd(MenuPage menuPage)
	{
		if (!allPages.Contains(menuPage))
		{
			allPages.Add(menuPage);
		}
	}

	public void PageRemove(MenuPage menuPage)
	{
		if (allPages.Contains(menuPage))
		{
			allPages.Remove(menuPage);
		}
	}

	public MenuPage PageOpen(MenuPageIndex menuPageIndex, bool addedPageOnTop = false)
	{
		MenuPages menuPages = this.menuPages.Find((MenuPages x) => x.menuPageIndex == menuPageIndex);
		if (menuPages == null)
		{
			Debug.LogError("Page not found");
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(menuPages.menuPage);
		MenuPage component = obj.GetComponent<MenuPage>();
		obj.transform.SetParent(MenuHolder.instance.transform);
		obj.GetComponent<RectTransform>().localPosition = new Vector2(0f, 0f);
		obj.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
		component.addedPageOnTop = addedPageOnTop;
		if (!addedPageOnTop)
		{
			instance.PageSetCurrent(menuPageIndex, component);
		}
		else
		{
			component.parentPage = currentMenuPage;
		}
		return component;
	}

	public void PageClose(MenuPageIndex menuPageIndex)
	{
		if (menuPages.Find((MenuPages x) => x.menuPageIndex == menuPageIndex) != null)
		{
			MenuPage menuPage = allPages.Find((MenuPage x) => x.menuPageIndex == menuPageIndex);
			if (!(menuPage == null))
			{
				menuPage.PageStateSet(MenuPage.PageState.Closing);
				allPages.Remove(menuPage);
			}
		}
	}

	public bool PageCheck(MenuPageIndex menuPageIndex)
	{
		return allPages.Find((MenuPage x) => x.menuPageIndex == menuPageIndex) != null;
	}

	public void PageSwap(MenuPageIndex menuPageIndex)
	{
		currentMenuPage.PageStateSet(MenuPage.PageState.Closing);
		PageOpen(menuPageIndex);
	}

	public MenuPage PageOpenOnTop(MenuPageIndex menuPageIndex)
	{
		MenuPage menuPage = currentMenuPage;
		PageInactiveAdd(menuPage);
		currentMenuPage.PageStateSet(MenuPage.PageState.Inactive);
		MenuPage menuPage2 = PageOpen(menuPageIndex);
		menuPage2.pageIsOnTopOfOtherPage = true;
		menuPage2.pageUnderThisPage = menuPage;
		return menuPage2;
	}

	public void PageAddOnTop(MenuPageIndex menuPageIndex)
	{
		if (!addedPagesOnTop.Contains(currentMenuPage))
		{
			_ = currentMenuPage;
			MenuPage item = PageOpen(menuPageIndex, addedPageOnTop: true);
			if (!addedPagesOnTop.Contains(currentMenuPage))
			{
				addedPagesOnTop.Add(item);
			}
		}
	}

	public void PagePopUpTwoOptions(MenuButtonPopUp menuButtonPopUp, string popUpHeader, Color popUpHeaderColor, string popUpText, string option1Text, string option2Text)
	{
		MenuPageIndex menuPageIndex = MenuPageIndex.PopUpTwoOptions;
		MenuPage pageUnderThisPage = currentMenuPage;
		currentMenuPage.PageStateSet(MenuPage.PageState.Inactive);
		MenuPage menuPage = PageOpen(menuPageIndex);
		menuPage.pageIsOnTopOfOtherPage = true;
		menuPage.pageUnderThisPage = pageUnderThisPage;
		MenuPageTwoOptions component = menuPage.GetComponent<MenuPageTwoOptions>();
		menuPage.menuHeaderName = popUpHeader;
		menuPage.menuHeader.text = popUpHeader;
		menuPage.menuHeader.color = popUpHeaderColor;
		component.option1Event = menuButtonPopUp.option1Event;
		component.option2Event = menuButtonPopUp.option2Event;
		component.bodyTextMesh.text = popUpText;
		component.option1Button.buttonTextString = option1Text;
		component.option2Button.buttonTextString = option2Text;
	}

	public void MenuHover()
	{
		menuHover = 0.1f;
	}

	public void PageSetCurrent(MenuPageIndex menuPageIndex, MenuPage menuPage)
	{
		currentMenuPageIndex = menuPageIndex;
		currentMenuPage = menuPage;
	}

	public void StateSet(MenuState state)
	{
		currentMenuState = (int)state;
	}

	private void PageInactiveAdd(MenuPage menuPage)
	{
		if (!inactivePages.Contains(menuPage))
		{
			inactivePages.Add(menuPage);
		}
	}

	private void PageInactiveRemove(MenuPage menuPage)
	{
		if (inactivePages.Contains(menuPage))
		{
			inactivePages.Remove(menuPage);
		}
	}

	public void PageReactivatePageUnderThisPage(MenuPage _menuPage)
	{
		if (!(currentMenuPage != _menuPage) && (bool)currentMenuPage.pageUnderThisPage)
		{
			if (currentMenuPage.pageUnderThisPage.currentPageState == MenuPage.PageState.Inactive)
			{
				currentMenuPage.pageUnderThisPage.PageStateSet(MenuPage.PageState.Activating);
			}
			PageSetCurrent(currentMenuPage.pageUnderThisPage.menuPageIndex, currentMenuPage.pageUnderThisPage);
		}
	}

	public void PageCloseAllExcept(MenuPageIndex menuPageIndex)
	{
		foreach (MenuPage allPage in allPages)
		{
			if (allPage.menuPageIndex != menuPageIndex)
			{
				allPage.PageStateSet(MenuPage.PageState.Closing);
			}
		}
	}

	public void PageCloseAll()
	{
		foreach (MenuPage allPage in allPages)
		{
			allPage.PageStateSet(MenuPage.PageState.Closing);
		}
	}

	public void PageCloseAllAddedOnTop()
	{
		foreach (MenuPage item in addedPagesOnTop)
		{
			item.PageStateSet(MenuPage.PageState.Closing);
		}
	}

	public void PlayerHeadAdd(MenuPlayerHead head)
	{
		if (!playerHeads.Contains(head))
		{
			playerHeads.Add(head);
		}
		for (int i = 0; i < playerHeads.Count; i++)
		{
			if (playerHeads[i] == null)
			{
				playerHeads.RemoveAt(i);
			}
		}
	}

	public void SetActiveSelectionBox(MenuSelectionBox selectBox)
	{
		activeSelectionBox = selectBox;
	}

	public void PlayerHeadRemove(MenuPlayerHead head)
	{
		if (playerHeads.Contains(head))
		{
			playerHeads.Remove(head);
		}
		for (int i = 0; i < playerHeads.Count; i++)
		{
			if (playerHeads[i] == null)
			{
				playerHeads.RemoveAt(i);
			}
		}
	}

	public void PagePopUpScheduled(string headerText, Color headerColor, string bodyText, string buttonText)
	{
		pagePopUpScheduled = true;
		pagePopUpScheduledHeaderText = headerText;
		pagePopUpScheduledHeaderColor = headerColor;
		pagePopUpScheduledBodyText = bodyText;
		pagePopUpScheduledButtonText = buttonText;
	}

	public void PagePopUpScheduledShow()
	{
		if (pagePopUpScheduled)
		{
			PagePopUp(pagePopUpScheduledHeaderText, pagePopUpScheduledHeaderColor, pagePopUpScheduledBodyText, pagePopUpScheduledButtonText);
			PagePopUpScheduledReset();
		}
	}

	public void PagePopUpScheduledReset()
	{
		pagePopUpScheduled = false;
	}

	public void SelectionBoxAdd(MenuSelectionBox selectBox)
	{
		if (!selectionBoxes.Contains(selectBox))
		{
			selectionBoxes.Add(selectBox);
		}
		for (int i = 0; i < selectionBoxes.Count; i++)
		{
			if (selectionBoxes[i] == null)
			{
				selectionBoxes.RemoveAt(i);
			}
		}
	}

	public void SelectionBoxRemove(MenuSelectionBox selectBox)
	{
		if (selectionBoxes.Contains(selectBox))
		{
			selectionBoxes.Remove(selectBox);
		}
		for (int i = 0; i < selectionBoxes.Count; i++)
		{
			if (selectionBoxes[i] == null)
			{
				selectionBoxes.RemoveAt(i);
			}
		}
	}

	public MenuSelectionBox SelectionBoxGetCorrect(MenuPage parentPage, MenuScrollBox menuScrollBox)
	{
		return selectionBoxes.Find((MenuSelectionBox x) => x.menuPage == parentPage && x.menuScrollBox == menuScrollBox);
	}

	public void PagePopUp(string headerText, Color headerColor, string bodyText, string buttonText)
	{
		MenuPageIndex menuPageIndex = MenuPageIndex.PopUp;
		MenuPage menuPage = currentMenuPage;
		if (currentMenuPage.menuPageIndex == MenuPageIndex.PopUpTwoOptions)
		{
			menuPage = (currentMenuPage = currentMenuPage.pageUnderThisPage);
		}
		menuPage.PageStateSet(MenuPage.PageState.Inactive);
		MenuPage menuPage2 = PageOpen(menuPageIndex);
		menuPage2.pageIsOnTopOfOtherPage = true;
		menuPage2.pageUnderThisPage = menuPage;
		MenuPagePopUp component = menuPage2.GetComponent<MenuPagePopUp>();
		menuPage2.menuHeaderName = headerText;
		menuPage2.menuHeader.text = headerText;
		menuPage2.menuHeader.color = headerColor;
		component.bodyTextMesh.text = bodyText;
		component.okButton.buttonTextString = buttonText;
		currentMenuPage = menuPage2;
		MenuEffectPopUpOpen();
	}
}
