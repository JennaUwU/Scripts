using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DebugRuben : MonoBehaviour
{
	private PhotonView photonView;

	private HurtCollider hurtCollider;

	private float hurtColliderTimer;

	private Transform playerTransform;

	public List<GameObject> spawnObjects;

	private void Start()
	{
		hurtCollider = GetComponentInChildren<HurtCollider>(includeInactive: true);
		hurtCollider.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (SemiFunc.KeyDownRuben(KeyCode.F6))
		{
			SpawnObject(AssetManager.instance.surplusValuableSmall, base.transform.position + base.transform.forward * 2f, "Valuables/");
		}
		if (SemiFunc.KeyDownRuben(KeyCode.F7))
		{
			SpawnObject(AssetManager.instance.surplusValuableBig, base.transform.position + base.transform.forward * 2f, "Valuables/");
		}
		if (SemiFunc.KeyDownRuben(KeyCode.F5))
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, 999f);
		}
		if ((!LevelGenerator.Instance.Generated && (bool)SpectateCamera.instance && GameDirector.instance.currentState == GameDirector.gameState.Main) || !PlayerController.instance.playerAvatarScript || PlayerController.instance.playerAvatarScript.deadSet)
		{
			return;
		}
		base.transform.position = Camera.main.transform.position;
		base.transform.rotation = Camera.main.transform.rotation;
		if (Input.GetKeyDown(KeyCode.F2))
		{
			hurtCollider.gameObject.SetActive(value: true);
			hurtColliderTimer = 0.2f;
		}
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
			if (hurtColliderTimer <= 0f)
			{
				hurtCollider.gameObject.SetActive(value: false);
			}
		}
		if (Input.GetKeyDown(KeyCode.F4))
		{
			PlayerController.instance.playerAvatarScript.playerHealth.Heal(75);
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
