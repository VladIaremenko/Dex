using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Helpers.Bits;
using static GameState;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static GameManager Instance { get; private set; }
	public static GameState State { get; private set; }
    public static ResourcesManager rm { get; private set; }
	public static InterfaceManager im { get; private set; }
	public static VoiceManager vm { get; private set; }

	public NetworkDebugStart starter;

	public UIScreen pauseScreen;
	public UIScreen optionsScreen;

	public MapData mapData;

	public CustomGroundKCCProcessor groundProcessorPrefab;

	[Networked]
	public PlayerObject MeetingCaller { get; set; }

	[Networked]
	public PlayerObject MeetingContext { get; set; }

	[Networked]
	public PlayerObject VoteResult { get; set; }

	[Networked(OnChanged = nameof(GameSettingsChanged))]
	public GameSettings Settings { get; set; } = GameSettings.Default;

	[Networked(OnChanged = nameof(TasksCompletedChanged))]
	public byte TasksCompleted { get; set; }

	public List<TaskStation> localTasks;
	public readonly Dictionary<TaskBase, byte> taskAmounts = new Dictionary<TaskBase, byte>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            rm = GetComponent<ResourcesManager>();
            im = GetComponent<InterfaceManager>();
			vm = GetComponent<VoiceManager>();
			State = GetComponent<GameState>();
        }
        else
        {
			Destroy(gameObject);
        }
    }

	public override void Spawned()
	{
		base.Spawned();
		vm.Init(
			Runner.GetComponent<Photon.Voice.Unity.VoiceConnection>(),
			Runner.GetComponentInChildren<Photon.Voice.Unity.Recorder>()
		);

		if (Runner.IsServer)
		{
			State.Server_SetState(EGameState.Pregame);
		}

		Runner.AddCallbacks(this);
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		base.Despawned(runner, hasState);
		runner.RemoveCallbacks(this);
		starter.Shutdown();
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();

		if (Runner.IsForward && Runner.Simulation.Tick % 100 == 0)
			im.gameUI.pingText.text = $"Ping: {1000 * Runner.GetPlayerRtt(Runner.LocalPlayer):N0}ms";
	}

	public void Server_StartGame()
	{
		if (Runner.IsServer == false)
		{
			Debug.LogWarning("This method is server-only");
			return;
		}

		if (State.Current != EGameState.Pregame) return;

		if (PlayerRegistry.Count < 4)
		{
			Debug.LogWarning($"It's recommended to play with at least 4 people!");
		}

		State.Server_SetState(EGameState.Play);
	}

	// this method is called only when there is a murder, not a vote ejection
	public static void OnPlayerKilled()
	{
		int numCrew = PlayerRegistry.CountWhere(p => !p.Controller.IsDead && p.Controller.IsSuspect == false);
		int numSus = PlayerRegistry.CountWhere(p => !p.Controller.IsDead && p.Controller.IsSuspect == true);

		if (numCrew <= numSus)
		{
			State.Server_DelaySetState(EGameState.ImpostorWin, 1);
		}
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	public void Rpc_CallMeeting(PlayerRef source, NetworkObject context, RpcInfo info = default)
	{
		PlayerObject caller = PlayerRegistry.GetPlayer(source);

		if (context == null)
		{
			if (caller.Controller.EmergencyMeetingUses > 0)
			{
				caller.Controller.EmergencyMeetingUses--;
				MeetingContext = null;
				MeetingCaller = caller;
				State.Server_SetState(EGameState.Meeting);

				im.gameUI.meetingUI.Server_SetTimer(info.Tick);
			}
			else
			{
				Debug.Log($"{caller.Nickname} is out of emergency meeting calls");
			}
		}
		else if (context.TryGetBehaviour(out DeadPlayer body))
		{
			MeetingContext = PlayerRegistry.GetPlayer(body.Ref);
			MeetingCaller = caller;
			State.Server_SetState(EGameState.Meeting);

			im.gameUI.meetingUI.Server_SetTimer(info.Tick);
			Runner.Despawn(body.Object);
		}
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	void Rpc_CompleteTask()
	{
		TasksCompleted++;

		if (TasksCompleted == Settings.numTasks * (PlayerRegistry.Count - Settings.numImposters))
		{
			Debug.Log("All Tasks Completed - Crew Wins");
			State.Server_SetState(EGameState.CrewWin);
		}
		else
		{
			Debug.Log($"{TasksCompleted} tasks completed");
		}
	}

	public void CompleteTask()
	{
		TaskStation task = PlayerMovement.Local.activeInteractable as TaskStation;
		Debug.LogWarning(task);
		if (localTasks.Remove(task))
		{
			im.gameUI.UpdateTaskUI();
			Rpc_CompleteTask();
		}
	}

	public bool HasTask(TaskStation task)
	{
		return localTasks.Contains(task);
	}

	public IEnumerable<TaskStation> Tasks()
	{
		return localTasks.AsEnumerable();
	}

	public IEnumerable<string> TaskNames()
	{
		return localTasks.Select(task => task.taskUI.task.Name);
	}

	public static void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
	
	static void GameSettingsChanged(Changed<GameManager> changed)
	{
		GameSettings settings = changed.Behaviour.Settings;
		
		im.imposterCountDisplay.text = $"{settings.numImposters}";
		im.tasksCountDisplay.text = $"{settings.numTasks}";
		im.emergencyMeetingsDisplay.text = $"{settings.numEmergencyMeetings}";
		im.discussionTimeDisplay.text = $"{settings.discussionTime}s";
		im.votingTimeDisplay.text = $"{settings.votingTime}s";
		im.walkSpeedDisplay.text = $"{settings.walkSpeed}";
		im.playerCollisionDisplay.text = settings.playerCollision ? "Yes" : "No";

		if (Instance.Runner.IsServer)
		{
			int playerLayer = LayerMask.NameToLayer("Player");
			PlayerRegistry.ForEach(p =>
			{
				p.Controller.cc.Settings.CollisionLayerMask.value =
					p.Controller.cc.Settings.CollisionLayerMask.value.OverrideBit(playerLayer, settings.playerCollision);
				p.Controller.cc.GetProcessor<CustomGroundKCCProcessor>().KinematicSpeed = settings.walkSpeed;
			});
			changed.Behaviour.groundProcessorPrefab.KinematicSpeed = settings.walkSpeed;
		}
	}

	static void TasksCompletedChanged(Changed<GameManager> changed)
	{
		GameManager self = changed.Behaviour;
		float pct = self.TasksCompleted /
			(float)(self.Settings.numTasks * (PlayerRegistry.Count - Instance.Settings.numImposters));

		im.gameUI.totalTaskBarFill.fillAmount = pct;
	}
	
	void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
	{
		UIScreen.CloseAll();
	}

	void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, Fusion.Sockets.NetAddress remoteAddress, Fusion.Sockets.NetConnectFailedReason reason) { }
	void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
	void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner) { }
	void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
	void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
	void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
	void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
	void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}
