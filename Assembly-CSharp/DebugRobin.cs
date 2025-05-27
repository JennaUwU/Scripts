using Photon.Pun;
using UnityEngine;

public class DebugRobin : MonoBehaviour
{
	private Transform playerTransform;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F6))
		{
			SpawnObject(AssetManager.instance.surplusValuableMedium, base.transform.position + base.transform.forward * 2f, "Valuables/");
		}
		if (Input.GetKeyDown(KeyCode.F4))
		{
			PlayerController.instance.playerAvatarScript.playerHealth.Hurt(10, savingGrace: true);
		}
		if (Input.GetKeyDown(KeyCode.F5))
		{
			PlayerController.instance.playerAvatarScript.playerHealth.Heal(10);
		}
	}

	private void SpawnObject(GameObject _object, Vector3 _position, string _path)
	{
		if (!SemiFunc.IsMultiplayer())
		{
			Object.Instantiate(_object, _position, Quaternion.identity);
		}
		else if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			PhotonNetwork.InstantiateRoomObject(_path + _object.name, _position, Quaternion.identity, 0);
		}
	}
}
