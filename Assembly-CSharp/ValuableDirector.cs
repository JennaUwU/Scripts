using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ValuableDirector : MonoBehaviour
{
	public enum ValuableDebug
	{
		Normal = 0,
		All = 1,
		None = 2
	}

	public static ValuableDirector instance;

	private PhotonView PhotonView;

	internal ValuableDebug valuableDebug;

	[HideInInspector]
	public bool setupComplete;

	[HideInInspector]
	public bool valuablesSpawned;

	internal int valuableSpawnPlayerReady;

	internal int valuableSpawnAmount;

	internal int valuableTargetAmount = -1;

	internal int switchSetupPlayerReady;

	private string resourcePath = "Valuables/";

	[Space(20f)]
	public AnimationCurve totalMaxAmountCurve;

	private int totalMaxAmount;

	[Space(20f)]
	public AnimationCurve tinyMaxAmountCurve;

	public int tinyChance;

	private int tinyMaxAmount;

	private string tinyPath = "01 Tiny";

	private List<GameObject> tinyValuables = new List<GameObject>();

	private List<ValuableVolume> tinyVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve smallMaxAmountCurve;

	public int smallChance;

	private int smallMaxAmount;

	private string smallPath = "02 Small";

	private List<GameObject> smallValuables = new List<GameObject>();

	private List<ValuableVolume> smallVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve mediumMaxAmountCurve;

	public int mediumChance;

	private int mediumMaxAmount;

	private string mediumPath = "03 Medium";

	private List<GameObject> mediumValuables = new List<GameObject>();

	private List<ValuableVolume> mediumVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve bigMaxAmountCurve;

	public int bigChance;

	private int bigMaxAmount;

	private string bigPath = "04 Big";

	private List<GameObject> bigValuables = new List<GameObject>();

	private List<ValuableVolume> bigVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve wideMaxAmountCurve;

	public int wideChance;

	private int wideMaxAmount;

	private string widePath = "05 Wide";

	private List<GameObject> wideValuables = new List<GameObject>();

	private List<ValuableVolume> wideVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve tallMaxAmountCurve;

	public int tallChance;

	private int tallMaxAmount;

	private string tallPath = "06 Tall";

	private List<GameObject> tallValuables = new List<GameObject>();

	private List<ValuableVolume> tallVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve veryTallMaxAmountCurve;

	public int veryTallChance;

	private int veryTallMaxAmount;

	private string veryTallPath = "07 Very Tall";

	private List<GameObject> veryTallValuables = new List<GameObject>();

	private List<ValuableVolume> veryTallVolumes = new List<ValuableVolume>();

	[Space(20f)]
	public List<ValuableObject> valuableList = new List<ValuableObject>();

	private void Awake()
	{
		instance = this;
		PhotonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient)
		{
			StartCoroutine(SetupClient());
		}
	}

	public IEnumerator SetupClient()
	{
		while (valuableTargetAmount == -1)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (valuableSpawnAmount < valuableTargetAmount)
		{
			yield return new WaitForSeconds(0.1f);
		}
		PhotonView.RPC("PlayerReadyRPC", RpcTarget.All);
	}

	public IEnumerator SetupHost()
	{
		float time = SemiFunc.RunGetDifficultyMultiplier();
		if (SemiFunc.RunIsArena())
		{
			time = 0.75f;
		}
		totalMaxAmount = Mathf.RoundToInt(totalMaxAmountCurve.Evaluate(time));
		tinyMaxAmount = Mathf.RoundToInt(tinyMaxAmountCurve.Evaluate(time));
		smallMaxAmount = Mathf.RoundToInt(smallMaxAmountCurve.Evaluate(time));
		mediumMaxAmount = Mathf.RoundToInt(mediumMaxAmountCurve.Evaluate(time));
		bigMaxAmount = Mathf.RoundToInt(bigMaxAmountCurve.Evaluate(time));
		wideMaxAmount = Mathf.RoundToInt(wideMaxAmountCurve.Evaluate(time));
		tallMaxAmount = Mathf.RoundToInt(tallMaxAmountCurve.Evaluate(time));
		veryTallMaxAmount = Mathf.RoundToInt(veryTallMaxAmountCurve.Evaluate(time));
		if (SemiFunc.RunIsArena())
		{
			totalMaxAmount /= 2;
			tinyMaxAmount /= 3;
			smallMaxAmount /= 3;
			mediumMaxAmount /= 3;
			bigMaxAmount /= 3;
			wideMaxAmount /= 2;
			tallMaxAmount /= 2;
			veryTallMaxAmount /= 2;
		}
		foreach (LevelValuables valuablePreset in LevelGenerator.Instance.Level.ValuablePresets)
		{
			tinyValuables.AddRange(valuablePreset.tiny);
			smallValuables.AddRange(valuablePreset.small);
			mediumValuables.AddRange(valuablePreset.medium);
			bigValuables.AddRange(valuablePreset.big);
			wideValuables.AddRange(valuablePreset.wide);
			tallValuables.AddRange(valuablePreset.tall);
			veryTallValuables.AddRange(valuablePreset.veryTall);
		}
		List<ValuableVolume> list = Object.FindObjectsOfType<ValuableVolume>(includeInactive: false).ToList();
		tinyVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Tiny);
		tinyVolumes.Shuffle();
		smallVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Small);
		smallVolumes.Shuffle();
		mediumVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Medium);
		mediumVolumes.Shuffle();
		bigVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Big);
		bigVolumes.Shuffle();
		wideVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Wide);
		wideVolumes.Shuffle();
		tallVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Tall);
		tallVolumes.Shuffle();
		veryTallVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.VeryTall);
		veryTallVolumes.Shuffle();
		if (valuableDebug == ValuableDebug.All)
		{
			totalMaxAmount = list.Count;
			tinyMaxAmount = tinyVolumes.Count;
			smallMaxAmount = smallVolumes.Count;
			mediumMaxAmount = mediumVolumes.Count;
			bigMaxAmount = bigVolumes.Count;
			wideMaxAmount = wideVolumes.Count;
			tallMaxAmount = tallVolumes.Count;
			veryTallMaxAmount = veryTallVolumes.Count;
		}
		if (valuableDebug == ValuableDebug.None || LevelGenerator.Instance.Level.ValuablePresets.Count <= 0)
		{
			totalMaxAmount = 0;
			tinyMaxAmount = 0;
			smallMaxAmount = 0;
			mediumMaxAmount = 0;
			bigMaxAmount = 0;
			wideMaxAmount = 0;
			tallMaxAmount = 0;
			veryTallMaxAmount = 0;
		}
		valuableTargetAmount = 0;
		string[] _names = new string[7] { "Tiny", "Small", "Medium", "Big", "Wide", "Tall", "Very Tall" };
		int[] _maxAmount = new int[7] { tinyMaxAmount, smallMaxAmount, mediumMaxAmount, bigMaxAmount, wideMaxAmount, tallMaxAmount, veryTallMaxAmount };
		List<ValuableVolume>[] _volumes = new List<ValuableVolume>[7] { tinyVolumes, smallVolumes, mediumVolumes, bigVolumes, wideVolumes, tallVolumes, veryTallVolumes };
		string[] _path = new string[7] { tinyPath, smallPath, mediumPath, bigPath, widePath, tallPath, veryTallPath };
		int[] _chance = new int[7] { tinyChance, smallChance, mediumChance, bigChance, wideChance, tallChance, veryTallChance };
		List<GameObject>[] _valuables = new List<GameObject>[7] { tinyValuables, smallValuables, mediumValuables, bigValuables, wideValuables, tallValuables, veryTallValuables };
		int[] _volumeIndex = new int[7];
		for (int _i = 0; _i < totalMaxAmount; _i++)
		{
			float num = -1f;
			int num2 = -1;
			for (int num3 = 0; num3 < _names.Length; num3++)
			{
				if (_volumeIndex[num3] < _maxAmount[num3] && _volumeIndex[num3] < _volumes[num3].Count)
				{
					int num4 = Random.Range(0, _chance[num3]);
					if ((float)num4 > num)
					{
						num = num4;
						num2 = num3;
					}
				}
			}
			if (num2 == -1)
			{
				break;
			}
			ValuableVolume volume = _volumes[num2][_volumeIndex[num2]];
			GameObject valuable = _valuables[num2][Random.Range(0, _valuables[num2].Count)];
			Spawn(valuable, volume, _path[num2]);
			_volumeIndex[num2]++;
			yield return null;
		}
		if (valuableTargetAmount < totalMaxAmount && (bool)DebugComputerCheck.instance && (!DebugComputerCheck.instance.enabled || !DebugComputerCheck.instance.LevelDebug || !DebugComputerCheck.instance.ModuleOverrideActive || !DebugComputerCheck.instance.ModuleOverride))
		{
			for (int num5 = 0; num5 < _names.Length; num5++)
			{
				if (_volumeIndex[num5] < _maxAmount[num5])
				{
					Debug.LogError("Could not spawn enough ''" + _names[num5] + "'' valuables!");
				}
			}
		}
		if (GameManager.instance.gameMode == 1)
		{
			PhotonView.RPC("ValuablesTargetSetRPC", RpcTarget.All, valuableTargetAmount);
		}
		valuableSpawnPlayerReady++;
		while (GameManager.instance.gameMode == 1 && valuableSpawnPlayerReady < PhotonNetwork.CurrentRoom.PlayerCount)
		{
			yield return new WaitForSeconds(0.1f);
		}
		VolumesAndSwitchSetup();
		while (GameManager.instance.gameMode == 1 && switchSetupPlayerReady < PhotonNetwork.CurrentRoom.PlayerCount)
		{
			yield return new WaitForSeconds(0.1f);
		}
		setupComplete = true;
	}

	private void Spawn(GameObject _valuable, ValuableVolume _volume, string _path)
	{
		if (GameManager.instance.gameMode == 0)
		{
			Object.Instantiate(_valuable, _volume.transform.position, _volume.transform.rotation);
		}
		else
		{
			PhotonNetwork.InstantiateRoomObject(resourcePath + _path + "/" + _valuable.name, _volume.transform.position, _volume.transform.rotation, 0);
		}
		valuableTargetAmount++;
	}

	[PunRPC]
	private void ValuablesTargetSetRPC(int _amount)
	{
		valuableTargetAmount = _amount;
	}

	[PunRPC]
	private void PlayerReadyRPC()
	{
		valuableSpawnPlayerReady++;
	}

	public void VolumesAndSwitchSetup()
	{
		if (GameManager.instance.gameMode == 0)
		{
			VolumesAndSwitchSetupRPC();
		}
		else
		{
			PhotonView.RPC("VolumesAndSwitchSetupRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void VolumesAndSwitchSetupRPC()
	{
		ValuableVolume[] array = Object.FindObjectsOfType<ValuableVolume>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Setup();
		}
		ValuablePropSwitch[] array2 = Object.FindObjectsOfType<ValuablePropSwitch>(includeInactive: true);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Setup();
		}
		if (GameManager.instance.gameMode == 0)
		{
			VolumesAndSwitchReadyRPC();
		}
		else
		{
			PhotonView.RPC("VolumesAndSwitchReadyRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void VolumesAndSwitchReadyRPC()
	{
		switchSetupPlayerReady++;
	}
}
