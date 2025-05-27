using UnityEngine;

public class PlayerLocalCamera : MonoBehaviour
{
	public bool debug;

	private void OnDrawGizmos()
	{
		if (debug)
		{
			Gizmos.color = new Color(1f, 0f, 0.79f, 0.5f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawSphere(Vector3.zero, 0.1f);
			Gizmos.DrawCube(new Vector3(0f, 0f, 0.15f), new Vector3(0.1f, 0.1f, 0.3f));
		}
	}
}
