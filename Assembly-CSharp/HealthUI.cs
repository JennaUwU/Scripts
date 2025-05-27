using TMPro;
using UnityEngine;

public class HealthUI : SemiUI
{
	private TextMeshProUGUI Text;

	private PlayerHealth playerHealth;

	private bool setup = true;

	public static HealthUI instance;

	private int playerHealthValue;

	private int playerHealthPrevious;

	private TextMeshProUGUI textMaxHealth;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		textMaxHealth = base.transform.Find("HealthMax").GetComponent<TextMeshProUGUI>();
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)playerHealth)
		{
			playerHealthValue = playerHealth.health;
			if (playerHealthValue != playerHealthPrevious)
			{
				SemiUISpringShakeY(20f, 10f, 0.3f);
				Color color = Color.white;
				if (playerHealthValue < playerHealthPrevious)
				{
					color = Color.red;
				}
				SemiUITextFlashColor(color, 0.2f);
				SemiUISpringScale(0.3f, 5f, 0.2f);
				playerHealthPrevious = playerHealthValue;
			}
		}
		if (setup)
		{
			if (LevelGenerator.Instance.Generated)
			{
				playerHealth = PlayerController.instance.playerAvatarScript.playerHealth;
				setup = false;
			}
		}
		else
		{
			Text.text = playerHealthValue.ToString();
			textMaxHealth.text = "<b><color=#008b20>/</color></b>" + playerHealth.maxHealth;
		}
	}
}
