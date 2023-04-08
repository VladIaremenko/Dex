using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class EmergencyButton : Interactable
{
	public override bool CanInteract => PlayerMovement.Local.IsDead == false && PlayerMovement.Local.EmergencyMeetingUses > 0;

	public override void Interact()
	{
		GameManager.Instance.Rpc_CallMeeting(Runner.LocalPlayer, null);
	}
}
