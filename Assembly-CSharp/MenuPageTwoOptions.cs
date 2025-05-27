using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuPageTwoOptions : MonoBehaviour
{
	public static MenuPageTwoOptions instance;

	internal MenuPage menuPage;

	internal UnityEvent option1Event;

	internal UnityEvent option2Event;

	public TextMeshProUGUI bodyTextMesh;

	public MenuButton option1Button;

	public MenuButton option2Button;

	private void Start()
	{
		instance = this;
		menuPage = GetComponent<MenuPage>();
	}

	private void Update()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void ButtonEventOption1()
	{
		if (option1Event != null)
		{
			option1Event.Invoke();
		}
		MenuManager.instance.PageReactivatePageUnderThisPage(menuPage);
		menuPage.PageStateSet(MenuPage.PageState.Closing);
	}

	public void ButtonEventOption2()
	{
		if (option2Event != null)
		{
			option2Event.Invoke();
		}
		MenuManager.instance.PageReactivatePageUnderThisPage(menuPage);
		menuPage.PageStateSet(MenuPage.PageState.Closing);
	}
}
