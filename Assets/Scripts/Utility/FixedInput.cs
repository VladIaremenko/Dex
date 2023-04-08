using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedInput
{
	readonly HashSet<KeyCode> setDown = new HashSet<KeyCode>();
	readonly HashSet<KeyCode> setUp = new HashSet<KeyCode>();

	public void PollDown(KeyCode key)
	{
		if (Input.GetKeyDown(key)) setDown.Add(key);
	}

	public void PollUp(KeyCode key)
	{
		if (Input.GetKeyUp(key)) setUp.Add(key);
	}

	public bool GetDown(KeyCode key)
	{
		return setDown.Contains(key);
	}

	public bool GetUp(KeyCode key)
	{
		return setUp.Contains(key);
	}

	public void Clear()
	{
		setDown.Clear();
		setUp.Clear();
	}
}
