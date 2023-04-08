using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public abstract class Interactable : NetworkBehaviour
{
	public bool isInteractionInstant = false;
	public bool isGhostAccessible = true;

	public abstract bool CanInteract { get; }

	public abstract void Interact();

	
}
