using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Module : MonoBehaviourPunCallbacks, IPunObservable
{
	public enum Type
	{
		Normal = 0,
		Passage = 1,
		DeadEnd = 2,
		Extraction = 3
	}

	private Color colorPositive = Color.green;

	private Color colorNegative = new Color(1f, 0.74f, 0.61f);

	public bool wallsInside;

	[Space]
	public bool wallsMap;

	public bool levelPointsEntrance;

	[Space]
	public bool levelPointsWaypoints;

	[Space]
	public bool levelPointsRoomVolume;

	[Space]
	public bool levelPointsNavmesh;

	[Space]
	public bool levelPointsConnected;

	public bool lightsMax;

	[Space]
	public bool lightsPrefab;

	public bool roomVolumeDoors;

	[Space]
	public bool roomVolumeHeight;

	[Space]
	public bool roomVolumeSpace;

	public bool navmeshConnected;

	[Space]
	public bool navmeshPitfalls;

	public bool valuablesAllTypes;

	[Space]
	public bool valuablesMaxed;

	[Space]
	public bool valuablesSwitch;

	[Space]
	public bool valuablesSwitchNavmesh;

	[Space]
	public bool valuablesTest;

	public bool ModulePropSwitchSetup;

	[Space]
	public bool ModulePropSwitchNavmesh;

	internal bool ConnectingTop;

	internal bool ConnectingRight;

	internal bool ConnectingBottom;

	internal bool ConnectingLeft;

	[Space]
	internal bool SetupDone;

	internal bool First;

	[Space]
	internal int GridX;

	internal int GridY;

	public bool Explored;

	internal bool StartRoom;

	private void Start()
	{
		if ((bool)GetComponent<StartRoom>())
		{
			StartRoom = true;
			return;
		}
		base.transform.parent = LevelGenerator.Instance.LevelParent.transform;
		StartCoroutine(ReadyCheck());
	}

	private IEnumerator ReadyCheck()
	{
		while (!ConnectingTop && !ConnectingRight && !ConnectingBottom && !ConnectingLeft && !First)
		{
			yield return new WaitForSeconds(0.1f);
		}
		SetupDone = true;
		ModulePropSwitch[] componentsInChildren = GetComponentsInChildren<ModulePropSwitch>();
		foreach (ModulePropSwitch obj in componentsInChildren)
		{
			obj.Module = this;
			obj.Setup();
		}
		LevelGenerator.Instance.ModulesSpawned++;
		if (!wallsInside || !wallsMap || !levelPointsEntrance || !levelPointsWaypoints || !levelPointsRoomVolume || !levelPointsNavmesh || !levelPointsConnected || !lightsMax || !lightsPrefab || !roomVolumeDoors || !roomVolumeHeight || !roomVolumeSpace || !navmeshConnected || !navmeshPitfalls || !valuablesAllTypes || !valuablesMaxed || !valuablesSwitch || !valuablesSwitchNavmesh || !valuablesTest || !ModulePropSwitchSetup || !ModulePropSwitchNavmesh)
		{
			Debug.LogWarning("Module not checked off: " + base.name, base.gameObject);
		}
	}

	private void ResetChecklist()
	{
		wallsInside = false;
		wallsMap = false;
		levelPointsEntrance = false;
		levelPointsWaypoints = false;
		levelPointsRoomVolume = false;
		levelPointsNavmesh = false;
		levelPointsConnected = false;
		lightsMax = false;
		lightsPrefab = false;
		roomVolumeDoors = false;
		roomVolumeHeight = false;
		roomVolumeSpace = false;
		navmeshConnected = false;
		navmeshPitfalls = false;
		valuablesAllTypes = false;
		valuablesMaxed = false;
		valuablesSwitch = false;
		valuablesSwitchNavmesh = false;
		valuablesTest = false;
		ModulePropSwitchSetup = false;
		ModulePropSwitchNavmesh = false;
	}

	private void SetAllChecklist()
	{
		wallsInside = true;
		wallsMap = true;
		levelPointsEntrance = true;
		levelPointsWaypoints = true;
		levelPointsRoomVolume = true;
		levelPointsNavmesh = true;
		levelPointsConnected = true;
		lightsMax = true;
		lightsPrefab = true;
		roomVolumeDoors = true;
		roomVolumeHeight = true;
		roomVolumeSpace = true;
		navmeshConnected = true;
		navmeshPitfalls = true;
		valuablesAllTypes = true;
		valuablesMaxed = true;
		valuablesSwitch = true;
		valuablesSwitchNavmesh = true;
		valuablesTest = true;
		ModulePropSwitchSetup = true;
		ModulePropSwitchNavmesh = true;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(ConnectingTop);
			stream.SendNext(ConnectingRight);
			stream.SendNext(ConnectingBottom);
			stream.SendNext(ConnectingLeft);
			stream.SendNext(First);
		}
		else
		{
			ConnectingTop = (bool)stream.ReceiveNext();
			ConnectingRight = (bool)stream.ReceiveNext();
			ConnectingBottom = (bool)stream.ReceiveNext();
			ConnectingLeft = (bool)stream.ReceiveNext();
			First = (bool)stream.ReceiveNext();
		}
	}
}
