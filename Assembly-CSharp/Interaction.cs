using System.Collections;
using UnityEngine;

public class Interaction : MonoBehaviour
{
	public enum InteractionType
	{
		None = 0,
		VacuumCleaner = 1,
		Duster = 2,
		Sledgehammer = 3,
		DirtFinder = 4,
		Picker = 5
	}

	public InteractionType Type;

	public Sprite Sprite;

	private void Start()
	{
		StartCoroutine(Add());
	}

	private IEnumerator Add()
	{
		while (!CleanDirector.instance.RemoveExcessSpots)
		{
			yield return new WaitForSeconds(0.1f);
		}
	}
}
