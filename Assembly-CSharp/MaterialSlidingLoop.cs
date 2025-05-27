using UnityEngine;

public class MaterialSlidingLoop : MonoBehaviour
{
	private AudioSource source;

	public float activeTimer;

	public MaterialPreset material;

	public float pitchMultiplier;

	public float getMaterialTimer;

	private AudioLowPassLogic lowPassLogic;

	private void Start()
	{
		lowPassLogic = GetComponent<AudioLowPassLogic>();
		source = GetComponent<AudioSource>();
		activeTimer = 1f;
	}

	private void Update()
	{
		material.SlideLoop.Source = source;
		if (getMaterialTimer > 0f)
		{
			getMaterialTimer -= Time.deltaTime;
		}
		if (activeTimer > 0f)
		{
			activeTimer -= Time.deltaTime;
			material.SlideLoop.PlayLoop(playing: true, 5f, 5f, pitchMultiplier);
			return;
		}
		lowPassLogic.Volume -= 5f * Time.deltaTime;
		if (lowPassLogic.Volume <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
