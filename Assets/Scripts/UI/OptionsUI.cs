using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsUI : MonoBehaviour
{
	public Slider masterVol;
	public Slider sfxVol;
	public Slider uiVol;
	public Slider ambienceVol;
	public Slider voiceVol;

	public TMP_InputField nicknameInput;
	public Dropdown regionDropdown;

	private void OnEnable()
	{
		masterVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.mainVolumeParam));
		sfxVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.sfxVolumeParam));
		uiVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.uiVolumeParam));
		ambienceVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.ambienceVolumeParam));
		voiceVol.SetValueWithoutNotify(AudioManager.GetFloatNormalized(AudioManager.voiceVolumeParam));

		nicknameInput.interactable = regionDropdown.interactable = GameManager.Instance.Runner == null;
	}

	public void SetVolumeMaster(float value)
	{
		AudioManager.SetVolumeMaster(value);
	}

	public void SetVolumeSFX(float value)
	{
		AudioManager.SetVolumeSFX(value);
	}

	public void SetVolumeUI(float value)
	{
		AudioManager.SetVolumeUI(value);
	}

	public void SetVolumeAmbience(float value)
	{
		AudioManager.SetVolumeAmbience(value);
	}

	public void SetVolumeVoice(float value)
	{
		AudioManager.SetVolumeVoice(value);
	}
}
