using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PhysGrabCart : MonoBehaviour
{
	public enum State
	{
		Locked = 0,
		Dragged = 1,
		Handled = 2
	}

	public bool isSmallCart;

	public GameObject smallCartHurtCollider;

	internal State currentState;

	internal State previousState = State.Handled;

	public TextMeshPro displayText;

	public Transform handlePoint;

	private PhysGrabObject physGrabObject;

	internal Rigidbody rb;

	public float stabilizationForce = 100f;

	private Vector3 hitPoint;

	private PhotonView photonView;

	internal bool cartActive;

	private bool cartActivePrevious;

	public GameObject buttonObject;

	private List<Collider> capsuleColliders = new List<Collider>();

	private List<Collider> cartInside = new List<Collider>();

	public PhysicMaterial physMaterialSlippery;

	public PhysicMaterial physMaterialSticky;

	public PhysicMaterial physMaterialALilSlippery;

	public PhysicMaterial physMaterialNormal;

	private Vector3 velocityRef;

	internal bool cartBeingPulled;

	private float playerInteractionTimer;

	private PhysGrabObjectGrabArea physGrabObjectGrabArea;

	private MeshRenderer cartMesh;

	public MeshRenderer[] grabMesh;

	private List<Material> grabMaterial = new List<Material>();

	[Space]
	public PhysGrabInCart physGrabInCart;

	internal Transform inCart;

	internal Vector3 actualVelocity;

	internal Vector3 actualVelocityLastPosition;

	private Vector3 lastPosition;

	internal List<PhysGrabObject> itemsInCart = new List<PhysGrabObject>();

	internal int itemsInCartCount;

	internal int haulCurrent;

	private float objectInCartCheckTimer = 0.5f;

	private int haulPrevious;

	private float haulUpdateEffectTimer;

	private bool deductedFromHaul;

	private bool resetHaulText;

	private Color originalHaulColor;

	public Sound soundHaulIncrease;

	public Sound soundHaulDecrease;

	[Space]
	public Sound soundLocked;

	public Sound soundDragged;

	public Sound soundHandled;

	private bool thirtyFPSUpdate;

	private float thirtyFPSUpdateTimer;

	private float autoTurnOffTimer;

	private float draggedTimer;

	public Transform cartGrabPoint;

	private ItemEquippable itemEquippable;

	private void Start()
	{
		itemEquippable = GetComponent<ItemEquippable>();
		originalHaulColor = displayText.color;
		physGrabObject = GetComponent<PhysGrabObject>();
		rb = GetComponent<Rigidbody>();
		rb.mass = 8f;
		inCart = base.transform.Find("In Cart");
		foreach (Transform item in base.transform)
		{
			Transform transform2 = item.Find("Semi Box Collider");
			if (item.name.Contains("Inside"))
			{
				cartInside.Add(transform2.GetComponent<Collider>());
			}
			if ((bool)transform2 && (transform2.GetComponent<Collider>().gameObject.layer == LayerMask.NameToLayer("PhysGrabObject") || transform2.GetComponent<Collider>().gameObject.layer == LayerMask.NameToLayer("Default")))
			{
				transform2.GetComponent<Collider>().gameObject.layer = LayerMask.NameToLayer("PhysGrabObjectCart");
			}
			if (item.name.Contains("Cart Mesh"))
			{
				cartMesh = item.GetComponent<MeshRenderer>();
			}
			if (item.name.Contains("Cart Wall Collider"))
			{
				transform2.GetComponent<Collider>().material = SemiFunc.PhysicMaterialPhysGrabObject();
			}
			if (item.name.Contains("Capsule"))
			{
				capsuleColliders.Add(item.GetComponent<Collider>());
			}
		}
		photonView = GetComponent<PhotonView>();
		physGrabObjectGrabArea = GetComponent<PhysGrabObjectGrabArea>();
		MeshRenderer[] array = grabMesh;
		foreach (MeshRenderer meshRenderer in array)
		{
			grabMaterial.Add(meshRenderer.material);
		}
	}

	private void ObjectsInCart()
	{
		if (SemiFunc.PlayerNearestDistance(base.transform.position) > 12f)
		{
			return;
		}
		if (objectInCartCheckTimer > 0f)
		{
			objectInCartCheckTimer -= Time.deltaTime;
		}
		else
		{
			Collider[] array = Physics.OverlapBox(inCart.position, inCart.localScale / 2f, inCart.rotation);
			itemsInCart.Clear();
			haulPrevious = haulCurrent;
			itemsInCartCount = 0;
			haulCurrent = 0;
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (collider.gameObject.layer != LayerMask.NameToLayer("PhysGrabObject"))
				{
					continue;
				}
				PhysGrabObject componentInParent = collider.GetComponentInParent<PhysGrabObject>();
				if ((bool)componentInParent && !itemsInCart.Contains(componentInParent))
				{
					itemsInCart.Add(componentInParent);
					ValuableObject componentInParent2 = collider.GetComponentInParent<ValuableObject>();
					if ((bool)componentInParent2)
					{
						haulCurrent += (int)componentInParent2.dollarValueCurrent;
					}
					itemsInCartCount++;
				}
			}
			objectInCartCheckTimer = 0.5f;
		}
		if (haulPrevious != haulCurrent)
		{
			haulUpdateEffectTimer = 0.3f;
			if (haulCurrent > haulPrevious)
			{
				deductedFromHaul = false;
				soundHaulIncrease.Play(displayText.transform.position);
			}
			else
			{
				deductedFromHaul = true;
				soundHaulDecrease.Play(displayText.transform.position);
			}
			haulPrevious = haulCurrent;
		}
		if (haulUpdateEffectTimer > 0f)
		{
			haulUpdateEffectTimer -= Time.deltaTime;
			haulUpdateEffectTimer = Mathf.Max(0f, haulUpdateEffectTimer);
			Color color = Color.white;
			if (deductedFromHaul)
			{
				color = Color.red;
			}
			displayText.color = color;
			if (thirtyFPSUpdate)
			{
				displayText.text = GlitchyText();
			}
			resetHaulText = false;
		}
		else if (!resetHaulText)
		{
			displayText.color = originalHaulColor;
			SetHaulText();
			resetHaulText = true;
		}
	}

	private void SetHaulText()
	{
		string text = "<color=#bd4300>$</color>";
		displayText.text = text + SemiFunc.DollarGetString(Mathf.Max(0, haulCurrent));
	}

	private void ThirtyFPS()
	{
		if (thirtyFPSUpdateTimer > 0f)
		{
			thirtyFPSUpdateTimer -= Time.deltaTime;
			thirtyFPSUpdateTimer = Mathf.Max(0f, thirtyFPSUpdateTimer);
		}
		else
		{
			thirtyFPSUpdate = true;
			thirtyFPSUpdateTimer = 1f / 30f;
		}
	}

	private string GlitchyText()
	{
		string text = "";
		for (int i = 0; i < 9; i++)
		{
			bool flag = false;
			if (UnityEngine.Random.Range(0, 4) == 0 && i <= 5)
			{
				text += "TAX";
				i += 2;
				flag = true;
			}
			if (UnityEngine.Random.Range(0, 3) == 0 && !flag)
			{
				text += "$";
				flag = true;
			}
			if (!flag)
			{
				text += UnityEngine.Random.Range(0, 10);
			}
		}
		return text;
	}

	private void StateMessages()
	{
		if (!SemiFunc.RunIsShop() && physGrabObject.grabbedLocal)
		{
			if (currentState == State.Handled)
			{
				Color color = new Color(0.2f, 0.8f, 0.1f);
				ItemInfoExtraUI.instance.ItemInfoText("Mode: STRONG", color);
			}
			if (currentState == State.Dragged)
			{
				Color color2 = new Color(1f, 0.46f, 0f);
				ItemInfoExtraUI.instance.ItemInfoText("Mode: WEAK", color2);
			}
		}
	}

	private void SmallCartLogic()
	{
		if (!isSmallCart || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (itemEquippable.isEquipping)
		{
			if (!smallCartHurtCollider.activeSelf)
			{
				smallCartHurtCollider.SetActive(value: true);
			}
		}
		else if (!smallCartHurtCollider.activeSelf)
		{
			smallCartHurtCollider.SetActive(value: false);
		}
		if (currentState == State.Locked)
		{
			CartMassOverride(8f);
			physGrabObject.OverrideMaterial(physMaterialSticky);
		}
	}

	private void Update()
	{
		if ((bool)itemEquippable && itemEquippable.isUnequipping)
		{
			return;
		}
		ThirtyFPS();
		ObjectsInCart();
		StateMessages();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			AutoTurnOff();
			StateLogic();
			if (playerInteractionTimer > 0f)
			{
				playerInteractionTimer -= Time.deltaTime;
			}
			thirtyFPSUpdate = false;
		}
	}

	private void FixedUpdate()
	{
		if ((bool)physGrabObjectGrabArea && physGrabObjectGrabArea.listOfAllGrabbers.Count > 0)
		{
			CartSteer();
		}
		else
		{
			cartBeingPulled = false;
		}
		if (!LevelGenerator.Instance.Generated || (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient))
		{
			return;
		}
		if (rb.IsSleeping())
		{
			if (Mathf.Abs(base.transform.rotation.eulerAngles.x) > 0.05f || Mathf.Abs(base.transform.rotation.eulerAngles.z) > 0.05f)
			{
				Vector3 eulerAngles = base.transform.rotation.eulerAngles;
				eulerAngles.x = 0f;
				eulerAngles.z = 0f;
				rb.MoveRotation(Quaternion.Euler(eulerAngles));
				rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y, 0f);
			}
		}
		else if (!rb.isKinematic)
		{
			Vector3 eulerAngles2 = base.transform.rotation.eulerAngles;
			eulerAngles2.x = 0f;
			eulerAngles2.z = 0f;
			rb.MoveRotation(Quaternion.Euler(eulerAngles2));
			rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y, 0f);
		}
		actualVelocity = (base.transform.position - actualVelocityLastPosition) / Time.fixedDeltaTime;
		actualVelocityLastPosition = base.transform.position;
	}

	private void AutoTurnOff()
	{
		if (physGrabObject.playerGrabbing.Count <= 0)
		{
			cartActive = false;
		}
	}

	private void CartMassOverride(float mass)
	{
		physGrabObject.OverrideMass(mass);
	}

	private void CartSteer()
	{
		List<PhysGrabber> listOfAllGrabbers = physGrabObjectGrabArea.listOfAllGrabbers;
		foreach (PhysGrabber item in listOfAllGrabbers)
		{
			if ((bool)item)
			{
				if (item.isLocal)
				{
					TutorialDirector.instance.playerUsedCart = true;
				}
				item.OverrideGrabPoint(cartGrabPoint);
			}
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PhysGrabber item2 in listOfAllGrabbers)
		{
			if ((bool)item2 && !inCart.GetComponent<BoxCollider>().bounds.Contains(item2.transform.position))
			{
				float num = 1f;
				float num2 = 1f;
				Rigidbody component = GetComponent<Rigidbody>();
				CartMassOverride(4f);
				if (item2 == PhysGrabber.instance)
				{
					SemiFunc.PhysGrabberLocalChangeAlpha(0.1f);
				}
				if (!cartActive && item2.initialPressTimer > 0f)
				{
					cartActive = true;
				}
				if (!cartActive || item2 != listOfAllGrabbers[0])
				{
					break;
				}
				cartBeingPulled = true;
				item2.physGrabForcesDisabled = true;
				float num3 = 2f;
				float num4 = 2.5f;
				if (isSmallCart)
				{
					num3 = 1.5f;
					num4 = 2f;
				}
				float num5 = 5f;
				Vector3 lhs = PlayerController.instance.rb.velocity;
				if (!item2.isLocal)
				{
					lhs = item2.playerAvatar.rbVelocityRaw;
				}
				bool flag = Vector3.Dot(lhs, base.transform.forward) > 0f;
				if (item2.playerAvatar.isSprinting)
				{
					num5 = 7f;
				}
				if (item2.playerAvatar.isSprinting && flag)
				{
					num3 = 3f;
					num4 = 4f;
				}
				float t = Mathf.Clamp(Vector3.Dot(component.velocity, item2.transform.forward) / num5, 0f, 1f);
				float num6 = Mathf.Lerp(num3, num4, t);
				Vector3 vector = item2.transform.rotation * Vector3.back;
				Vector3 vector2 = item2.playerAvatar.transform.position - vector * num6;
				float num7 = Mathf.Clamp(Vector3.Distance(base.transform.position, vector2 / 1f), 0f, 1f);
				Vector3 vector3 = (vector2 - base.transform.position).normalized * 5f * num7;
				vector3 = Vector3.ClampMagnitude(vector3, 5f);
				float y = component.velocity.y;
				component.velocity = Vector3.MoveTowards(component.velocity, vector3, num7 * 2f);
				component.velocity = new Vector3(component.velocity.x, y, component.velocity.z) * num;
				component.velocity = Vector3.ClampMagnitude(component.velocity, 5f);
				Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(item2.transform.position - base.transform.position, Vector3.up).eulerAngles.y + 180f, 0f);
				Quaternion rotation = Quaternion.Euler(0f, component.rotation.eulerAngles.y, 0f);
				(quaternion * Quaternion.Inverse(rotation)).ToAngleAxis(out var angle, out var axis);
				if (angle > 180f)
				{
					angle -= 360f;
				}
				float value = Mathf.Clamp(Mathf.Abs(angle) / 180f, 0.2f, 1f) * 20f;
				value = Mathf.Clamp(value, 0f, 4f);
				Vector3 vector4 = MathF.PI / 180f * angle * axis.normalized * value;
				vector4 = Vector3.ClampMagnitude(vector4, 4f);
				component.angularVelocity = Vector3.MoveTowards(component.angularVelocity, vector4, value) * num2;
				component.angularVelocity = Vector3.ClampMagnitude(component.angularVelocity, 4f);
			}
		}
	}

	private void StateLogic()
	{
		if (LevelGenerator.Instance.Generated)
		{
			if (cartActive != cartActivePrevious)
			{
				cartActivePrevious = cartActive;
			}
			if (physGrabObject.playerGrabbing.Count > 0)
			{
				draggedTimer += Time.deltaTime;
			}
			else
			{
				draggedTimer = 0f;
			}
			if (cartActive)
			{
				currentState = State.Handled;
			}
			else if (draggedTimer > 0.25f)
			{
				currentState = State.Dragged;
			}
			else
			{
				currentState = State.Locked;
			}
			if (currentState != previousState)
			{
				previousState = currentState;
				StateSwitch(currentState);
			}
		}
	}

	private void StateSwitch(State _state)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("StateSwitchRPC", RpcTarget.All, _state);
		}
		else
		{
			StateSwitchRPC(_state);
		}
	}

	[PunRPC]
	private void StateSwitchRPC(State _state)
	{
		currentState = _state;
		if (currentState == State.Locked)
		{
			soundLocked.Play(base.transform.position);
			cartMesh.material.SetColor("_EmissionColor", Color.red);
			foreach (Material item in grabMaterial)
			{
				Color red = Color.red;
				item.SetColor("_EmissionColor", red);
				item.mainTextureOffset = new Vector2(0f, 0f);
			}
			foreach (Collider capsuleCollider in capsuleColliders)
			{
				capsuleCollider.material = physMaterialNormal;
			}
			{
				foreach (Collider item2 in cartInside)
				{
					item2.material = physMaterialNormal;
				}
				return;
			}
		}
		if (currentState == State.Dragged)
		{
			soundDragged.Play(base.transform.position);
			Material material = cartMesh.material;
			Color value = new Color(1f, 0.46f, 0f);
			material.SetColor("_EmissionColor", value);
			foreach (Material item3 in grabMaterial)
			{
				item3.SetColor("_EmissionColor", value);
				item3.mainTextureOffset = new Vector2(0f, 0f);
			}
			foreach (Collider capsuleCollider2 in capsuleColliders)
			{
				capsuleCollider2.material = physMaterialALilSlippery;
			}
			{
				foreach (Collider item4 in cartInside)
				{
					item4.material = physMaterialALilSlippery;
				}
				return;
			}
		}
		soundHandled.Play(base.transform.position);
		cartMesh.material.SetColor("_EmissionColor", Color.green);
		int num = 0;
		foreach (Material item5 in grabMaterial)
		{
			item5.SetColor("_EmissionColor", Color.green);
			if (num == 1)
			{
				item5.mainTextureOffset = new Vector2(0.5f, 0f);
			}
			num++;
		}
		foreach (Collider capsuleCollider3 in capsuleColliders)
		{
			capsuleCollider3.material = physMaterialSlippery;
		}
		foreach (Collider item6 in cartInside)
		{
			item6.material = SemiFunc.PhysicMaterialPhysGrabObject();
		}
	}
}
