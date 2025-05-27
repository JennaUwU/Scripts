using UnityEngine;

public class AmbienceLoop : MonoBehaviour
{
	public static AmbienceLoop instance;

	public AudioSource source;

	private AudioClip clip;

	private float volume;

	[Space]
	public AnimationCurve roomCurve;

	public float roomLerpSpeed = 1f;

	private float roomLerpAmount;

	private float roomVolumePrevious;

	private float roomVolumeCurrent;

	private RoomAmbience roomAmbience;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (PlayerController.instance.playerAvatarScript.RoomVolumeCheck.CurrentRooms.Count > 0)
		{
			RoomAmbience roomAmbience = PlayerController.instance.playerAvatarScript.RoomVolumeCheck.CurrentRooms[0].RoomAmbience;
			if ((bool)roomAmbience && roomAmbience != this.roomAmbience)
			{
				this.roomAmbience = roomAmbience;
				roomLerpAmount = 0f;
				roomVolumePrevious = roomVolumeCurrent;
			}
		}
		if ((bool)this.roomAmbience)
		{
			if (roomLerpAmount < 1f)
			{
				roomLerpAmount += roomLerpSpeed * Time.deltaTime;
				float t = roomCurve.Evaluate(roomLerpAmount);
				roomVolumeCurrent = Mathf.Lerp(roomVolumePrevious, this.roomAmbience.volume, t);
			}
			source.volume = volume * roomVolumeCurrent;
		}
	}

	public void Setup()
	{
		foreach (LevelAmbience ambiencePreset in LevelGenerator.Instance.Level.AmbiencePresets)
		{
			if ((bool)ambiencePreset.loopClip)
			{
				clip = ambiencePreset.loopClip;
				volume = ambiencePreset.loopVolume;
			}
		}
		source.clip = clip;
		source.volume = 0f;
		source.loop = true;
		source.Play();
	}

	public void LiveUpdate()
	{
		foreach (LevelAmbience ambiencePreset in LevelGenerator.Instance.Level.AmbiencePresets)
		{
			if ((bool)ambiencePreset.loopClip)
			{
				clip = ambiencePreset.loopClip;
				volume = ambiencePreset.loopVolume;
			}
		}
		source.volume = volume;
	}
}
