using UnityEngine;
using UnityEngine.UI;

public class MenuSelectionBox : MonoBehaviour
{
	public static MenuSelectionBox instance;

	internal Vector3 targetPosition;

	internal Vector3 targetScale;

	internal RawImage rawImage;

	internal RectTransform rectTransform;

	internal Vector3 originalPos;

	internal Vector3 originalScale;

	private float activeTargetTimer;

	private float prevPosTimer;

	private Vector3 prevPos;

	private Vector3 currentPos;

	private Vector3 pulsatePos;

	private float clickTimer;

	private Color flashColor;

	internal MenuPage menuPage;

	internal bool firstSelection = true;

	internal bool isInScrollBox;

	internal MenuScrollBox menuScrollBox;

	private void Start()
	{
		targetPosition = base.transform.localPosition;
		targetScale = base.transform.localScale * 100f;
		originalPos = base.transform.localPosition;
		originalScale = base.transform.localScale;
		rawImage = GetComponentInChildren<RawImage>();
		menuPage = GetComponentInParent<MenuPage>();
		menuPage.selectionBox = this;
		rectTransform = GetComponent<RectTransform>();
		isInScrollBox = false;
		menuScrollBox = GetComponentInParent<MenuScrollBox>();
		if ((bool)menuScrollBox)
		{
			isInScrollBox = true;
		}
		else
		{
			instance = this;
		}
		MenuManager.instance.SelectionBoxAdd(this);
	}

	private void Update()
	{
		if (menuPage.currentPageState != MenuPage.PageState.Active && !menuPage.addedPageOnTop)
		{
			rawImage.color = new Color(0.4f, 0.08f, 0.015f, 0f);
			RectTransform component = menuPage.GetComponent<RectTransform>();
			base.transform.localPosition = new Vector3(component.rect.width / 2f, component.rect.height / 2f, base.transform.localPosition.z);
			base.transform.localScale = new Vector3(0f, 0f, 1f);
			targetScale = base.transform.localScale * 100f;
			targetPosition = rectTransform.localPosition;
			activeTargetTimer = 0f;
			return;
		}
		if (prevPosTimer <= 0f)
		{
			prevPos = currentPos;
			currentPos = base.transform.localPosition - pulsatePos;
			prevPosTimer = 1f / 120f;
		}
		else
		{
			prevPosTimer -= Time.deltaTime;
		}
		rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPosition, 20f * Time.deltaTime);
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, targetScale / 100f, 20f * Time.deltaTime);
		if (activeTargetTimer > 0f)
		{
			float num = 0.5f;
			base.transform.localScale = base.transform.localScale + new Vector3(num * 0.01f, num * 0.01f, 1f) * Mathf.Sin(Time.time * 20f);
			base.transform.localPosition = base.transform.localPosition + pulsatePos;
			activeTargetTimer -= Time.deltaTime;
			Color color = new Color(0.08f, 0.2f, 0.4f, 0.75f);
			Color color2 = new Color(0.2f, 0.5f, 1f, 1f);
			if (Vector3.Distance(base.transform.localPosition, targetPosition) <= 5f)
			{
				prevPos = currentPos;
			}
			rawImage.color = Color.Lerp(color, color2, Vector3.Distance(prevPos, currentPos) * 0.5f);
		}
		else
		{
			Color color3 = new Color(0.4f, 0.08f, 0.015f, 0f);
			rawImage.color = Color.Lerp(rawImage.color, color3, 10f * Time.deltaTime);
		}
		ClickColorAnimate();
	}

	public void MenuSelectionBoxSetTarget(Vector3 pos, Vector3 scale, MenuPage parentPage, bool _isInScrollBox, MenuScrollBox _menuScrollBox, Vector2 customScale = default(Vector2))
	{
		if (_isInScrollBox != isInScrollBox || (_isInScrollBox && _menuScrollBox != menuScrollBox))
		{
			MenuSelectionBox menuSelectionBox = MenuManager.instance.SelectionBoxGetCorrect(parentPage, _menuScrollBox);
			if ((bool)menuSelectionBox)
			{
				MenuManager.instance.SetActiveSelectionBox(menuSelectionBox);
				parentPage.selectionBox = menuSelectionBox;
				menuSelectionBox.Reinstate();
				menuSelectionBox.MenuSelectionBoxSetTarget(pos, scale, parentPage, _isInScrollBox, _menuScrollBox, customScale);
			}
			return;
		}
		MenuManager.instance.SetActiveSelectionBox(this);
		if (instance != this)
		{
			Reinstate();
		}
		if (firstSelection)
		{
			firstSelection = false;
			base.transform.localPosition = pos;
			base.transform.localScale = Vector3.zero;
			targetPosition = pos;
			targetScale = scale;
		}
		else
		{
			pos = new Vector3(pos.x, pos.y, 0f);
			targetPosition = pos;
			targetScale = scale + new Vector3(customScale.x, customScale.y, 0f);
			float num = targetScale.y * 0.2f;
			targetScale += new Vector3(num, num, 0f);
			targetPosition += new Vector3(0f, 0f, 0f);
			activeTargetTimer = 0.2f;
		}
	}

	public void SetClick(Color color)
	{
		flashColor = color;
		clickTimer = 1f;
	}

	private void ClickColorAnimate()
	{
		if (!(clickTimer <= 0f))
		{
			Color color = flashColor;
			Color color2 = new Color(0.08f, 0.2f, 0.4f, 0.75f);
			rawImage.color = Color.Lerp(color, color2, 1f - clickTimer);
			clickTimer -= Time.deltaTime * 10f;
		}
	}

	private void OnEnable()
	{
		base.transform.localPosition = originalPos;
		base.transform.localScale = originalScale;
		targetScale = originalScale * 100f;
		targetPosition = originalPos;
	}

	private void OnDestroy()
	{
		MenuManager.instance.SelectionBoxRemove(this);
	}

	public void Reinstate()
	{
		instance = this;
	}
}
