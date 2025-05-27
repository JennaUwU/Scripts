using UnityEngine;

public class DebugUI : MonoBehaviour
{
	public GameObject enableParent;

	private void Start()
	{
		if (!Application.isEditor)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (SemiFunc.DebugDev() && Input.GetKeyDown(KeyCode.F1))
		{
			enableParent.SetActive(!enableParent.activeSelf);
		}
	}
}
