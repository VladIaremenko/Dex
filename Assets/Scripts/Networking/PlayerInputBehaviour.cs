using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputBehaviour : Fusion.Behaviour, INetworkRunnerCallbacks
{

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		var frameworkInput = new NetworkInputPrototype();

		if (PlayerMovement.Local && PlayerMovement.Local.activeInteractable == null
			&& GameManager.Instance
			&& (GameManager.State.Current == GameState.EGameState.Play
			|| GameManager.State.Current == GameState.EGameState.Pregame))
		{
			if (Input.GetKey(KeyCode.W))
			{
				frameworkInput.Buttons.Set(NetworkInputPrototype.BUTTON_FORWARD, true);
			}

			if (Input.GetKey(KeyCode.S))
			{
				frameworkInput.Buttons.Set(NetworkInputPrototype.BUTTON_BACKWARD, true);
			}

			if (Input.GetKey(KeyCode.A))
			{
				frameworkInput.Buttons.Set(NetworkInputPrototype.BUTTON_LEFT, true);
			}

			if (Input.GetKey(KeyCode.D))
			{
				frameworkInput.Buttons.Set(NetworkInputPrototype.BUTTON_RIGHT, true);
			}

			if (Input.GetKey(KeyCode.E))
			{
				frameworkInput.Buttons.Set(NetworkInputPrototype.BUTTON_ACTION1, true);
			}

			if (Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject() == false)
			{
				frameworkInput.Buttons.Set(NetworkInputPrototype.BUTTON_WALK, true);

				Vector2 mouseVec = new Vector2(Input.mousePosition.x / Screen.width - 0.5f, Input.mousePosition.y / Screen.height - 0.5f);
				frameworkInput.Yaw = Mathf.Atan2(mouseVec.y, mouseVec.x) * Mathf.Rad2Deg;
			}
		}

		input.Set(frameworkInput);
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnConnectedToServer(NetworkRunner runner) { }
	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	public void OnDisconnectedFromServer(NetworkRunner runner) { }
	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
	public void OnSceneLoadDone(NetworkRunner runner) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}