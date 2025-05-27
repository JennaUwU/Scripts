using System;
using System.Collections.Generic;
using UnityEngine;

public class Materials : MonoBehaviour
{
	public enum Type
	{
		None = 0,
		Wood = 1,
		Rug = 2,
		Tile = 3,
		Stone = 4,
		Catwalk = 5,
		Snow = 6,
		Metal = 7,
		Wetmetal = 8,
		Gravel = 9,
		Grass = 10,
		Water = 11
	}

	public enum SoundType
	{
		Light = 0,
		Medium = 1,
		Heavy = 2
	}

	public enum HostType
	{
		LocalPlayer = 0,
		OtherPlayer = 1,
		Enemy = 2
	}

	[Serializable]
	public class MaterialTrigger
	{
		internal MaterialPreset LastMaterialList;

		internal Type LastMaterialType;

		internal MaterialSlidingLoop SlidingLoopObject;
	}

	public static Materials Instance;

	public LayerMask LayerMask;

	[Space]
	public List<MaterialPreset> MaterialList;

	private MaterialPreset LastMaterialList;

	private void Awake()
	{
		Instance = this;
	}

	public void Impulse(Vector3 origin, Vector3 direction, SoundType soundType, bool footstep, MaterialTrigger materialTrigger, HostType hostType)
	{
		Vector3 material = GetMaterial(origin, materialTrigger);
		if (!LastMaterialList)
		{
			return;
		}
		float volumeMultiplier = 1f;
		float falloffMultiplier = 1f;
		float offscreenVolumeMultiplier = 1f;
		float offscreenFalloffMultiplier = 1f;
		switch (hostType)
		{
		case HostType.OtherPlayer:
			volumeMultiplier = 0.5f;
			break;
		case HostType.Enemy:
			volumeMultiplier = 0.5f;
			falloffMultiplier = 0.5f;
			offscreenVolumeMultiplier = 0.25f;
			offscreenFalloffMultiplier = 0.25f;
			break;
		}
		switch (soundType)
		{
		case SoundType.Light:
			if (footstep)
			{
				if (LastMaterialList.RareFootstepLightMax > 0)
				{
					LastMaterialList.RareFootstepLightCurrent -= 1f;
					if (LastMaterialList.RareFootstepLightCurrent <= 0f)
					{
						LastMaterialList.RareFootstepLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
						LastMaterialList.RareFootstepLightCurrent = UnityEngine.Random.Range(LastMaterialList.RareFootstepLightMin, LastMaterialList.RareFootstepLightMax);
					}
				}
				LastMaterialList.FootstepLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
				break;
			}
			if (LastMaterialList.RareImpactLightMax > 0)
			{
				LastMaterialList.RareImpactLightCurrent -= 1f;
				if (LastMaterialList.RareImpactLightCurrent <= 0f)
				{
					LastMaterialList.RareImpactLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
					LastMaterialList.RareImpactLightCurrent = UnityEngine.Random.Range(LastMaterialList.RareImpactLightMin, LastMaterialList.RareImpactLightMax);
				}
			}
			LastMaterialList.ImpactLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
			break;
		case SoundType.Medium:
			if (footstep)
			{
				if (LastMaterialList.RareFootstepMediumMax > 0)
				{
					LastMaterialList.RareFootstepMediumCurrent -= 1f;
					if (LastMaterialList.RareFootstepMediumCurrent <= 0f)
					{
						LastMaterialList.RareFootstepMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
						LastMaterialList.RareFootstepMediumCurrent = UnityEngine.Random.Range(LastMaterialList.RareFootstepMediumMin, LastMaterialList.RareFootstepMediumMax);
					}
				}
				LastMaterialList.FootstepMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
				break;
			}
			if (LastMaterialList.RareImpactMediumMax > 0)
			{
				LastMaterialList.RareImpactMediumCurrent -= 1f;
				if (LastMaterialList.RareImpactMediumCurrent <= 0f)
				{
					LastMaterialList.RareImpactMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
					LastMaterialList.RareImpactMediumCurrent = UnityEngine.Random.Range(LastMaterialList.RareImpactMediumMin, LastMaterialList.RareImpactMediumMax);
				}
			}
			LastMaterialList.ImpactMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
			break;
		case SoundType.Heavy:
			if (footstep)
			{
				if (LastMaterialList.RareFootstepHeavyMax > 0)
				{
					LastMaterialList.RareFootstepHeavyCurrent -= 1f;
					if (LastMaterialList.RareFootstepHeavyCurrent <= 0f)
					{
						LastMaterialList.RareFootstepHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
						LastMaterialList.RareFootstepHeavyCurrent = UnityEngine.Random.Range(LastMaterialList.RareFootstepHeavyMin, LastMaterialList.RareFootstepHeavyMax);
					}
				}
				LastMaterialList.FootstepHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
				break;
			}
			if (LastMaterialList.RareImpactHeavyMax > 0)
			{
				LastMaterialList.RareImpactHeavyCurrent -= 1f;
				if (LastMaterialList.RareImpactHeavyCurrent <= 0f)
				{
					LastMaterialList.RareImpactHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
					LastMaterialList.RareImpactHeavyCurrent = UnityEngine.Random.Range(LastMaterialList.RareImpactHeavyMin, LastMaterialList.RareImpactHeavyMax);
				}
			}
			LastMaterialList.ImpactHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
			break;
		}
	}

	public void Slide(Vector3 origin, MaterialTrigger materialTrigger, float spatialBlend, bool isPlayer)
	{
		float volumeMultiplier = 1f;
		if (!isPlayer)
		{
			volumeMultiplier = 0.5f;
		}
		Vector3 material = GetMaterial(origin, materialTrigger);
		if ((bool)LastMaterialList)
		{
			LastMaterialList.SlideOneShot.SpatialBlend = spatialBlend;
			LastMaterialList.SlideOneShot.Play(material, volumeMultiplier);
		}
	}

	public void SlideLoop(Vector3 origin, MaterialTrigger materialTrigger, float spatialBlend, float pitchMultiplier)
	{
		Vector3 position = origin;
		bool flag = materialTrigger.SlidingLoopObject != null;
		if (!flag || materialTrigger.SlidingLoopObject.getMaterialTimer <= 0f)
		{
			position = GetMaterial(origin, materialTrigger);
			if (flag)
			{
				materialTrigger.SlidingLoopObject.getMaterialTimer = 0.25f;
			}
		}
		if (materialTrigger.LastMaterialList != null)
		{
			bool flag2 = false;
			if (!flag)
			{
				flag2 = true;
			}
			else if (materialTrigger.SlidingLoopObject.material != materialTrigger.LastMaterialList)
			{
				materialTrigger.SlidingLoopObject = null;
				flag2 = true;
			}
			if (flag2)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(AudioManager.instance.AudioMaterialSlidingLoop, position, Quaternion.identity, AudioManager.instance.SoundsParent);
				materialTrigger.SlidingLoopObject = gameObject.GetComponent<MaterialSlidingLoop>();
				materialTrigger.SlidingLoopObject.material = materialTrigger.LastMaterialList;
			}
			materialTrigger.SlidingLoopObject.activeTimer = 0.1f;
			materialTrigger.SlidingLoopObject.transform.position = position;
			materialTrigger.SlidingLoopObject.pitchMultiplier = pitchMultiplier;
		}
	}

	private Vector3 GetMaterial(Vector3 origin, MaterialTrigger materialTrigger)
	{
		origin = new Vector3(origin.x, origin.y + 0.1f, origin.z);
		Type _type = materialTrigger.LastMaterialType;
		if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1f, LayerMask, QueryTriggerInteraction.Collide))
		{
			MaterialSurface component = hitInfo.collider.gameObject.GetComponent<MaterialSurface>();
			if ((bool)component)
			{
				_type = component.Type;
				origin = hitInfo.point;
			}
		}
		LastMaterialList = MaterialList.Find((MaterialPreset x) => x.Type == _type);
		materialTrigger.LastMaterialType = _type;
		materialTrigger.LastMaterialList = LastMaterialList;
		return origin;
	}
}
