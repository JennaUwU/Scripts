using System.Collections;
using UnityEngine;

public class DirtFinderMapWall : MonoBehaviour
{
	public enum WallType
	{
		Wall_1x1 = 0,
		Door_1x1 = 1,
		Door_1x2 = 2,
		Door_Blocked = 3,
		Door_1x1_Diagonal = 4,
		Wall_1x05 = 5,
		Wall_1x025 = 6,
		Wall_1x1_Diagonal = 7,
		Wall_1x05_Diagonal = 8,
		Wall_1x025_Diagonal = 9,
		Door_1x05_Diagonal = 10,
		Door_1x1_Wizard = 11,
		Door_Blocked_Wizard = 12,
		Stairs = 13,
		Door_1x05 = 14,
		Door_1x1_Arctic = 15,
		Door_Blocked_Arctic = 16,
		Wall_1x1_Curve = 17,
		Wall_1x05_Curve = 18
	}

	public WallType Type;

	private void Start()
	{
		StartCoroutine(Add());
	}

	private IEnumerator Add()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Map.Instance.AddWall(this);
	}
}
