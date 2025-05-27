using TMPro;
using UnityEngine;

public class EnergyUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static EnergyUI instance;

	private TextMeshProUGUI textEnergyMax;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		textEnergyMax = base.transform.Find("EnergyMax").GetComponent<TextMeshProUGUI>();
	}

	protected override void Update()
	{
		base.Update();
		Text.text = Mathf.Ceil(PlayerController.instance.EnergyCurrent).ToString();
		textEnergyMax.text = "<b><color=orange>/</color></b>" + Mathf.Ceil(PlayerController.instance.EnergyStart);
	}
}
