using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : SemiUI
{
	public static BatteryUI instance;

	private int batteryState;

	public RawImage batteryImage;

	private float redBlinkTimer;

	private float batteryShowTimer;

	private Vector3 originalLocalScale;

	protected override void Start()
	{
		base.Start();
		instance = this;
		batteryState = 6;
		originalLocalScale = base.transform.localScale;
		base.transform.localScale = Vector3.zero;
	}

	private void BatteryLogic()
	{
		if (!LevelGenerator.Instance.Generated || !SemiFunc.FPSImpulse15() || !batteryImage)
		{
			return;
		}
		if (batteryState == 0)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.921f, 0.4f, 0.2f);
		}
		if (batteryState == 1)
		{
			batteryImage.uvRect = new Rect(0.369f, -0.687f, 0.4f, 0.2f);
		}
		if (batteryState == 2)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.687f, 0.4f, 0.2f);
		}
		if (batteryState == 3)
		{
			batteryImage.uvRect = new Rect(0.369f, -0.4523f, 0.4f, 0.2f);
		}
		if (batteryState == 4)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.4523f, 0.4f, 0.2f);
		}
		if (batteryState == 5)
		{
			batteryImage.uvRect = new Rect(0.369f, -0.218f, 0.4f, 0.2f);
		}
		if (batteryState == 6)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.218f, 0.4f, 0.2f);
		}
		if (batteryState > 6)
		{
			batteryState = 6;
		}
		if (batteryState < 0)
		{
			batteryState = 0;
		}
		if (batteryState <= 3 && SemiFunc.RunIsLobby())
		{
			float num = 1.8f;
			redBlinkTimer += Time.deltaTime * num;
			if (redBlinkTimer > 0.5f)
			{
				batteryImage.color = new Color(1f, 0f, 0f, 1f);
			}
			else
			{
				Color color = new Color(1f, 0.7f, 0f, 1f);
				batteryImage.color = color;
			}
			if (redBlinkTimer > 1f)
			{
				redBlinkTimer = 0f;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (SemiFunc.RunIsShop())
		{
			Hide();
			return;
		}
		BatteryFetch();
		BatteryLogic();
		if (batteryShowTimer > 0f)
		{
			if (!PhysGrabber.instance.grabbed)
			{
				batteryShowTimer = 0f;
			}
			batteryShowTimer -= Time.deltaTime;
			ItemInfoUI.instance.SemiUIScoot(new Vector2(0f, 20f));
		}
		else
		{
			Hide();
		}
	}

	public void BatteryFetch()
	{
		if (!batteryImage || !PhysGrabber.instance.grabbed || !SemiFunc.FPSImpulse5())
		{
			return;
		}
		PhysGrabObject grabbedPhysGrabObject = PhysGrabber.instance.grabbedPhysGrabObject;
		if (!grabbedPhysGrabObject)
		{
			return;
		}
		ItemBattery component = grabbedPhysGrabObject.GetComponent<ItemBattery>();
		if (!component || (component.onlyShowWhenItemToggleIsOn && !grabbedPhysGrabObject.GetComponent<ItemToggle>().toggleState))
		{
			return;
		}
		int batteryLifeInt = component.batteryLifeInt;
		if (batteryLifeInt != -1)
		{
			batteryState = batteryLifeInt;
			Color color = new Color(1f, 0.7f, 0f, 1f);
			if (batteryState == 0)
			{
				color = Color.red;
			}
			batteryImage.color = color;
		}
		batteryShowTimer = 1f;
	}
}
