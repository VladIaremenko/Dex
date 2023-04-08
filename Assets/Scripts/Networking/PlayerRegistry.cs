using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Helpers.Collections;
using System.Linq;

public class PlayerRegistry : NetworkBehaviour, INetworkRunnerCallbacks
{
	public const byte CAPACITY = 10;

	public static PlayerRegistry Instance { get; private set; }

	public static int Count => Instance.ObjectByRef.Count;

	[Networked, Capacity(CAPACITY)]
	NetworkDictionary<PlayerRef, PlayerObject> ObjectByRef { get; }

	public override void Spawned()
	{
		base.Spawned();
		Instance = this;
		Runner.AddCallbacks(this);
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		base.Despawned(runner, hasState);
		Instance = null;
		runner.RemoveCallbacks(this);
	}

	bool GetAvailable(out byte index)
	{
		if (ObjectByRef.Count == 0)
		{
			index = 0;
			return true;
		}
		else if (ObjectByRef.Count == CAPACITY)
		{
			index = default;
			return false;
		}

		byte[] indices = ObjectByRef.OrderBy(kvp => kvp.Value.Index).Select(kvp => kvp.Value.Index).ToArray();
		
		for (int i = 0; i < indices.Length - 1; i++)
		{
			if (indices[i + 1] > indices[i] + 1)
			{
				index = (byte)(indices[i] + 1);
				return true;
			}
		}

		index = (byte)(indices[indices.Length - 1] + 1);
		return true;
	}

	public static void Server_Add(NetworkRunner runner, PlayerRef pRef, PlayerObject pObj)
	{
		Debug.Assert(runner.IsServer);

		if (Instance.GetAvailable(out byte index))
		{
			byte colIndex = GetRandomColor();
			Instance.ObjectByRef.Add(pRef, pObj);
			pObj.Server_Init(pRef, index, colIndex);
		}
		else
		{
			Debug.LogWarning($"Unable to register player {pRef}", pObj);
		}
	}

	public static void Server_Remove(NetworkRunner runner, PlayerRef pRef)
	{
		Debug.Assert(runner.IsServer);
		Debug.Assert(pRef.IsValid);

		if (Instance.ObjectByRef.Remove(pRef) == false)
		{
			Debug.LogWarning("Could not remove player from registry");
		}
	}

	public static bool HasPlayer(PlayerRef pRef)
	{
		return Instance.ObjectByRef.ContainsKey(pRef);
	}

	public static PlayerObject GetPlayer(PlayerRef pRef)
	{
		if (HasPlayer(pRef))
			return Instance.ObjectByRef.Get(pRef);
		return null;
	}

	public static IEnumerable<PlayerObject> Where(System.Predicate<PlayerObject> match)
	{
		return Instance.ObjectByRef.Where(kvp => match.Invoke(kvp.Value)).Select(kvp => kvp.Value);
	}

	public static void ForEach(System.Action<PlayerObject> action)
	{
		foreach(var kvp in Instance.ObjectByRef)
		{
			action.Invoke(kvp.Value);
		}
	}

	public static void ForEachWhere(System.Predicate<PlayerObject> match, System.Action<PlayerObject> action)
	{
		foreach (var kvp in Instance.ObjectByRef)
		{
			if (match.Invoke(kvp.Value))
				action.Invoke(kvp.Value);
		}
	}

	public static int CountWhere(System.Predicate<PlayerObject> match)
	{
		int count = 0;
		foreach (var kvp in Instance.ObjectByRef)
		{
			if (match.Invoke(kvp.Value))
				count++;
		}
		return count;
	}

	public static bool Any(System.Predicate<PlayerObject> match)
	{
		foreach (var kvp in Instance.ObjectByRef)
		{
			if (match.Invoke(kvp.Value)) return true;
		}
		return false;
	}

	public static PlayerObject GetRandom()
	{
		byte index = (byte)Random.Range(0, Count);
		byte i = 0;
		foreach (var kvp in Instance.ObjectByRef)
		{
			PlayerObject pObj = kvp.Value;
			if (pObj != null)
			{
				if (i == index) return pObj;
				i++;
			}
		}
		throw new System.Exception("Something went inexplicably wrong");
	}

	public static PlayerObject[] GetRandom(int count)
	{
		List<PlayerObject> players = new List<PlayerObject>();
		foreach (var kvp in Instance.ObjectByRef)
		{
			players.Add(kvp.Value);
		}
		
		return players.Grab(count).ToArray();
	}

	public static byte[] GetAvailableColors()
	{
		List<byte> available = new List<byte>();
		for (byte i = 1; i < GameManager.rm.playerColours.Length; i++) available.Add(i);

		foreach (var item in Instance.ObjectByRef)
		{
			available.Remove(item.Value.ColorIndex);
		}

		return available.ToArray();
	}

	public static bool IsColorAvailable(byte c)
	{
		foreach (var item in Instance.ObjectByRef)
		{
			if (item.Value.ColorIndex == c) return false;
		}
		return true;
	}

	static byte GetRandomColor()
	{
		return GetAvailableColors().RandomElement();
	}

	void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		if (runner.IsServer) Server_Remove(runner, player);
	}

	#region INetworkRunnerCallbacks
	void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
	void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
	void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
	void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner) { }
	void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, Fusion.Sockets.NetAddress remoteAddress, Fusion.Sockets.NetConnectFailedReason reason) { }
	void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
	void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
	void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
	void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	#endregion
}
