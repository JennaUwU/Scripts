using TMPro;
using UnityEngine;

public class ItemInfoUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static ItemInfoUI instance;

	private string messagePrev = "prev";

	private float messageTimer;

	private GameObject bigMessageEmojiObject;

	private TextMeshProUGUI emojiText;

	private VertexGradient originalGradient;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		Text.text = "";
		originalGradient = Text.colorGradient;
	}

	public void ItemInfoText(ItemAttributes _itemAttributes, string message, bool enemy = false)
	{
		ItemAttributes currentlyLookingAtItemAttributes = PhysGrabber.instance.currentlyLookingAtItemAttributes;
		if (!PhysGrabber.instance.grabbed && (bool)_itemAttributes && (bool)currentlyLookingAtItemAttributes && currentlyLookingAtItemAttributes != _itemAttributes)
		{
			return;
		}
		if (message != Text.text)
		{
			messageTimer = 0f;
			SemiUIResetAllShakeEffects();
		}
		if (enemy)
		{
			VertexGradient colorGradient = new VertexGradient(new Color(1f, 0f, 0f), new Color(1f, 0f, 0f), new Color(1f, 0.1f, 0f), new Color(1f, 0.1f, 0f));
			Text.fontSize = 35f;
			Text.colorGradient = colorGradient;
		}
		else
		{
			Text.colorGradient = originalGradient;
			if (!SemiFunc.RunIsShop())
			{
				Text.fontSize = 15f;
			}
		}
		messageTimer = 0.1f;
		if (message != messagePrev)
		{
			Text.text = message;
			SemiUISpringShakeY(5f, 5f, 0.3f);
			SemiUISpringScale(0.1f, 2.5f, 0.2f);
			messagePrev = message;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (messageTimer > 0f)
		{
			messageTimer -= Time.deltaTime;
			return;
		}
		messagePrev = "prev";
		Hide();
	}
}
