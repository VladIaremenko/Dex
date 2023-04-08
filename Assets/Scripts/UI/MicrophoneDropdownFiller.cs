using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TMPro;
using UnityEngine.UI;
using Photon.Voice.Unity;
using DeviceInfo = Photon.Voice.DeviceInfo;
using Photon.Voice;

public class MicrophoneDropdownFiller : VoiceComponent
{
	public Dropdown dropdown;
	public Recorder rec;

	readonly List<DeviceInfo> availableDevices = new List<DeviceInfo>();

	public void Init()
	{
		rec = GameManager.vm.Rec;
		FillDropdown();
		dropdown.onValueChanged.AddListener(index =>
		{
			rec.MicrophoneDevice = availableDevices[index];
		});
	}

	void FillDropdown()
	{
		availableDevices.Clear();
		dropdown.ClearOptions();

		List<string> opts = new List<string>();
		foreach(DeviceInfo item in Platform.CreateAudioInEnumerator(this.Logger))
		{
			availableDevices.Add(item);
			opts.Add(item.Name);
		}

		dropdown.AddOptions(opts);
	}
}
