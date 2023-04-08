using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorKiosk : Interactable
{
	public ColorSelectionUI colorUI;

	public override bool CanInteract => true;

	public override void Interact()
	{
		colorUI.Open();
	}
}
