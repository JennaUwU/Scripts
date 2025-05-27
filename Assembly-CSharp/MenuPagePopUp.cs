using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuPagePopUp : MonoBehaviour
{
	public static MenuPagePopUp instance;

	internal MenuPage menuPage;

	internal UnityEvent option1Event;

	internal UnityEvent option2Event;

	public TextMeshProUGUI bodyTextMesh;

	public MenuButton okButton;

	private void Start()
	{
		instance = this;
		menuPage = GetComponent<MenuPage>();
	}

	public void ButtonEvent()
	{
		MenuManager.instance.PageReactivatePageUnderThisPage(menuPage);
		MenuManager.instance.MenuEffectPopUpClose();
		menuPage.PageStateSet(MenuPage.PageState.Closing);
	}
}
