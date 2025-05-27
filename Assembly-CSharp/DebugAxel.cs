using Photon.Pun;
using UnityEngine;

public class DebugAxel : MonoBehaviour
{
	private HurtCollider hurtCollider;

	private float hurtColliderTimer;

	private Transform playerTransform;

	public Sound sound;

	private void Start()
	{
		hurtCollider = GetComponentInChildren<HurtCollider>(includeInactive: true);
		hurtCollider.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F7))
		{
			SpawnObject(AssetManager.instance.surplusValuableSmall, base.transform.position + base.transform.forward * 2f, "Valuables/");
		}
		if (Input.GetKeyDown(KeyCode.F6))
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, 999f);
			foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
			{
				item.playerDeathHead.inExtractionPoint = true;
				item.playerDeathHead.Revive();
			}
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
