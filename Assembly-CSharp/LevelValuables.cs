using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Valuables - _____", menuName = "Level/Level Valuables Preset", order = 2)]
public class LevelValuables : ScriptableObject
{
	public List<GameObject> tiny;

	public List<GameObject> small;

	public List<GameObject> medium;

	public List<GameObject> big;

	public List<GameObject> wide;

	public List<GameObject> tall;

	public List<GameObject> veryTall;
}
