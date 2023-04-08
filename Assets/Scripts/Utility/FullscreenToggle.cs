using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenToggle : MonoBehaviour
{
	public KeyCode fullscreenKey = KeyCode.Escape;

	private void Update()
	{
		if (Input.GetKeyDown(fullscreenKey))
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
	}
}
