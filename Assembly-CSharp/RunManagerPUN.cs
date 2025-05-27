using Photon.Pun;
using UnityEngine;

public class RunManagerPUN : MonoBehaviour
{
	internal PhotonView photonView;

	private RunManager runManager;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		runManager = RunManager.instance;
		runManager.runManagerPUN = this;
		runManager.restarting = false;
		runManager.restartingDone = false;
	}

	[PunRPC]
	private void UpdateLevelRPC(string _levelName, int _levelsCompleted, bool _gameOver)
	{
		runManager.UpdateLevel(_levelName, _levelsCompleted, _gameOver);
	}
}
