using UnityEngine;

public class StunExplosion : MonoBehaviour
{
	public Light light;

	public AnimationCurve lightCurve;

	private float lightEval;

	private float removeTimer;

	private HurtCollider hurtCollider;

	public ItemGrenade itemGrenade;

	private void Start()
	{
		hurtCollider = GetComponentInChildren<HurtCollider>();
	}

	public void StunExplosionReset()
	{
		removeTimer = 0f;
		lightEval = 0f;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if ((bool)light)
		{
			if (lightEval < 1f)
			{
				light.intensity = 10f * lightCurve.Evaluate(lightEval);
				lightEval += 0.2f * Time.deltaTime;
			}
			else
			{
				light.intensity = 0f;
			}
		}
		if (removeTimer > 0.5f)
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
		else
		{
			hurtCollider.gameObject.SetActive(value: true);
		}
		removeTimer += Time.deltaTime;
		if (removeTimer >= 20f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
