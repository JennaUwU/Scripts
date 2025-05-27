using Photon.Pun;
using UnityEngine;

public class DebugJannek : MonoBehaviour
{
	private HurtCollider hurtCollider;

	private float hurtColliderTimer;

	private Transform playerTransform;

	private void Start()
	{
		hurtCollider = GetComponentInChildren<HurtCollider>(includeInactive: true);
		hurtCollider.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if ((!LevelGenerator.Instance.Generated && (bool)SpectateCamera.instance && GameDirector.instance.currentState == GameDirector.gameState.Main) || !PlayerController.instance.playerAvatarScript || PlayerController.instance.playerAvatarScript.deadSet)
		{
			return;
		}
		base.transform.position = Camera.main.transform.position;
		base.transform.rotation = Camera.main.transform.rotation;
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
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
			PlayerController.instance.playerAvatarScript.playerHealth.Heal(30);
		}
	}
}
