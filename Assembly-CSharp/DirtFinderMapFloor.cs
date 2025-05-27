using System.Collections;
using UnityEngine;

public class DirtFinderMapFloor : MonoBehaviour
{
	public enum FloorType
	{
		Floor_1x1 = 0,
		Floor_1x1_Diagonal = 1,
		Floor_1x05 = 2,
		Floor_1x025 = 3,
		Floor_1x05_Diagonal = 4,
		Floor_1x025_Diagonal = 5,
		Truck_Floor = 6,
		Truck_Wall = 7,
		Used_Floor = 8,
		Used_Wall = 9,
		Inactive_Floor = 10,
		Inactive_Wall = 11,
		Floor_1x1_Curve = 12,
		Floor_1x1_Curve_Inverted = 13,
		Floor_1x05_Curve = 14,
		Floor_1x05_Curve_Inverted = 15
	}

	public FloorType Type;

	internal MapObject MapObject;

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
		Map.Instance.AddFloor(this);
	}
}
