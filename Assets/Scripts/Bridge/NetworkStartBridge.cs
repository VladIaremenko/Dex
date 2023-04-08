using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkStartBridge : MonoBehaviour
{
	public NetworkDebugStart starter;
	public TMPro.TMP_InputField codeField;

	private void OnEnable()
	{
		codeField.text = starter.DefaultRoomName;
	}

	public void SetCode(string code)
	{
		starter.DefaultRoomName = code;
	}
	
	public void StartHost()
	{
		if (string.IsNullOrWhiteSpace(starter.DefaultRoomName))
			starter.DefaultRoomName = RoomCode.Create();
		starter.StartHost();
	}

	public void StartClient()
	{
		starter.StartClient();
	}

	public void Shutdown()
	{
		foreach (var runner in NetworkRunner.Instances)
		{
			runner.Shutdown();
		}
	}
}
