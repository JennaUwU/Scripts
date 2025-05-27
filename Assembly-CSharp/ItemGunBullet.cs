using System.Collections;
using UnityEngine;

public class ItemGunBullet : MonoBehaviour
{
	private Transform hitEffectTransform;

	private ParticleSystem particleSparks;

	private ParticleSystem particleSmoke;

	private ParticleSystem particleImpact;

	private Light hitLight;

	private LineRenderer shootLine;

	public HurtCollider hurtCollider;

	internal bool bulletHit;

	internal Vector3 hitPosition;

	public float hurtColliderTimer = 0.25f;

	private bool shootLineActive;

	private float shootLineLerp;

	internal AnimationCurve shootLineWidthCurve;

	public void ActivateAll()
	{
		base.gameObject.SetActive(value: true);
		hitEffectTransform = base.transform.Find("Hit Effect");
		particleSparks = hitEffectTransform.Find("Particle Sparks").GetComponent<ParticleSystem>();
		particleSmoke = hitEffectTransform.Find("Particle Smoke").GetComponent<ParticleSystem>();
		particleImpact = hitEffectTransform.Find("Particle Impact").GetComponent<ParticleSystem>();
		hitLight = hitEffectTransform.Find("Hit Light").GetComponent<Light>();
		shootLine = GetComponentInChildren<LineRenderer>();
		Vector3 position = base.transform.position;
		Vector3 forward = hitPosition - position;
		shootLine.enabled = true;
		shootLine.SetPosition(0, base.transform.position);
		shootLine.SetPosition(1, base.transform.position + forward.normalized * 0.5f);
		shootLine.SetPosition(2, hitPosition - forward.normalized * 0.5f);
		shootLine.SetPosition(3, hitPosition);
		shootLineActive = true;
		shootLineLerp = 0f;
		if (bulletHit)
		{
			hitEffectTransform.gameObject.SetActive(value: true);
			particleSparks.gameObject.SetActive(value: true);
			particleSmoke.gameObject.SetActive(value: true);
			particleImpact.gameObject.SetActive(value: true);
			hitLight.enabled = true;
			hurtCollider.gameObject.SetActive(value: true);
			Quaternion rotation = Quaternion.LookRotation(forward);
			hurtCollider.transform.rotation = rotation;
			hurtCollider.transform.position = hitPosition;
			hurtCollider.gameObject.SetActive(value: true);
			hitEffectTransform.position = hitPosition;
			hitEffectTransform.rotation = rotation;
		}
		StartCoroutine(BulletDestroy());
	}

	private IEnumerator BulletDestroy()
	{
		yield return new WaitForSeconds(0.2f);
		while (particleSparks.isPlaying || particleSmoke.isPlaying || particleImpact.isPlaying || hitLight.enabled || shootLine.enabled || hurtCollider.gameObject.activeSelf)
		{
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private void LineRendererLogic()
	{
		if (shootLineActive)
		{
			shootLine.widthMultiplier = shootLineWidthCurve.Evaluate(shootLineLerp);
			shootLineLerp += Time.deltaTime * 5f;
			if (shootLineLerp >= 1f)
			{
				shootLine.enabled = false;
				shootLine.gameObject.SetActive(value: false);
				shootLineActive = false;
			}
		}
	}

	private void Update()
	{
		LineRendererLogic();
		if (!bulletHit)
		{
			return;
		}
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
			hurtCollider.gameObject.SetActive(value: true);
		}
		else
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
		if ((bool)hitLight)
		{
			hitLight.intensity = Mathf.Lerp(hitLight.intensity, 0f, Time.deltaTime * 10f);
			if (hitLight.intensity < 0.01f)
			{
				hitLight.enabled = false;
			}
		}
	}
}
