using System.Collections;
using UnityEngine;

public class RoomVolume : MonoBehaviour
{
	public bool Truck;

	public bool Extraction;

	public Color Color = Color.blue;

	[Space]
	public ReverbPreset ReverbPreset;

	public RoomAmbience RoomAmbience;

	public Module Module;

	public MapModule MapModule;

	private bool Explored;

	private void Start()
	{
		Module = GetComponentInParent<Module>();
		RoomVolume[] componentsInParent = GetComponentsInParent<RoomVolume>();
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			if (componentsInParent[i] != this)
			{
				Object.Destroy(this);
				return;
			}
		}
		StartCoroutine(Setup());
	}

	private IEnumerator Setup()
	{
		yield return new WaitForSeconds(0.1f);
		BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>();
		foreach (BoxCollider boxCollider in componentsInChildren)
		{
			Vector3 halfExtents = boxCollider.size * 0.5f;
			halfExtents.x *= Mathf.Abs(boxCollider.transform.lossyScale.x);
			halfExtents.y *= Mathf.Abs(boxCollider.transform.lossyScale.y);
			halfExtents.z *= Mathf.Abs(boxCollider.transform.lossyScale.z);
			Collider[] array = Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), halfExtents, boxCollider.transform.rotation, LayerMask.GetMask("Other"), QueryTriggerInteraction.Collide);
			for (int j = 0; j < array.Length; j++)
			{
				LevelPoint component = array[j].transform.GetComponent<LevelPoint>();
				if ((bool)component)
				{
					component.Room = this;
				}
			}
		}
		if (!Extraction && !Truck && !Module.StartRoom && !SemiFunc.RunIsShop())
		{
			componentsInChildren = GetComponentsInChildren<BoxCollider>();
			foreach (BoxCollider boxCollider2 in componentsInChildren)
			{
				Vector3 scale = boxCollider2.size * 0.5f;
				scale.x *= Mathf.Abs(boxCollider2.transform.lossyScale.x);
				scale.y *= Mathf.Abs(boxCollider2.transform.lossyScale.y);
				scale.z *= Mathf.Abs(boxCollider2.transform.lossyScale.z);
				Vector3 position = boxCollider2.transform.TransformPoint(boxCollider2.center);
				Quaternion rotation = boxCollider2.transform.rotation;
				MapModule = Map.Instance.AddRoomVolume(base.gameObject, position, rotation, scale, Module);
			}
		}
	}

	public void SetExplored()
	{
		if (!Explored)
		{
			Explored = true;
			if ((bool)MapModule)
			{
				MapModule.Hide();
			}
		}
	}
}
