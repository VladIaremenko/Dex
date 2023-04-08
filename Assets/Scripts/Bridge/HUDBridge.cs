using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDBridge : MonoBehaviour
{
	public void Use()
	{
		PlayerMovement.Local.TryUse(true);
		//PlayerMovement.Local.InvokeActionDeferred(() => PlayerMovement.Local.TryUse(true));
	}

	public void Report()
	{
		PlayerMovement.Local.TryUse(false);
	}

	public void Kill()
	{
		PlayerMovement.Local.TryKill();
	}
}
