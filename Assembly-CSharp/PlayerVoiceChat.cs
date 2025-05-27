using System;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

[Serializable]
public class PlayerVoiceChat : MonoBehaviour
{
	public GameObject TTSprefab;

	public static PlayerVoiceChat instance;

	[FormerlySerializedAs("textToSpeech")]
	public TTSVoice ttsVoice;

	public AudioClip debugClip;

	internal bool debug;

	private float debugTalkingTimer;

	private float debugTalkingCooldown;

	internal bool inLobbyMixer;

	internal bool inLobbyMixerTTS;

	internal bool isTalking;

	private bool isTalkingPrevious;

	private float isTalkingTimer;

	internal float isTalkingStartTime;

	internal float voiceGain = 0.5f;

	internal PhotonView photonView;

	internal PlayerAvatar playerAvatar;

	internal AudioSource audioSource;

	private Recorder recorder;

	private Speaker speaker;

	[Space]
	public AudioMixerGroup mixerMicrophoneSound;

	public AudioMixerGroup mixerMicrophoneSpectate;

	public AudioMixerGroup mixerTTSSound;

	public AudioMixerGroup mixerTTSSpectate;

	[Space]
	public AudioMixerSnapshot volumeOff;

	public AudioMixerSnapshot volumeOn;

	[Space]
	public AudioLowPassLogic lowPassLogic;

	public AudioLowPassLogic lowPassLogicTTS;

	private float SpatialDisableTimer;

	private int sampleDataLength = 1024;

	internal float clipLoudnessNoTTS;

	internal float clipLoudnessTTS;

	internal float clipLoudness;

	private float[] clipSampleData;

	private float clipCheckTimer;

	private string currentDeviceName = "";

	private float investigateTimer;

	public AudioSource ttsAudioSource;

	private float[] ttsAudioSpectrum = new float[1024];

	internal bool TTSinstantiated;

	private float TTSinstantiatedTimer;

	private float TTSPitchChangeTimer;

	private float TTSPitchChangeTarget;

	private float TTSPitchChange;

	private float TTSPitchChangeSpeed;

	private float switchDeviceTimer;

	private int microphoneVolumeSetting = -1;

	private int microphoneVolumeSettingPrevious = -1;

	internal float microphoneVolumeMultiplier = 1f;

	private float pitchMultiplier = 1f;

	private float overridePitchTimer;

	private float overridePitchMultiplierTarget = 1f;

	private float overridePitchSpeedIn;

	private float overridePitchSpeedOut;

	private float overridePitchLerp;

	private float overridePitchTime;

	private bool overridePitchIsActive;

	private float currentBoost;

	private float overrideAddToClipLoudnessTimer;

	private float overrideAddToClipLoudness;

	internal bool recordingEnabled;

	internal bool microphoneEnabled;

	private bool microphoneEnabledPrevious;

	private void Awake()
	{
		RunManager.instance.voiceChats.Add(this);
	}

	private void Start()
	{
		clipSampleData = new float[sampleDataLength];
		audioSource = GetComponent<AudioSource>();
		photonView = GetComponent<PhotonView>();
		recorder = GetComponent<Recorder>();
		speaker = GetComponent<Speaker>();
		if (photonView.IsMine)
		{
			if ((bool)instance)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			instance = this;
			audioSource.volume = 0f;
			voiceGain = 0f;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		ToggleLobby(_toggle: true);
	}

	public void OverrideClipLoudnessAnimationValue(float _value)
	{
		overrideAddToClipLoudness = _value;
		overrideAddToClipLoudnessTimer = 0.1f;
	}

	private void OverrideClipLoudnessAnimationValueTick()
	{
		if (overrideAddToClipLoudnessTimer > 0f)
		{
			overrideAddToClipLoudnessTimer -= Time.deltaTime;
		}
		else
		{
			overrideAddToClipLoudness = 0f;
		}
	}

	private void FixedUpdate()
	{
		OverridePitchTick();
		OverrideClipLoudnessAnimationValueTick();
	}

	private void Update()
	{
		OverridePitchLogic();
		if (photonView.IsMine)
		{
			microphoneVolumeSetting = DataDirector.instance.SettingValueFetch(DataDirector.Setting.MicVolume);
			if (microphoneVolumeSetting != microphoneVolumeSettingPrevious)
			{
				microphoneVolumeSettingPrevious = microphoneVolumeSetting;
				photonView.RPC("MicrophoneVolumeSettingRPC", RpcTarget.OthersBuffered, microphoneVolumeSetting);
			}
		}
		microphoneVolumeMultiplier = (float)microphoneVolumeSetting * 0.01f;
		if (!TTSinstantiated && (bool)playerAvatar)
		{
			if (TTSinstantiatedTimer > 3f && (PunVoiceClient.Instance.Client.State == ClientState.Joined || PunVoiceClient.Instance.Client.State == ClientState.Disconnected))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(TTSprefab, base.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				ttsVoice = gameObject.GetComponent<TTSVoice>();
				ttsAudioSource = ttsVoice.GetComponent<AudioSource>();
				lowPassLogicTTS = ttsAudioSource.GetComponent<AudioLowPassLogic>();
				lowPassLogicTTS.Fetch = true;
				ttsVoice.playerAvatar = playerAvatar;
				if (playerAvatar.isLocal)
				{
					recorder.RecordingEnabled = true;
				}
				photonView.RPC("RecordingEnabled", RpcTarget.AllBuffered);
				TTSinstantiated = true;
			}
			else
			{
				TTSinstantiatedTimer += Time.deltaTime;
			}
		}
		if (TTSinstantiated && playerAvatar.isLocal)
		{
			microphoneEnabledPrevious = microphoneEnabled;
			microphoneEnabled = false;
			if (currentDeviceName != "NONE")
			{
				string[] devices = Microphone.devices;
				for (int i = 0; i < devices.Length; i++)
				{
					if (devices[i] == currentDeviceName)
					{
						microphoneEnabled = true;
						break;
					}
				}
			}
			if (currentDeviceName == "" || currentDeviceName != SessionManager.instance.micDeviceCurrent || microphoneEnabled != microphoneEnabledPrevious)
			{
				currentDeviceName = SessionManager.instance.micDeviceCurrent;
				if (!microphoneEnabled && currentDeviceName != "")
				{
					recorder.MicrophoneDevice = new DeviceInfo("");
				}
				else if (currentDeviceName != "NONE")
				{
					recorder.MicrophoneDevice = new DeviceInfo(currentDeviceName);
				}
			}
		}
		if (clipCheckTimer <= 0f)
		{
			clipCheckTimer = 0.001f;
			clipLoudness = 0f;
			clipLoudnessTTS = 0f;
			clipLoudnessNoTTS = 0f;
			if ((bool)audioSource && (bool)audioSource.clip && audioSource.isPlaying)
			{
				audioSource.clip.GetData(clipSampleData, audioSource.timeSamples);
				float[] array = clipSampleData;
				foreach (float f in array)
				{
					clipLoudness += Mathf.Abs(f);
				}
				clipLoudness /= sampleDataLength;
				clipLoudnessNoTTS = clipLoudness;
			}
			clipLoudness *= microphoneVolumeMultiplier;
			clipLoudnessNoTTS *= microphoneVolumeMultiplier;
			clipLoudness += overrideAddToClipLoudness;
			if ((bool)ttsVoice && ttsAudioSource.isPlaying)
			{
				ttsAudioSource.GetSpectrumData(ttsAudioSpectrum, 0, FFTWindow.BlackmanHarris);
				float num = (clipLoudnessTTS = Mathf.Max(ttsAudioSpectrum) * 2f);
				if (num > clipLoudness)
				{
					clipLoudness = num;
				}
			}
		}
		else
		{
			clipCheckTimer -= Time.deltaTime;
		}
		if (photonView.IsMine)
		{
			if (!debug)
			{
				if (clipLoudness > 0.005f)
				{
					isTalking = true;
					isTalkingTimer = 0.5f;
				}
			}
			else if (debugTalkingTimer > 0f)
			{
				debugTalkingTimer -= Time.deltaTime;
				isTalkingTimer = 1f;
				isTalking = true;
				if (debugTalkingTimer <= 0f)
				{
					debugTalkingCooldown = UnityEngine.Random.Range(3f, 10f);
				}
			}
			else
			{
				debugTalkingCooldown -= Time.deltaTime;
				if (debugTalkingCooldown <= 0f)
				{
					debugTalkingTimer = UnityEngine.Random.Range(1f, 6f);
				}
			}
			if (isTalkingTimer > 0f)
			{
				isTalkingTimer -= Time.deltaTime;
				if (isTalkingTimer <= 0f)
				{
					isTalking = false;
				}
			}
			if (isTalking != isTalkingPrevious)
			{
				isTalkingPrevious = isTalking;
				if (isTalking)
				{
					isTalkingStartTime = Time.time;
				}
				photonView.RPC("IsTalkingRPC", RpcTarget.Others, isTalking);
			}
		}
		if (debug)
		{
			if (isTalking)
			{
				lowPassLogic.Volume = Mathf.Lerp(lowPassLogic.Volume, 1f, Time.deltaTime * 20f);
				if ((bool)lowPassLogicTTS)
				{
					lowPassLogicTTS.Volume = lowPassLogic.Volume;
				}
			}
			else
			{
				lowPassLogic.Volume = Mathf.Lerp(lowPassLogic.Volume, 0f, Time.deltaTime * 20f);
				if ((bool)lowPassLogicTTS)
				{
					lowPassLogicTTS.Volume = lowPassLogic.Volume;
				}
			}
		}
		if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
		{
			if ((bool)playerAvatar && !playerAvatar.isDisabled)
			{
				bool flag = false;
				if (playerAvatar.isCrawling)
				{
					if (clipLoudness > 0.15f)
					{
						flag = true;
					}
				}
				else if (playerAvatar.isCrouching)
				{
					if (clipLoudness > 0.05f)
					{
						flag = true;
					}
				}
				else if (clipLoudness > 0.025f)
				{
					flag = true;
				}
				if (flag && investigateTimer <= 0f)
				{
					investigateTimer = 1f;
					EnemyDirector.instance.SetInvestigate(playerAvatar.PlayerVisionTarget.VisionTransform.transform.position, 5f);
				}
			}
			if (investigateTimer >= 0f)
			{
				investigateTimer -= Time.deltaTime;
			}
		}
		if (SpatialDisableTimer > 0f || inLobbyMixer || photonView.IsMine)
		{
			audioSource.spatialBlend = 0f;
			SpatialDisableTimer -= Time.deltaTime;
		}
		else
		{
			audioSource.spatialBlend = 1f;
		}
		float volume = 0f;
		if (!photonView.IsMine)
		{
			volume = voiceGain * microphoneVolumeMultiplier;
		}
		lowPassLogic.Volume = volume;
		if ((bool)lowPassLogicTTS && (bool)playerAvatar)
		{
			if (playerAvatar.isCrouching)
			{
				lowPassLogicTTS.Volume = 0.8f;
			}
			else
			{
				lowPassLogicTTS.Volume = 1f;
			}
		}
		if (!TTSinstantiated || !playerAvatar.isLocal)
		{
			return;
		}
		if (!microphoneEnabled)
		{
			recorder.TransmitEnabled = false;
		}
		else if (AudioManager.instance.pushToTalk)
		{
			if (SemiFunc.InputHold(InputKey.PushToTalk))
			{
				recorder.TransmitEnabled = true;
			}
			else
			{
				recorder.TransmitEnabled = false;
			}
		}
		else
		{
			recorder.TransmitEnabled = true;
		}
	}

	private void LateUpdate()
	{
		TtsFollowVoiceSettings();
	}

	private void OnDestroy()
	{
		RunManager.instance.voiceChats.Remove(this);
	}

	private void TtsFollowVoiceSettings()
	{
		if (!ttsVoice || !playerAvatar)
		{
			return;
		}
		if (playerAvatar.isCrouching || playerAvatar.isCrawling)
		{
			ttsVoice.setVoice(1);
		}
		else
		{
			ttsVoice.setVoice(0);
		}
		if (SemiFunc.IsMultiplayer())
		{
			float num = Mathf.Lerp(0.7f, 1.3f, (playerAvatar.PlayerVisionTarget.VisionTransform.transform.forward.y + 1f) / 1.5f) + TTSPitchChange;
			num *= pitchMultiplier;
			if (playerAvatar.isDisabled)
			{
				num = 1f;
			}
			ttsAudioSource.pitch = audioSource.pitch * num;
			ttsAudioSource.spatialBlend = audioSource.spatialBlend;
		}
		if (inLobbyMixer != inLobbyMixerTTS)
		{
			inLobbyMixerTTS = inLobbyMixer;
			if (inLobbyMixer)
			{
				ttsAudioSource.outputAudioMixerGroup = mixerTTSSpectate;
			}
			else
			{
				ttsAudioSource.outputAudioMixerGroup = mixerTTSSound;
			}
		}
	}

	public void SetDebug()
	{
		debug = true;
		audioSource.Stop();
		audioSource.clip = debugClip;
		audioSource.time = UnityEngine.Random.Range(0f, debugClip.length);
		audioSource.loop = true;
		if (photonView.IsMine)
		{
			audioSource.pitch = 0.8f * pitchMultiplier;
			audioSource.volume = 0.3f;
			lowPassLogic.Volume = 0.3f;
		}
		else
		{
			audioSource.pitch = 1.25f * pitchMultiplier;
		}
		audioSource.Play();
	}

	public void ToggleLobby(bool _toggle)
	{
		if (_toggle)
		{
			inLobbyMixer = true;
			if (photonView.IsMine)
			{
				volumeOn.TransitionTo(0.1f);
			}
			base.transform.position = new Vector3(1000f, 1000f, 1000f);
			audioSource.outputAudioMixerGroup = mixerMicrophoneSpectate;
			return;
		}
		inLobbyMixer = false;
		if (photonView.IsMine)
		{
			volumeOff.TransitionTo(0.1f);
			if ((bool)AudioManager.instance)
			{
				AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.On, 0.5f);
			}
		}
		audioSource.outputAudioMixerGroup = mixerMicrophoneSound;
		if ((bool)ttsAudioSource)
		{
			ttsAudioSource.outputAudioMixerGroup = mixerTTSSound;
		}
	}

	[PunRPC]
	public void IsTalkingRPC(bool _isTalking)
	{
		isTalking = _isTalking;
		if (isTalking)
		{
			isTalkingStartTime = Time.time;
		}
	}

	[PunRPC]
	public void MicrophoneVolumeSettingRPC(int _volume)
	{
		microphoneVolumeSetting = _volume;
	}

	[PunRPC]
	public void RecordingEnabled()
	{
		recordingEnabled = true;
	}

	public void SpatialDisable(float _time)
	{
		if (!photonView.IsMine)
		{
			SpatialDisableTimer = _time;
		}
	}

	public void OverridePitch(float _multiplier, float _timeIn, float _timeOut, float _overrideTimer = 0.1f, bool doRPC = true)
	{
		float num = overridePitchMultiplierTarget;
		overridePitchMultiplierTarget = _multiplier;
		overridePitchSpeedIn = _timeIn;
		overridePitchSpeedOut = _timeOut;
		overridePitchTimer = _overrideTimer;
		overridePitchTime = _overrideTimer;
		overridePitchIsActive = true;
		if (overridePitchIsActive && num < 0f && overridePitchMultiplierTarget < num)
		{
			overridePitchIsActive = false;
		}
		if (overridePitchIsActive && num > 0f && overridePitchMultiplierTarget > num)
		{
			overridePitchIsActive = false;
		}
	}

	public void OverridePitchCancel()
	{
		overridePitchMultiplierTarget = 1f;
		overridePitchSpeedIn = 0.1f;
		overridePitchSpeedOut = 0.1f;
		overridePitchTimer = 0f;
		overridePitchTime = 0f;
		overridePitchIsActive = false;
	}

	private void OverridePitchTick()
	{
		if (overridePitchTimer <= 0f)
		{
			overridePitchIsActive = false;
		}
		if (overridePitchTimer > 0f)
		{
			overridePitchIsActive = true;
			overridePitchTimer -= Time.fixedDeltaTime;
		}
	}

	private void OverridePitchLogic()
	{
		audioSource.pitch = pitchMultiplier;
		if (!overridePitchIsActive && overridePitchLerp < 0.05f)
		{
			pitchMultiplier = 1f;
			return;
		}
		if (overridePitchTimer > 0f)
		{
			overridePitchLerp += Time.deltaTime / overridePitchSpeedIn;
			if (overridePitchLerp > 1f)
			{
				overridePitchLerp = 1f;
			}
		}
		else
		{
			overridePitchLerp -= Time.deltaTime / overridePitchSpeedOut;
			if (overridePitchLerp < 0f)
			{
				overridePitchLerp = 0f;
			}
		}
		pitchMultiplier = Mathf.Lerp(1f, overridePitchMultiplierTarget, overridePitchLerp);
	}
}
