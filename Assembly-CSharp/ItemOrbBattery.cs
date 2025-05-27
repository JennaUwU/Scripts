using UnityEngine;

public class ItemOrbBattery : MonoBehaviour, ITargetingCondition
{
	private ItemOrb itemOrb;

	private PhysGrabObject physGrabObject;

	public bool CustomTargetingCondition(GameObject target)
	{
		return SemiFunc.BatteryChargeCondition(target.GetComponent<ItemBattery>());
	}

	private void Start()
	{
		itemOrb = GetComponent<ItemOrb>();
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	private void Update()
	{
		if (!itemOrb.itemActive || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PhysGrabObject item in itemOrb.objectAffected)
		{
			if ((bool)item && physGrabObject != item)
			{
				item.GetComponent<ItemBattery>().ChargeBattery(base.gameObject, SemiFunc.BatteryGetChargeRate(3));
			}
		}
	}
}
