using System.Collections;
using UnityEngine;

public class CandleFlame : MonoBehaviour
{
	private bool logicActive;

	public PropLight propLight;

	public AnimationCurve flickerCurve;

	public AnimationCurve swayCurve;

	private float flickerXNew;

	private float flickerXOld;

	private float flickerXLerp = 1f;

	private float flickerXSpeed;

	private float flickerYNew;

	private float flickerYOld;

	private float flickerYLerp = 1f;

	private float flickerYSpeed;

	private float flickerZNew;

	private float flickerZOld;

	private float flickerZLerp = 1f;

	private float flickerZSpeed;

	private void Awake()
	{
		if (!propLight || !propLight.lightComponent)
		{
			Debug.LogError("Candle Flame missing Prop Light!", base.gameObject);
			base.gameObject.SetActive(value: false);
		}
		else if (!logicActive)
		{
			StartCoroutine(LogicCoroutine());
		}
	}

	private void OnEnable()
	{
		if (!logicActive)
		{
			StartCoroutine(LogicCoroutine());
		}
	}

	private void OnDisable()
	{
		logicActive = false;
		StopAllCoroutines();
	}

	private IEnumerator LogicCoroutine()
	{
		logicActive = true;
		while (true)
		{
			yield return new WaitForSeconds(Logic());
		}
	}

	private float Logic()
	{
		if (propLight.turnedOff)
		{
			base.gameObject.SetActive(value: false);
			return 999f;
		}
		if (propLight.lightComponent.intensity > 0f)
		{
			flickerXLerp += Time.deltaTime * flickerXSpeed;
			if (flickerXLerp >= 1f)
			{
				flickerXLerp = 0f;
				flickerXOld = flickerXNew;
				flickerXNew = Random.Range(0.8f, 1.2f);
				flickerXSpeed = Random.Range(25f, 75f);
			}
			flickerYLerp += Time.deltaTime * flickerYSpeed;
			if (flickerYLerp >= 1f)
			{
				flickerYLerp = 0f;
				flickerYOld = flickerYNew;
				flickerYNew = Random.Range(0.8f, 1.2f);
				flickerYSpeed = Random.Range(25f, 75f);
			}
			flickerZLerp += Time.deltaTime * flickerZSpeed;
			if (flickerZLerp >= 1f)
			{
				flickerZLerp = 0f;
				flickerZOld = flickerZNew;
				flickerZNew = Random.Range(0.8f, 1.2f);
				flickerZSpeed = Random.Range(25f, 75f);
			}
			base.transform.localScale = new Vector3(Mathf.Lerp(flickerXOld, flickerXNew, flickerCurve.Evaluate(flickerXLerp)), Mathf.Lerp(flickerYOld, flickerYNew, flickerCurve.Evaluate(flickerYLerp)), Mathf.Lerp(flickerZOld, flickerZNew, flickerCurve.Evaluate(flickerZLerp)));
			return 0.025f;
		}
		return 2f;
	}
}
