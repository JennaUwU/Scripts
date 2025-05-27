using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPageSaves : MonoBehaviour
{
	public RectTransform saveFileInfo;

	public GameObject saveInfoDefault;

	public GameObject saveInfoSelected;

	public TextMeshProUGUI saveFileHeader;

	public TextMeshProUGUI saveFileHeaderDate;

	public TextMeshProUGUI saveFileInfoRow1;

	public TextMeshProUGUI saveFileInfoRow2;

	public TextMeshProUGUI saveFileInfoRow3;

	private Image saveFileInfoPanel;

	public RectTransform Scroller;

	public RectTransform saveFilePosition;

	public GameObject saveFilePrefab;

	internal string currentSaveFileName;

	internal List<MenuElementSaveFile> saveFiles = new List<MenuElementSaveFile>();

	internal float saveFileYOffset;

	public TextMeshProUGUI gameModeHeader;

	private void Start()
	{
		saveFileInfoPanel = saveFileInfo.GetComponentInChildren<Image>();
		List<string> list = StatsManager.instance.SaveFileGetAll();
		float num = 0f;
		foreach (string item in list)
		{
			GameObject gameObject = Object.Instantiate(saveFilePrefab, Scroller);
			gameObject.transform.localPosition = saveFilePosition.localPosition;
			gameObject.transform.SetSiblingIndex(3);
			MenuElementSaveFile component = gameObject.GetComponent<MenuElementSaveFile>();
			component.saveFileName = item;
			string text = StatsManager.instance.SaveFileGetTeamName(item);
			string text2 = StatsManager.instance.SaveFileGetDateAndTime(item);
			int num2 = int.Parse(StatsManager.instance.SaveFileGetRunLevel(item)) + 1;
			component.saveFileHeaderDate.text = text2;
			string text3 = ColorUtility.ToHtmlStringRGB(SemiFunc.ColorDifficultyGet(1f, 10f, num2));
			float time = StatsManager.instance.SaveFileGetTimePlayed(item);
			component.saveFileHeaderLevel.text = "<sprite name=truck> <color=#" + text3 + ">" + num2 + "</color>";
			component.saveFileHeader.text = text;
			Color numberColor = new Color(0.1f, 0.4f, 0.8f);
			Color unitColor = new Color(0.05f, 0.3f, 0.6f);
			component.saveFileInfoRow1.text = "<sprite name=clock>  " + SemiFunc.TimeToString(time, fancy: true, numberColor, unitColor);
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y + num, gameObject.transform.localPosition.z);
			float num3 = gameObject.GetComponent<RectTransform>().rect.height + 2f;
			num -= num3;
			saveFileYOffset = num3;
			saveFiles.Add(gameObject.GetComponent<MenuElementSaveFile>());
		}
		if (SemiFunc.MainMenuIsMultiplayer())
		{
			gameModeHeader.text = "Multiplayer mode";
		}
		else
		{
			gameModeHeader.text = "Singleplayer mode";
		}
	}

	public void OnGoBack()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Main);
	}

	private void Update()
	{
		if (saveFileInfoPanel.color != new Color(0f, 0f, 0f, 1f))
		{
			saveFileInfoPanel.color = Color.Lerp(saveFileInfoPanel.color, new Color(0f, 0f, 0f, 1f), Time.deltaTime * 10f);
		}
	}

	public void OnNewGame()
	{
		if (saveFiles.Count >= 10)
		{
			MenuManager.instance.PageCloseAllAddedOnTop();
			MenuManager.instance.PagePopUp("Save file limit reached", Color.red, "You can only have 10 save files at a time. Please delete some save files to make room for new ones.", "OK");
		}
		else if (SemiFunc.MainMenuIsMultiplayer())
		{
			SemiFunc.MenuActionHostGame();
		}
		else
		{
			SemiFunc.MenuActionSingleplayerGame();
		}
	}

	public void OnLoadGame()
	{
		if (SemiFunc.MainMenuIsMultiplayer())
		{
			SemiFunc.MenuActionHostGame(currentSaveFileName);
		}
		else
		{
			SemiFunc.MenuActionSingleplayerGame(currentSaveFileName);
		}
	}

	public void OnDeleteGame()
	{
		SemiFunc.SaveFileDelete(currentSaveFileName);
		bool flag = false;
		foreach (MenuElementSaveFile saveFile in saveFiles)
		{
			if (flag && (bool)saveFile)
			{
				RectTransform component = saveFile.GetComponent<RectTransform>();
				component.localPosition = new Vector3(component.localPosition.x, component.localPosition.y + saveFileYOffset, component.localPosition.z);
				MenuElementAnimations component2 = saveFile.GetComponent<MenuElementAnimations>();
				component2.UIAniNudgeY();
				component2.UIAniRotate();
				component2.UIAniNewInitialPosition(new Vector2(component.localPosition.x, component.localPosition.y));
			}
			if (saveFile.saveFileName == currentSaveFileName)
			{
				Object.Destroy(saveFile.gameObject);
				flag = true;
			}
		}
		saveFiles.RemoveAll((MenuElementSaveFile x) => x == null);
		GoBackToDefaultInfo();
	}

	public void GoBackToDefaultInfo()
	{
		MenuElementAnimations component = saveFileInfo.GetComponent<MenuElementAnimations>();
		component.UIAniNudgeX();
		component.UIAniRotate();
		saveInfoDefault.SetActive(value: true);
		saveInfoSelected.SetActive(value: false);
		saveFileInfoPanel.color = new Color(0.45f, 0f, 0f, 1f);
	}

	private void InfoPlayerNames(TextMeshProUGUI _textMesh, string _fileName)
	{
		_textMesh.text = "";
		List<string> list = StatsManager.instance.SaveFileGetPlayerNames(_fileName);
		list.Sort((string text, string text2) => text.Length.CompareTo(text2.Length));
		if (list != null)
		{
			int count = list.Count;
			int num = 0;
			foreach (string item in list)
			{
				if (num == count - 1)
				{
					_textMesh.text += item;
				}
				else if (num == count - 2)
				{
					_textMesh.text = _textMesh.text + item + "<color=#444444>   and   </color>";
				}
				else
				{
					_textMesh.text = _textMesh.text + item + "<color=#444444>,</color>   ";
				}
				num++;
			}
		}
		if (list == null || (list != null && list.Count == 0))
		{
			_textMesh.text += "You did it all alone!";
		}
	}

	public void SaveFileSelected(string saveFileName)
	{
		MenuElementAnimations component = saveFileInfo.GetComponent<MenuElementAnimations>();
		component.UIAniNudgeX();
		component.UIAniRotate();
		saveInfoDefault.SetActive(value: false);
		saveInfoSelected.SetActive(value: true);
		saveFileInfoPanel.color = new Color(0f, 0.1f, 0.25f, 1f);
		string text = StatsManager.instance.SaveFileGetTeamName(saveFileName);
		string text2 = StatsManager.instance.SaveFileGetDateAndTime(saveFileName);
		saveFileHeader.text = text;
		saveFileHeaderDate.text = text2;
		currentSaveFileName = saveFileName;
		string text3 = "      ";
		float time = StatsManager.instance.SaveFileGetTimePlayed(saveFileName);
		int num = int.Parse(StatsManager.instance.SaveFileGetRunLevel(saveFileName)) + 1;
		string text4 = ColorUtility.ToHtmlStringRGB(SemiFunc.ColorDifficultyGet(1f, 10f, num));
		string text5 = StatsManager.instance.SaveFileGetRunCurrency(saveFileName);
		saveFileInfoRow1.text = "<sprite name=truck>  <color=#" + text4 + "><b>" + num + "</b></color>";
		saveFileInfoRow1.text += text3;
		TextMeshProUGUI textMeshProUGUI = saveFileInfoRow1;
		textMeshProUGUI.text = textMeshProUGUI.text + "<sprite name=clock>  " + SemiFunc.TimeToString(time, fancy: true, new Color(0.1f, 0.4f, 0.8f), new Color(0.05f, 0.3f, 0.6f));
		saveFileInfoRow1.text += text3;
		string text6 = ColorUtility.ToHtmlStringRGB(new Color(0.2f, 0.5f, 0.3f));
		TextMeshProUGUI textMeshProUGUI2 = saveFileInfoRow1;
		textMeshProUGUI2.text = textMeshProUGUI2.text + "<sprite name=$$>  <b>" + text5 + "</b><color=#" + text6 + ">k</color>";
		string text7 = SemiFunc.DollarGetString(int.Parse(StatsManager.instance.SaveFileGetTotalHaul(saveFileName)));
		saveFileInfoRow2.text = "<color=#" + text6 + "><sprite name=$$$> TOTAL HAUL:      <b></b>$ </color><b>" + text7 + "</b><color=#" + text6 + ">k</color>";
		InfoPlayerNames(saveFileInfoRow3, saveFileName);
	}
}
