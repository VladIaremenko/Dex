using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsKiosk : Interactable
{
	public SettingsUI settingsUI;

	public override bool CanInteract => Runner.IsServer;

	public override void Interact()
	{
		settingsUI.Open();
	}
}
