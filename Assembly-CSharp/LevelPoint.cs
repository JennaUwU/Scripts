using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelPoint : MonoBehaviour
{
	public bool DebugMeshActive = true;

	public Mesh DebugMesh;

	internal bool inStartRoom;

	[Space]
	public bool ModuleConnect;

	public bool Truck;

	private bool ModuleConnected;

	public RoomVolume Room;

	[Space]
	public List<LevelPoint> ConnectedPoints;

	[HideInInspector]
	public List<LevelPoint> AllLevelPoints;

	private void Start()
	{
		LevelGenerator.Instance.LevelPathPoints.Add(this);
		if (Truck)
		{
			LevelGenerator.Instance.LevelPathTruck = this;
		}
		if ((bool)GetComponentInParent<StartRoom>())
		{
			inStartRoom = true;
		}
		if (ModuleConnect)
		{
			StartCoroutine(ModuleConnectSetup());
		}
		StartCoroutine(NavMeshCheck());
	}

	private IEnumerator NavMeshCheck()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
		bool flag = false;
		if (!NavMesh.SamplePosition(base.transform.position, out var _, 0.5f, -1))
		{
			flag = true;
			Debug.LogError("Level Point not on Navmesh! Fix!", base.gameObject);
		}
		if (!Room)
		{
			flag = true;
			Debug.LogError("Level Point did not find a room volume!! Fix!!!", base.gameObject);
		}
		foreach (LevelPoint connectedPoint in ConnectedPoints)
		{
			if (!connectedPoint)
			{
				flag = true;
				Debug.LogError("Level Point not fully connected! Fix!!", base.gameObject);
				continue;
			}
			bool flag2 = false;
			foreach (LevelPoint connectedPoint2 in connectedPoint.ConnectedPoints)
			{
				if (connectedPoint2 == this)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				flag = true;
				Debug.LogError("Level Point not fully connected! Fix!!", base.gameObject);
			}
		}
		if (flag && Application.isEditor)
		{
			Object.Instantiate(AssetManager.instance.debugLevelPointError, base.transform.position, Quaternion.identity);
		}
	}

	private IEnumerator ModuleConnectSetup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		float num = 999f;
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if (levelPathPoint.ModuleConnect)
			{
				float num2 = Vector3.Distance(base.transform.position, levelPathPoint.transform.position);
				if (num2 < 15f && num2 < num && Vector3.Dot(levelPathPoint.transform.forward, base.transform.forward) <= -0.8f && Vector3.Dot(levelPathPoint.transform.forward, (base.transform.position - levelPathPoint.transform.position).normalized) > 0.8f)
				{
					num = num2;
					ConnectedPoints.Add(levelPathPoint);
				}
			}
		}
		ModuleConnected = true;
	}
}
