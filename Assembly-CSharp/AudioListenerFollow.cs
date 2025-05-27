using UnityEngine;

public class AudioListenerFollow : MonoBehaviour
{
	public static AudioListenerFollow instance;

	public Transform TargetPositionTransform;

	public Transform TargetRotationTransform;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		TargetPositionTransform = Camera.main.transform;
		TargetRotationTransform = Camera.main.transform;
	}

	private void Update()
	{
		if ((bool)TargetPositionTransform)
		{
			if ((bool)SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Death))
			{
				base.transform.position = TargetPositionTransform.position;
			}
			else
			{
				base.transform.position = TargetPositionTransform.position + TargetPositionTransform.forward * AssetManager.instance.mainCamera.nearClipPlane;
			}
			if ((bool)TargetRotationTransform)
			{
				base.transform.rotation = TargetRotationTransform.rotation;
			}
		}
	}
}
