using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PhysGrabBeam : MonoBehaviour
{
	public PlayerAvatar playerAvatar;

	public Transform PhysGrabPointOrigin;

	public Transform PhysGrabPointOriginClient;

	public Transform PhysGrabPoint;

	public Transform PhysGrabPointPuller;

	public Material greenScreenMaterial;

	private Material originalMaterial;

	[HideInInspector]
	public Vector3 physGrabPointPullerSmoothPosition;

	public float CurveStrength = 1f;

	public int CurveResolution = 20;

	[Header("Texture Scrolling")]
	public Vector2 scrollSpeed = new Vector2(5f, 0f);

	[HideInInspector]
	public Vector2 originalScrollSpeed;

	private LineRenderer lineRenderer;

	[HideInInspector]
	public Material lineMaterial;

	private void Start()
	{
		if (!playerAvatar.isLocal)
		{
			PhysGrabPointOrigin = PhysGrabPointOriginClient;
		}
		originalScrollSpeed = scrollSpeed;
		lineRenderer = GetComponent<LineRenderer>();
		originalMaterial = lineRenderer.material;
		lineMaterial = lineRenderer.material;
	}

	private void LateUpdate()
	{
		DrawCurve();
		ScrollTexture();
	}

	private void OnEnable()
	{
		physGrabPointPullerSmoothPosition = PhysGrabPointPuller.position;
		if ((bool)VideoGreenScreen.instance)
		{
			lineMaterial = greenScreenMaterial;
			GetComponent<LineRenderer>().material = greenScreenMaterial;
		}
	}

	private void OnDisable()
	{
		lineMaterial = originalMaterial;
		if ((bool)lineRenderer)
		{
			lineRenderer.material = originalMaterial;
		}
	}

	private void DrawCurve()
	{
		if ((bool)PhysGrabPointPuller)
		{
			Vector3[] array = new Vector3[CurveResolution];
			Vector3 position = PhysGrabPointPuller.position;
			_ = Vector3.zero;
			physGrabPointPullerSmoothPosition = Vector3.Lerp(physGrabPointPullerSmoothPosition, position, Time.deltaTime * 10f);
			Vector3 p = physGrabPointPullerSmoothPosition * CurveStrength;
			for (int i = 0; i < CurveResolution; i++)
			{
				float t = (float)i / ((float)CurveResolution - 1f);
				array[i] = CalculateBezierPoint(t, PhysGrabPointOrigin.position, p, PhysGrabPoint.position);
			}
			lineRenderer.positionCount = CurveResolution;
			lineRenderer.SetPositions(array);
		}
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		return Mathf.Pow(1f - t, 2f) * p0 + 2f * (1f - t) * t * p1 + Mathf.Pow(t, 2f) * p2;
	}

	private void ScrollTexture()
	{
		if ((bool)lineMaterial)
		{
			if (playerAvatar.physGrabber.colorState == 1)
			{
				lineMaterial.mainTextureScale = new Vector2(-1f, 1f);
			}
			else
			{
				lineMaterial.mainTextureScale = new Vector2(1f, 1f);
			}
			Vector2 mainTextureOffset = Time.time * scrollSpeed;
			lineMaterial.mainTextureOffset = mainTextureOffset;
		}
	}
}
