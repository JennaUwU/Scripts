using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Unity.AI.Navigation;
using UnityEngine;

public class LevelGenerator : MonoBehaviourPunCallbacks, IPunObservable
{
	public enum LevelState
	{
		Start = 0,
		Load = 1,
		Tiles = 2,
		StartRoom = 3,
		ConnectObjects = 4,
		ModuleGeneration = 5,
		BlockObjects = 6,
		ModuleSpawnLocal = 7,
		ModuleSpawnRemote = 8,
		LevelPoint = 9,
		Item = 10,
		Valuable = 11,
		PlayerSetup = 12,
		PlayerSpawn = 13,
		Enemy = 14,
		Done = 15
	}

	[Serializable]
	public class Tile
	{
		public int x;

		public int y;

		public bool active;

		public bool first;

		public int connections;

		public bool connectedTop;

		public bool connectedRight;

		public bool connectedBot;

		public bool connectedLeft;

		public Module.Type type;
	}

	public LevelState State;

	public static LevelGenerator Instance;

	[HideInInspector]
	public PhotonView PhotonView;

	internal GameObject DebugModule;

	internal GameObject DebugStartRoom;

	internal bool DebugNormal;

	internal bool DebugPassage;

	internal bool DebugDeadEnd;

	internal int DebugAmount;

	internal bool DebugNoEnemy;

	internal float DebugLevelSize = 1f;

	internal bool AllPlayersReady;

	public bool Generated;

	internal int ModulesSpawned;

	private int ModulesReadyPlayers;

	[Space]
	internal int EnemiesSpawnTarget;

	internal int EnemiesSpawned;

	private int EnemyReadyPlayers;

	private bool EnemyReady;

	internal int playerSpawned;

	[Space]
	public Level Level;

	internal int ModuleAmount;

	private int PassageAmount;

	private int DeadEndAmount;

	private int ExtractionAmount;

	public static float TileSize = 5f;

	public static float ModuleWidth = 3f;

	public static float ModuleHeight = 1f;

	[Space]
	public GameObject LevelParent;

	public GameObject EnemyParent;

	public GameObject ItemParent;

	private List<GameObject> ModulesNormalShuffled_1;

	private List<GameObject> ModulesNormalShuffled_2;

	private List<GameObject> ModulesNormalShuffled_3;

	private int ModulesNormalIndex_1;

	private int ModulesNormalIndex_2;

	private int ModulesNormalIndex_3;

	private int ModulesNormalIndexLoops_1;

	private int ModulesNormalIndexLoops_2;

	private int ModulesNormalIndexLoops_3;

	private List<GameObject> ModulesPassageShuffled_1;

	private List<GameObject> ModulesPassageShuffled_2;

	private List<GameObject> ModulesPassageShuffled_3;

	private int ModulesPassageIndex_1;

	private int ModulesPassageIndex_2;

	private int ModulesPassageIndex_3;

	private int ModulesPassageIndexLoops_1;

	private int ModulesPassageIndexLoops_2;

	private int ModulesPassageIndexLoops_3;

	private List<GameObject> ModulesDeadEndShuffled_1;

	private List<GameObject> ModulesDeadEndShuffled_2;

	private List<GameObject> ModulesDeadEndShuffled_3;

	private int ModulesDeadEndIndex_1;

	private int ModulesDeadEndIndex_2;

	private int ModulesDeadEndIndex_3;

	private int ModulesDeadEndIndexLoops_1;

	private int ModulesDeadEndIndexLoops_2;

	private int ModulesDeadEndIndexLoops_3;

	private List<GameObject> ModulesExtractionShuffled_1;

	private List<GameObject> ModulesExtractionShuffled_2;

	private List<GameObject> ModulesExtractionShuffled_3;

	private int ModulesExtractionIndex_1;

	private int ModulesExtractionIndex_2;

	private int ModulesExtractionIndex_3;

	private int ModulesExtractionIndexLoops_1;

	private int ModulesExtractionIndexLoops_2;

	private int ModulesExtractionIndexLoops_3;

	[Space]
	public AnimationCurve DifficultyCurve1;

	public AnimationCurve DifficultyCurve2;

	public AnimationCurve DifficultyCurve3;

	private float ModuleRarity1;

	private float ModuleRarity2;

	private float ModuleRarity3;

	[Space]
	public int LevelWidth = 3;

	public int LevelHeight = 3;

	private Tile[,] LevelGrid;

	private Vector3[] ModuleRotations = new Vector3[4]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 90f, 0f),
		new Vector3(0f, 180f, 0f),
		new Vector3(0f, 270f, 0f)
	};

	[Space]
	public List<LevelPoint> LevelPathPoints;

	public LevelPoint LevelPathTruck;

	private NavMeshSurface NavMeshSurface;

	private string ResourceParent = "Level";

	private string ResourceStart = "Start Room";

	private string ResourceModules = "Modules";

	private string ResourceOther = "Other";

	private string ResourceEnemies = "Enemies";

	[Space]
	public GameObject PlayerDeathHeadPrefab;

	public GameObject PlayerTumblePrefab;

	private bool waitingForSubCoroutine;

	private void Awake()
	{
		Instance = this;
		PhotonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		StartCoroutine(Generate());
	}

	private IEnumerator Generate()
	{
		yield return new WaitForSeconds(0.2f);
		if (!SemiFunc.IsMultiplayer())
		{
			AllPlayersReady = true;
		}
		while (!AllPlayersReady)
		{
			State = LevelState.Load;
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.2f);
		Level = RunManager.instance.levelCurrent;
		RunManager.instance.levelPrevious = Level;
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			ModuleAmount = Level.ModuleAmount;
			if (DebugAmount > 0)
			{
				ModuleAmount = DebugAmount;
			}
			StartCoroutine(TileGeneration());
			while (waitingForSubCoroutine)
			{
				yield return null;
			}
			StartCoroutine(StartRoomGeneration());
			while (waitingForSubCoroutine)
			{
				yield return null;
			}
			StartCoroutine(GenerateConnectObjects());
			while (waitingForSubCoroutine)
			{
				yield return null;
			}
			StartCoroutine(ModuleGeneration());
			while (waitingForSubCoroutine)
			{
				yield return null;
			}
			StartCoroutine(GenerateBlockObjects());
			while (waitingForSubCoroutine)
			{
				yield return null;
			}
			if (GameManager.instance.gameMode == 1)
			{
				PhotonView.RPC("ModuleAmountRPC", RpcTarget.AllBuffered, ModuleAmount);
			}
		}
		while (ModulesSpawned < ModuleAmount - 1)
		{
			State = LevelState.ModuleSpawnLocal;
			yield return new WaitForSeconds(0.1f);
		}
		if (GameManager.instance.gameMode == 1)
		{
			PhotonView.RPC("ModulesReadyRPC", RpcTarget.AllBuffered);
		}
		while (GameManager.instance.gameMode == 1 && ModulesReadyPlayers < PhotonNetwork.CurrentRoom.PlayerCount)
		{
			State = LevelState.ModuleSpawnRemote;
			yield return new WaitForSeconds(0.1f);
		}
		EnvironmentDirector.Instance.Setup();
		PostProcessing.Instance.Setup();
		LevelMusic.instance.Setup();
		ConstantMusic.instance.Setup();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			while (LevelPathPoints.Count == 0)
			{
				State = LevelState.LevelPoint;
				yield return new WaitForSeconds(0.1f);
			}
			State = LevelState.Item;
			if (!SemiFunc.IsMultiplayer())
			{
				ItemSetup();
			}
			else
			{
				PhotonView.RPC("ItemSetup", RpcTarget.AllBuffered);
			}
			StartCoroutine(ValuableDirector.instance.SetupHost());
			while (!ValuableDirector.instance.setupComplete)
			{
				State = LevelState.Valuable;
				yield return new WaitForSeconds(0.1f);
			}
			NavMeshSetup();
			yield return null;
			while (GameDirector.instance.PlayerList.Count == 0)
			{
				State = LevelState.PlayerSetup;
				yield return new WaitForSeconds(0.1f);
			}
			PlayerSpawn();
			yield return null;
			while (playerSpawned < GameDirector.instance.PlayerList.Count)
			{
				State = LevelState.PlayerSpawn;
				yield return new WaitForSeconds(0.1f);
			}
			if (Level.HasEnemies && !DebugNoEnemy)
			{
				EnemySetup();
				yield return null;
				if (GameManager.Multiplayer())
				{
					while (!EnemyReady)
					{
						State = LevelState.Enemy;
						if (EnemyReadyPlayers >= PhotonNetwork.CurrentRoom.PlayerCount || EnemiesSpawnTarget <= 0)
						{
							PhotonView.RPC("EnemyReadyAllRPC", RpcTarget.AllBuffered);
							EnemyReady = true;
						}
						yield return new WaitForSeconds(0.1f);
					}
				}
				else
				{
					while (EnemiesSpawned < EnemiesSpawnTarget)
					{
						yield return new WaitForSeconds(0.1f);
					}
				}
			}
			State = LevelState.Done;
			if (!SemiFunc.IsMultiplayer())
			{
				GenerateDone();
			}
			else
			{
				PhotonView.RPC("GenerateDone", RpcTarget.AllBuffered);
			}
			SessionManager.instance.CrownPlayer();
		}
		else
		{
			while (!Generated)
			{
				yield return new WaitForSeconds(0.1f);
			}
		}
	}

	public void PlayerSpawn()
	{
		List<SpawnPoint> list = UnityEngine.Object.FindObjectsOfType<SpawnPoint>().ToList();
		list.Shuffle();
		List<SpawnPoint> list2 = new List<SpawnPoint>();
		bool flag = false;
		foreach (SpawnPoint item in list)
		{
			if (item.debug)
			{
				list2.Add(item);
				flag = true;
			}
		}
		if (flag)
		{
			int num = 0;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				player.Spawn(list2[num].transform.position, list2[num].transform.rotation);
				num++;
				if (num >= list2.Count)
				{
					num = 0;
				}
			}
		}
		else
		{
			int num2 = 0;
			foreach (PlayerAvatar player2 in GameDirector.instance.PlayerList)
			{
				player2.Spawn(list[num2].transform.position, list[num2].transform.rotation);
				num2++;
				if (num2 >= list.Count)
				{
					num2 = 0;
				}
			}
		}
		if (SemiFunc.MenuLevel())
		{
			return;
		}
		foreach (PlayerAvatar player3 in GameDirector.instance.PlayerList)
		{
			if (!player3.playerDeathHead)
			{
				GameObject gameObject = (GameManager.Multiplayer() ? PhotonNetwork.Instantiate(PlayerDeathHeadPrefab.name, new Vector3(0f, 3000f, 0f), Quaternion.identity, 0) : UnityEngine.Object.Instantiate(PlayerDeathHeadPrefab, new Vector3(0f, 3000f, 0f), Quaternion.identity));
				PlayerDeathHead component = gameObject.GetComponent<PlayerDeathHead>();
				component.playerAvatar = player3;
				component.playerAvatar.playerDeathHead = component;
			}
			if (!player3.tumble)
			{
				GameObject gameObject2 = (GameManager.Multiplayer() ? PhotonNetwork.Instantiate(PlayerTumblePrefab.name, new Vector3(0f, 3000f, 0f), Quaternion.identity, 0) : UnityEngine.Object.Instantiate(PlayerTumblePrefab, new Vector3(0f, 3000f, 0f), Quaternion.identity));
				PlayerTumble component2 = gameObject2.GetComponent<PlayerTumble>();
				component2.playerAvatar = player3;
				component2.playerAvatar.tumble = component2;
			}
		}
	}

	public void NavMeshSetup()
	{
		if (GameManager.instance.gameMode == 0)
		{
			NavMeshSetupRPC();
		}
		else
		{
			PhotonView.RPC("NavMeshSetupRPC", RpcTarget.AllBuffered);
		}
	}

	public void MeshColliderFinder()
	{
		List<MeshCollider> list = new List<MeshCollider>();
		Debug.LogWarning("");
		Debug.LogWarning("Mesh Colliders:");
		MeshCollider[] array = UnityEngine.Object.FindObjectsOfType<MeshCollider>();
		foreach (MeshCollider meshCollider in array)
		{
			bool flag = false;
			foreach (MeshCollider item in list)
			{
				if (item.sharedMesh == meshCollider.sharedMesh)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(meshCollider);
			}
		}
		foreach (MeshCollider item2 in list)
		{
			Debug.LogWarning("    " + item2.sharedMesh.name, item2.gameObject);
		}
	}

	[PunRPC]
	public void ItemSetup()
	{
		if (!SemiFunc.RunIsArena())
		{
			ShopManager.instance.ShopInitialize();
			ItemManager.instance.ItemsInitialize();
		}
	}

	[PunRPC]
	private void NavMeshSetupRPC()
	{
		NavMeshSurface = GetComponent<NavMeshSurface>();
		NavMeshSurface.RemoveData();
		NavMeshSurface.BuildNavMesh();
		base.transform.localPosition = new Vector3(0f, 0.001f, 0f);
	}

	private IEnumerator StartRoomGeneration()
	{
		waitingForSubCoroutine = true;
		State = LevelState.StartRoom;
		List<GameObject> list = new List<GameObject>();
		list.AddRange(Level.StartRooms);
		list.Shuffle();
		if ((bool)DebugStartRoom)
		{
			list[0] = DebugStartRoom;
		}
		GameObject gameObject = ((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(ResourceParent + "/" + Level.ResourcePath + "/" + ResourceStart + "/" + list[0].name, Vector3.zero, Quaternion.identity, 0) : UnityEngine.Object.Instantiate(list[0], Vector3.zero, Quaternion.identity));
		gameObject.transform.parent = LevelParent.transform;
		yield return null;
		waitingForSubCoroutine = false;
	}

	private IEnumerator TileGeneration()
	{
		waitingForSubCoroutine = true;
		State = LevelState.Tiles;
		LevelWidth = Mathf.Max(2, Mathf.CeilToInt((float)LevelWidth * DebugLevelSize));
		LevelHeight = Mathf.Max(2, Mathf.CeilToInt((float)LevelHeight * DebugLevelSize));
		LevelGrid = new Tile[LevelWidth, LevelHeight];
		for (int i = 0; i < LevelWidth; i++)
		{
			for (int j = 0; j < LevelHeight; j++)
			{
				LevelGrid[i, j] = new Tile
				{
					x = i,
					y = j,
					active = false
				};
			}
		}
		ExtractionAmount = 0;
		if (ModuleAmount > 4)
		{
			ModuleAmount = Mathf.Min(5 + RunManager.instance.levelsCompleted, 10);
			ModuleAmount = Mathf.CeilToInt((float)ModuleAmount * DebugLevelSize);
			if (!DebugModule)
			{
				DeadEndAmount = Mathf.CeilToInt(ModuleAmount / 3);
				if (ModuleAmount >= 10)
				{
					ExtractionAmount = 3;
				}
				else if (ModuleAmount >= 8)
				{
					ExtractionAmount = 2;
				}
				else if (ModuleAmount >= 6)
				{
					ExtractionAmount = 1;
				}
				else
				{
					ExtractionAmount = 0;
				}
			}
		}
		if (Level == RunManager.instance.levelShop)
		{
			DeadEndAmount = 1;
		}
		int moduleAmount = ModuleAmount;
		LevelGrid[LevelWidth / 2, 0].active = true;
		LevelGrid[LevelWidth / 2, 0].first = true;
		moduleAmount--;
		int num = LevelWidth / 2;
		int num2 = 0;
		while (moduleAmount > 0)
		{
			int num3 = -999;
			int num4 = -999;
			while (num + num3 < 0 || num + num3 >= LevelWidth || num2 + num4 < 0 || num2 + num4 >= LevelHeight)
			{
				num3 = 0;
				num4 = 0;
				int num5 = UnityEngine.Random.Range(0, 4);
				if (num2 == 1)
				{
					num5 = UnityEngine.Random.Range(0, 3);
				}
				if (DebugPassage)
				{
					num5 = 2;
				}
				switch (num5)
				{
				case 0:
					num3--;
					break;
				case 1:
					num3++;
					break;
				case 2:
					num4++;
					break;
				case 3:
					num4--;
					break;
				}
			}
			num += num3;
			num2 += num4;
			if (!LevelGrid[num, num2].active)
			{
				LevelGrid[num, num2].active = true;
				moduleAmount--;
			}
		}
		yield return null;
		List<Tile> possibleExtractionTiles = new List<Tile>();
		if (!DebugNormal && !DebugPassage && !DebugDeadEnd)
		{
			for (int k = 0; k < LevelWidth; k++)
			{
				for (int l = 0; l < LevelHeight; l++)
				{
					if (!LevelGrid[k, l].active)
					{
						int num6 = 0;
						Tile tile = GridGetTile(k, l + 1);
						if (tile != null && tile.active)
						{
							num6++;
						}
						Tile tile2 = GridGetTile(k + 1, l);
						if (tile2 != null && tile2.active)
						{
							num6++;
						}
						Tile tile3 = GridGetTile(k, l - 1);
						if (tile3 != null && tile3.active)
						{
							num6++;
						}
						Tile tile4 = GridGetTile(k - 1, l);
						if (tile4 != null && tile4.active)
						{
							num6++;
						}
						if (num6 == 1)
						{
							possibleExtractionTiles.Add(LevelGrid[k, l]);
						}
					}
				}
			}
		}
		yield return null;
		int num7 = ExtractionAmount;
		Tile tile5 = new Tile();
		tile5.x = LevelWidth / 2;
		tile5.y = -1;
		List<Tile> _extractionTiles = new List<Tile> { tile5 };
		while (num7 > 0 && possibleExtractionTiles.Count > 0)
		{
			Tile tile6 = null;
			float num8 = 0f;
			foreach (Tile item in possibleExtractionTiles)
			{
				float num9 = 9999999f;
				foreach (Tile item2 in _extractionTiles)
				{
					float num10 = Vector2.Distance(new Vector2(item2.x, item2.y), new Vector2(item.x, item.y));
					if (num10 < num9)
					{
						num9 = num10;
					}
				}
				if (num9 > num8)
				{
					num8 = num9;
					tile6 = item;
				}
			}
			SetExtractionTile(Module.Type.Extraction, tile6, ref _extractionTiles, ref possibleExtractionTiles);
			num7--;
		}
		yield return null;
		int num11 = DeadEndAmount;
		while (num11 > 0 && possibleExtractionTiles.Count > 0)
		{
			Tile tile7 = possibleExtractionTiles[UnityEngine.Random.Range(0, possibleExtractionTiles.Count)];
			SetExtractionTile(Module.Type.DeadEnd, tile7, ref _extractionTiles, ref possibleExtractionTiles);
			num11--;
		}
		yield return null;
		waitingForSubCoroutine = false;
	}

	private void SetExtractionTile(Module.Type _type, Tile _tile, ref List<Tile> _extractionTiles, ref List<Tile> _possibleExtractionTiles)
	{
		_tile.type = _type;
		_tile.active = true;
		_extractionTiles.Add(_tile);
		_possibleExtractionTiles.Remove(_tile);
		bool flag = false;
		while (!flag)
		{
			flag = true;
			foreach (Tile _possibleExtractionTile in _possibleExtractionTiles)
			{
				if (GridGetTile(_possibleExtractionTile.x, _possibleExtractionTile.y - 1) == _tile || GridGetTile(_possibleExtractionTile.x + 1, _possibleExtractionTile.y) == _tile || GridGetTile(_possibleExtractionTile.x, _possibleExtractionTile.y + 1) == _tile || GridGetTile(_possibleExtractionTile.x - 1, _possibleExtractionTile.y) == _tile)
				{
					_possibleExtractionTiles.Remove(_possibleExtractionTile);
					flag = false;
					break;
				}
			}
		}
	}

	private IEnumerator ModuleGeneration()
	{
		waitingForSubCoroutine = true;
		State = LevelState.ModuleGeneration;
		ModulesNormalShuffled_1 = new List<GameObject>();
		ModulesNormalShuffled_2 = new List<GameObject>();
		ModulesNormalShuffled_3 = new List<GameObject>();
		ModulesPassageShuffled_1 = new List<GameObject>();
		ModulesPassageShuffled_2 = new List<GameObject>();
		ModulesPassageShuffled_3 = new List<GameObject>();
		ModulesDeadEndShuffled_1 = new List<GameObject>();
		ModulesDeadEndShuffled_2 = new List<GameObject>();
		ModulesDeadEndShuffled_3 = new List<GameObject>();
		ModulesExtractionShuffled_1 = new List<GameObject>();
		ModulesExtractionShuffled_2 = new List<GameObject>();
		ModulesExtractionShuffled_3 = new List<GameObject>();
		ModuleRarity1 = DifficultyCurve1.Evaluate(SemiFunc.RunGetDifficultyMultiplier());
		ModuleRarity2 = DifficultyCurve2.Evaluate(SemiFunc.RunGetDifficultyMultiplier());
		ModuleRarity3 = DifficultyCurve3.Evaluate(SemiFunc.RunGetDifficultyMultiplier());
		if (!DebugModule)
		{
			ModulesNormalShuffled_1.AddRange(Level.ModulesNormal1);
			ModulesNormalShuffled_1.Shuffle();
			ModulesNormalShuffled_2.AddRange(Level.ModulesNormal2);
			ModulesNormalShuffled_2.Shuffle();
			ModulesNormalShuffled_3.AddRange(Level.ModulesNormal3);
			ModulesNormalShuffled_3.Shuffle();
			ModulesPassageShuffled_1.AddRange(Level.ModulesPassage1);
			ModulesPassageShuffled_1.Shuffle();
			ModulesPassageShuffled_2.AddRange(Level.ModulesPassage2);
			ModulesPassageShuffled_2.Shuffle();
			ModulesPassageShuffled_3.AddRange(Level.ModulesPassage3);
			ModulesPassageShuffled_3.Shuffle();
			ModulesDeadEndShuffled_1.AddRange(Level.ModulesDeadEnd1);
			ModulesDeadEndShuffled_1.Shuffle();
			ModulesDeadEndShuffled_2.AddRange(Level.ModulesDeadEnd2);
			ModulesDeadEndShuffled_2.Shuffle();
			ModulesDeadEndShuffled_3.AddRange(Level.ModulesDeadEnd3);
			ModulesDeadEndShuffled_3.Shuffle();
			ModulesExtractionShuffled_1.AddRange(Level.ModulesExtraction1);
			ModulesExtractionShuffled_1.Shuffle();
			ModulesExtractionShuffled_2.AddRange(Level.ModulesExtraction2);
			ModulesExtractionShuffled_2.Shuffle();
			ModulesExtractionShuffled_3.AddRange(Level.ModulesExtraction3);
			ModulesExtractionShuffled_3.Shuffle();
		}
		else
		{
			ModulesNormalShuffled_1.Add(DebugModule);
			ModulesPassageShuffled_1.Add(DebugModule);
			ModulesDeadEndShuffled_1.Add(DebugModule);
			ModulesExtractionShuffled_1.Add(DebugModule);
		}
		if (ModulesNormalShuffled_1.Count == 0)
		{
			waitingForSubCoroutine = false;
			yield break;
		}
		for (int x = 0; x < LevelWidth; x++)
		{
			for (int y = 0; y < LevelHeight; y++)
			{
				if (!LevelGrid[x, y].active)
				{
					continue;
				}
				Vector3 rotation = Vector3.zero;
				Vector3 position = new Vector3((float)x * ModuleWidth * TileSize - (float)(LevelWidth / 2) * ModuleWidth * TileSize, 0f, (float)y * ModuleWidth * TileSize + ModuleWidth * TileSize / 2f);
				if (!DebugNormal && !DebugPassage && !DebugDeadEnd && LevelGrid[x, y].type == Module.Type.Extraction)
				{
					if (GridCheckActive(x, y - 1))
					{
						rotation = Vector3.zero;
					}
					if (GridCheckActive(x - 1, y))
					{
						rotation = new Vector3(0f, 90f, 0f);
					}
					if (GridCheckActive(x, y + 1))
					{
						rotation = new Vector3(0f, 180f, 0f);
					}
					if (GridCheckActive(x + 1, y))
					{
						rotation = new Vector3(0f, -90f, 0f);
					}
					SpawnModule(x, y, position, rotation, Module.Type.Extraction);
					continue;
				}
				if (DebugDeadEnd || (!DebugNormal && !DebugPassage && LevelGrid[x, y].type == Module.Type.DeadEnd))
				{
					if (GridCheckActive(x, y - 1))
					{
						rotation = Vector3.zero;
					}
					if (GridCheckActive(x - 1, y))
					{
						rotation = new Vector3(0f, 90f, 0f);
					}
					if (GridCheckActive(x, y + 1))
					{
						rotation = new Vector3(0f, 180f, 0f);
					}
					if (GridCheckActive(x + 1, y))
					{
						rotation = new Vector3(0f, -90f, 0f);
					}
					SpawnModule(x, y, position, rotation, Module.Type.DeadEnd);
					continue;
				}
				if (!DebugNormal && (DebugPassage || PassageAmount < Level.PassageMaxAmount))
				{
					if (DebugPassage || (GridCheckActive(x, y + 1) && (GridCheckActive(x, y - 1) || LevelGrid[x, y].first) && !GridCheckActive(x + 1, y) && !GridCheckActive(x - 1, y)))
					{
						if (UnityEngine.Random.Range(0, 100) < 50)
						{
							rotation = new Vector3(0f, 180f, 0f);
						}
						SpawnModule(x, y, position, rotation, Module.Type.Passage);
						PassageAmount++;
						continue;
					}
					if (!LevelGrid[x, y].first && GridCheckActive(x + 1, y) && GridCheckActive(x - 1, y) && !GridCheckActive(x, y + 1) && !GridCheckActive(x, y - 1))
					{
						rotation = new Vector3(0f, 90f, 0f);
						if (UnityEngine.Random.Range(0, 100) < 50)
						{
							rotation = new Vector3(0f, -90f, 0f);
						}
						SpawnModule(x, y, position, rotation, Module.Type.Passage);
						PassageAmount++;
						continue;
					}
				}
				rotation = ModuleRotations[UnityEngine.Random.Range(0, ModuleRotations.Length)];
				SpawnModule(x, y, position, rotation, Module.Type.Normal);
				yield return null;
			}
		}
		waitingForSubCoroutine = false;
	}

	private bool GridCheckActive(int x, int y)
	{
		if (x >= 0 && x < LevelWidth && y >= 0 && y < LevelHeight)
		{
			return LevelGrid[x, y].active;
		}
		return false;
	}

	private Tile GridGetTile(int x, int y)
	{
		if (x >= 0 && x < LevelWidth && y >= 0 && y < LevelHeight)
		{
			return LevelGrid[x, y];
		}
		return null;
	}

	private GameObject PickModule(List<GameObject> _list1, List<GameObject> _list2, List<GameObject> _list3, ref int _index1, ref int _index2, ref int _index3, ref int _loops1, ref int _loops2, ref int _loops3)
	{
		GameObject result = null;
		float[] array = new float[3] { ModuleRarity1, ModuleRarity2, ModuleRarity3 };
		if (_list2.Count <= 0)
		{
			array[1] = 0f;
		}
		if (_list3.Count <= 0)
		{
			array[2] = 0f;
		}
		int num = Mathf.Max(_loops1, _loops2, _loops3);
		int num2 = Mathf.Min(_loops1, _loops2, _loops3);
		if (num != num2)
		{
			if (_loops1 == num2 && array[0] > 0f)
			{
				if (_loops1 != _loops2)
				{
					array[1] = 0f;
				}
				if (_loops1 != _loops3)
				{
					array[2] = 0f;
				}
			}
			else if (_loops2 == num2 && array[1] > 0f)
			{
				if (_loops2 != _loops1)
				{
					array[0] = 0f;
				}
				if (_loops2 != _loops3)
				{
					array[2] = 0f;
				}
			}
			else if (_loops3 == num2 && array[2] > 0f)
			{
				if (_loops3 != _loops1)
				{
					array[0] = 0f;
				}
				if (_loops3 != _loops2)
				{
					array[1] = 0f;
				}
			}
		}
		float num3 = -1f;
		int num4 = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] > 0f)
			{
				float num5 = UnityEngine.Random.Range(0f, array[i]);
				if (num5 > num3)
				{
					num3 = num5;
					num4 = i;
				}
			}
		}
		switch (num4)
		{
		case 0:
			result = _list1[_index1];
			_index1++;
			if (_index1 >= _list1.Count)
			{
				_list1.Shuffle();
				_index1 = 0;
				_loops1++;
			}
			break;
		case 1:
			result = _list2[_index2];
			_index2++;
			if (_index2 >= _list2.Count)
			{
				_list2.Shuffle();
				_index2 = 0;
				_loops2++;
			}
			break;
		case 2:
			result = _list3[_index3];
			_index3++;
			if (_index3 >= _list3.Count)
			{
				_list3.Shuffle();
				_index3 = 0;
				_loops3++;
			}
			break;
		}
		return result;
	}

	private void SpawnModule(int x, int y, Vector3 position, Vector3 rotation, Module.Type type)
	{
		GameObject gameObject = null;
		switch (type)
		{
		case Module.Type.Normal:
			gameObject = PickModule(ModulesNormalShuffled_1, ModulesNormalShuffled_2, ModulesNormalShuffled_3, ref ModulesNormalIndex_1, ref ModulesNormalIndex_2, ref ModulesNormalIndex_3, ref ModulesNormalIndexLoops_1, ref ModulesNormalIndexLoops_2, ref ModulesNormalIndexLoops_3);
			LevelGrid[x, y].type = Module.Type.Normal;
			break;
		case Module.Type.Passage:
			gameObject = PickModule(ModulesPassageShuffled_1, ModulesPassageShuffled_2, ModulesPassageShuffled_3, ref ModulesPassageIndex_1, ref ModulesPassageIndex_2, ref ModulesPassageIndex_3, ref ModulesPassageIndexLoops_1, ref ModulesPassageIndexLoops_2, ref ModulesPassageIndexLoops_3);
			LevelGrid[x, y].type = Module.Type.Passage;
			break;
		case Module.Type.DeadEnd:
			gameObject = PickModule(ModulesDeadEndShuffled_1, ModulesDeadEndShuffled_2, ModulesDeadEndShuffled_3, ref ModulesDeadEndIndex_1, ref ModulesDeadEndIndex_2, ref ModulesDeadEndIndex_3, ref ModulesDeadEndIndexLoops_1, ref ModulesDeadEndIndexLoops_2, ref ModulesDeadEndIndexLoops_3);
			LevelGrid[x, y].type = Module.Type.DeadEnd;
			break;
		case Module.Type.Extraction:
			gameObject = PickModule(ModulesExtractionShuffled_1, ModulesExtractionShuffled_2, ModulesExtractionShuffled_3, ref ModulesExtractionIndex_1, ref ModulesExtractionIndex_2, ref ModulesExtractionIndex_3, ref ModulesExtractionIndexLoops_1, ref ModulesExtractionIndexLoops_2, ref ModulesExtractionIndexLoops_3);
			LevelGrid[x, y].type = Module.Type.Extraction;
			break;
		}
		GameObject gameObject2 = ((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(ResourceParent + "/" + Level.ResourcePath + "/" + ResourceModules + "/" + gameObject.name, position, Quaternion.Euler(rotation), 0) : UnityEngine.Object.Instantiate(gameObject, position, Quaternion.Euler(rotation)));
		gameObject2.transform.parent = LevelParent.transform;
		Module component = gameObject2.GetComponent<Module>();
		component.GridX = x;
		component.GridY = y;
		component.First = LevelGrid[x, y].first;
		if (GridCheckActive(x, y + 1))
		{
			component.ConnectingTop = true;
		}
		if (GridCheckActive(x, y - 1) || component.First)
		{
			component.ConnectingBottom = true;
		}
		if (GridCheckActive(x + 1, y))
		{
			component.ConnectingRight = true;
		}
		if (GridCheckActive(x - 1, y))
		{
			component.ConnectingLeft = true;
		}
	}

	private IEnumerator GenerateConnectObjects()
	{
		waitingForSubCoroutine = true;
		State = LevelState.ConnectObjects;
		float moduleWidth = ModuleWidth * TileSize;
		for (int x = 0; x < LevelWidth; x++)
		{
			for (int y = 0; y < LevelHeight; y++)
			{
				if (LevelGrid[x, y].active)
				{
					if (GridCheckActive(x, y + 1))
					{
						LevelGrid[x, y].connections++;
					}
					if (GridCheckActive(x, y - 1))
					{
						LevelGrid[x, y].connections++;
					}
					if (GridCheckActive(x + 1, y))
					{
						LevelGrid[x, y].connections++;
					}
					if (GridCheckActive(x - 1, y))
					{
						LevelGrid[x, y].connections++;
					}
					float num = (float)x * moduleWidth - (float)(LevelWidth / 2) * moduleWidth;
					float num2 = (float)y * moduleWidth + moduleWidth / 2f;
					if (y + 1 < LevelHeight && LevelGrid[x, y + 1].active && !LevelGrid[x, y + 1].connectedBot)
					{
						SpawnConnectObject(new Vector3(num, 0f, num2 + moduleWidth / 2f), Vector3.zero);
						LevelGrid[x, y].connectedTop = true;
					}
					if (x + 1 < LevelWidth && LevelGrid[x + 1, y].active && !LevelGrid[x + 1, y].connectedLeft)
					{
						SpawnConnectObject(new Vector3(num + moduleWidth / 2f, 0f, num2), new Vector3(0f, 90f, 0f));
						LevelGrid[x, y].connectedRight = true;
					}
					if ((y - 1 >= 0 && LevelGrid[x, y - 1].active && !LevelGrid[x, y - 1].connectedTop) || (x == LevelWidth / 2 && y == 0))
					{
						SpawnConnectObject(new Vector3(num, 0f, num2 - moduleWidth / 2f), Vector3.zero);
						LevelGrid[x, y].connectedBot = true;
					}
					if (x - 1 >= 0 && LevelGrid[x - 1, y].active && !LevelGrid[x - 1, y].connectedRight)
					{
						SpawnConnectObject(new Vector3(num - moduleWidth / 2f, 0f, num2), Vector3.zero);
						LevelGrid[x, y].connectedLeft = true;
					}
					yield return null;
				}
			}
		}
		waitingForSubCoroutine = false;
	}

	private void SpawnConnectObject(Vector3 position, Vector3 rotation)
	{
		if ((bool)Level.ConnectObject)
		{
			GameObject gameObject = ((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(ResourceParent + "/" + Level.ResourcePath + "/" + ResourceOther + "/" + Level.ConnectObject.name, position, Quaternion.Euler(rotation), 0) : UnityEngine.Object.Instantiate(Level.ConnectObject, position, Quaternion.Euler(rotation)));
			gameObject.transform.parent = LevelParent.transform;
			ModuleConnectObject component = gameObject.GetComponent<ModuleConnectObject>();
			component.ModuleConnecting = true;
			component.MasterSetup = true;
		}
	}

	private IEnumerator GenerateBlockObjects()
	{
		waitingForSubCoroutine = true;
		State = LevelState.BlockObjects;
		float moduleWidth = ModuleWidth * TileSize;
		for (int x = 0; x < LevelWidth; x++)
		{
			for (int y = 0; y < LevelHeight; y++)
			{
				if (LevelGrid[x, y].active && LevelGrid[x, y].type == Module.Type.Normal)
				{
					float num = (float)x * moduleWidth - (float)(LevelWidth / 2) * moduleWidth;
					float num2 = (float)y * moduleWidth + moduleWidth / 2f;
					if (y + 1 >= LevelHeight || !LevelGrid[x, y + 1].active)
					{
						SpawnBlockObject(new Vector3(num, 0f, num2 + moduleWidth / 2f), new Vector3(0f, 180f, 0f));
					}
					if (x + 1 >= LevelWidth || !LevelGrid[x + 1, y].active)
					{
						SpawnBlockObject(new Vector3(num + moduleWidth / 2f, 0f, num2), new Vector3(0f, -90f, 0f));
					}
					if ((y - 1 < 0 || !LevelGrid[x, y - 1].active) && (x != LevelWidth / 2 || y != 0))
					{
						SpawnBlockObject(new Vector3(num, 0f, num2 - moduleWidth / 2f), new Vector3(0f, 0f, 0f));
					}
					if (x - 1 < 0 || !LevelGrid[x - 1, y].active)
					{
						SpawnBlockObject(new Vector3(num - moduleWidth / 2f, 0f, num2), new Vector3(0f, 90f, 0f));
					}
					yield return null;
				}
			}
		}
		waitingForSubCoroutine = false;
	}

	private void SpawnBlockObject(Vector3 position, Vector3 rotation)
	{
		if ((bool)Level.BlockObject)
		{
			GameObject gameObject = ((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(ResourceParent + "/" + Level.ResourcePath + "/" + ResourceOther + "/" + Level.BlockObject.name, position, Quaternion.Euler(rotation), 0) : UnityEngine.Object.Instantiate(Level.BlockObject, position, Quaternion.Euler(rotation)));
			gameObject.transform.parent = LevelParent.transform;
		}
	}

	private void EnemySetup()
	{
		RoomVolume roomVolume = null;
		foreach (RoomVolume item in UnityEngine.Object.FindObjectsOfType<RoomVolume>().ToList())
		{
			if (item.Truck)
			{
				roomVolume = item;
				break;
			}
		}
		LevelPoint levelPoint = null;
		float num = 0f;
		foreach (LevelPoint levelPathPoint in LevelPathPoints)
		{
			float num2 = Vector3.Distance(levelPathPoint.transform.position, roomVolume.transform.position);
			if (num2 > num)
			{
				num = num2;
				levelPoint = levelPathPoint;
			}
		}
		RunManager.instance.EnemiesSpawnedRemoveStart();
		bool flag = false;
		if (EnemyDirector.instance.debugEnemy != null)
		{
			flag = true;
			EnemySetup[] debugEnemy = EnemyDirector.instance.debugEnemy;
			foreach (EnemySetup enemySetup in debugEnemy)
			{
				EnemySpawn(enemySetup, levelPoint.transform.position);
			}
		}
		if (!flag)
		{
			EnemyDirector.instance.AmountSetup();
			for (int j = 0; j < EnemyDirector.instance.totalAmount; j++)
			{
				EnemySpawn(EnemyDirector.instance.GetEnemy(), levelPoint.transform.position);
			}
			EnemyDirector.instance.DebugResult();
		}
		RunManager.instance.EnemiesSpawnedRemoveEnd();
		if (GameManager.Multiplayer())
		{
			PhotonView.RPC("EnemySpawnTargetRPC", RpcTarget.AllBuffered, EnemiesSpawnTarget);
		}
	}

	private void EnemySpawn(EnemySetup enemySetup, Vector3 position)
	{
		foreach (GameObject spawnObject in enemySetup.spawnObjects)
		{
			GameObject gameObject = ((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(ResourceEnemies + "/" + spawnObject.name, position, Quaternion.identity, 0) : UnityEngine.Object.Instantiate(spawnObject, position, Quaternion.identity));
			EnemyParent component = gameObject.GetComponent<EnemyParent>();
			if ((bool)component)
			{
				component.SetupDone = true;
				gameObject.GetComponentInChildren<Enemy>().EnemyTeleported(position);
				EnemiesSpawnTarget++;
				EnemyDirector.instance.FirstSpawnPointAdd(component);
			}
		}
	}

	[PunRPC]
	private void GenerateDone()
	{
		SemiFunc.OnLevelGenDone();
		GameDirector.instance.SetStart();
		Generated = true;
	}

	[PunRPC]
	private void EnemySpawnTargetRPC(int _amount)
	{
		EnemiesSpawnTarget = _amount;
	}

	[PunRPC]
	private void EnemyReadyRPC()
	{
		EnemyReadyPlayers++;
	}

	[PunRPC]
	private void EnemyReadyAllRPC()
	{
		EnemyReady = true;
	}

	[PunRPC]
	private void PlayerSpawnedRPC()
	{
		playerSpawned++;
	}

	[PunRPC]
	private void ModulesReadyRPC()
	{
		ModulesReadyPlayers++;
	}

	[PunRPC]
	private void ModuleAmountRPC(int amount)
	{
		ModuleAmount = amount;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(ModulesReadyPlayers);
		}
		else
		{
			ModulesReadyPlayers = (int)stream.ReceiveNext();
		}
	}
}
