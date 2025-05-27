using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuElementSaveFile : MonoBehaviour
{
	public Image fadePanel;

	private MenuElementHover menuElementHover;

	private float initialFadeAlpha;

	private MenuPageSaves parentPageSaves;

	internal string saveFileName;

	public TextMeshProUGUI saveFileHeader;

	public TextMeshProUGUI saveFileHeaderLevel;

	public TextMeshProUGUI saveFileHeaderDate;

	public TextMeshProUGUI saveFileInfoRow1;

	private void Start()
	{
		menuElementHover = GetComponent<MenuElementHover>();
		initialFadeAlpha = fadePanel.color.a;
		parentPageSaves = GetComponentInParent<MenuPageSaves>();
	}

	private void Update()
	{
		if (menuElementHover.isHovering)
		{
			Color color = fadePanel.color;
			color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * 10f);
			fadePanel.color = color;
			if (SemiFunc.InputDown(InputKey.Confirm) || SemiFunc.InputDown(InputKey.Grab))
			{
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm);
				parentPageSaves.SaveFileSelected(saveFileName);
			}
		}
		else
		{
			Color color2 = fadePanel.color;
			color2.a = Mathf.Lerp(color2.a, initialFadeAlpha, Time.deltaTime * 10f);
			fadePanel.color = color2;
		}
	}
}
