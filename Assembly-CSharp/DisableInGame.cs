using UnityEngine;

public class DisableInGame : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}
}
