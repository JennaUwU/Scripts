using Photon.Pun;
using UnityEngine;

public class ItemGrenadeDuctTaped : MonoBehaviour
{
	public GameObject grenadePrefab;

	private ParticleScriptExplosion particleScriptExplosion;

	private PhotonView photonView;

	public Sound soundExplosion;

	public Sound soundExplosionGlobal;

	private void Start()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		photonView = GetComponent<PhotonView>();
	}

	public void Explosion()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			for (int i = 0; i < 3; i++)
			{
				Vector3 vector = new Vector3(0f, 0.2f * (float)i, 0f);
				ItemGrenadeHuman component = Object.Instantiate(grenadePrefab, base.transform.position + vector, Quaternion.identity).GetComponent<ItemGrenadeHuman>();
				component.Initialize();
				component.Spawn();
			}
		}
		else if (SemiFunc.IsMasterClient())
		{
			for (int j = 0; j < 3; j++)
			{
				Vector3 vector2 = new Vector3(0f, 0.2f * (float)j, 0f);
				GameObject obj = PhotonNetwork.Instantiate("Items/Item Grenade Human", base.transform.position + vector2, Quaternion.identity, 0);
				obj.GetComponent<ItemGrenadeHuman>().Initialize();
				obj.GetComponent<ItemGrenadeHuman>().Spawn();
			}
		}
		particleScriptExplosion.Spawn(base.transform.position, 0.8f, 50, 100, 4f, onlyParticleEffect: false, disableSound: true);
		soundExplosion.Play(base.transform.position);
		soundExplosionGlobal.Play(base.transform.position);
	}
}
