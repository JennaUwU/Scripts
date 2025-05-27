using UnityEngine;

public class ModuleBlockObject : MonoBehaviour
{
	private void Start()
	{
		base.transform.parent = LevelGenerator.Instance.LevelParent.transform;
	}
}
