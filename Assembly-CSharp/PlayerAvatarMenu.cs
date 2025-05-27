using UnityEngine;

public class PlayerAvatarMenu : MonoBehaviour
{
	public static PlayerAvatarMenu instance;

	public Transform cameraAndStuff;

	private MenuPage parentPage;

	private Vector3 startPosition;

	internal Rigidbody rb;

	private Vector3 rotationForce;

	internal PlayerAvatarVisuals playerVisuals;

	private void Awake()
	{
		startPosition = new Vector3(0f, 0f, -2000f);
		if ((bool)instance && instance.startPosition == startPosition)
		{
			startPosition = new Vector3(0f, 4f, -2000f);
		}
		playerVisuals = GetComponentInChildren<PlayerAvatarVisuals>();
		instance = this;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		parentPage = GetComponentInParent<MenuPage>();
		base.transform.SetParent(null);
		base.transform.localScale = Vector3.one;
		base.transform.position = startPosition;
		cameraAndStuff.SetParent(null);
		cameraAndStuff.localScale = Vector3.one;
	}

	private void FixedUpdate()
	{
		rb.MovePosition(startPosition);
		if (rotationForce.magnitude > 0.1f)
		{
			rb.AddTorque(rotationForce * Time.fixedDeltaTime);
			rotationForce = Vector3.zero;
			rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 1f);
		}
	}

	private void Update()
	{
		if (SemiFunc.InputMovementX() > 0.01f || SemiFunc.InputMovementX() < -0.01f)
		{
			float y = (0f - SemiFunc.InputMovementX()) * 3000f;
			Rotate(new Vector3(0f, y, 0f));
		}
		if (!parentPage)
		{
			Object.Destroy(cameraAndStuff.gameObject);
			Object.Destroy(base.gameObject);
		}
	}

	public void Rotate(Vector3 _rotation)
	{
		rotationForce = _rotation;
	}
}
