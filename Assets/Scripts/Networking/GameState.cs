using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class GameState : NetworkBehaviour
{
	public enum EGameState { Off, Pregame, Play, Meeting, VoteResults, CrewWin, ImpostorWin }

	[Networked] public EGameState Previous { get; set; }
	[Networked] public EGameState Current { get; set; }

	[Networked] TickTimer Delay { get; set; }
	[Networked] EGameState DelayedState { get; set; }

	protected StateMachine<EGameState> StateMachine = new StateMachine<EGameState>();

	public override void Spawned()
	{
		StateMachine[EGameState.Off].onExit = newState =>
		{
			Debug.Log($"Exited {EGameState.Off} to {newState}");

			if (Runner.IsServer) { }

			if (Runner.IsPlayer) // [PLAYER] Off -> *
			{
				GameManager.im.gameUI.InitPregame(Runner);
			}
		};

		StateMachine[EGameState.Pregame].onEnter = state =>
		{
			Debug.Log($"Entered {EGameState.Pregame} from {state}");

			if (Runner.IsServer) // [SERVER] * -> Pregame
			{
				PlayerRegistry.ForEach(pObj =>
				{
					pObj.Controller.IsDead = false;
					pObj.Controller.IsSuspect = false;
					pObj.Controller.cc.SetPosition(Vector3.zero);
					pObj.Controller.EndInteraction();
					pObj.Controller.Server_UpdateDeadState();
				});

				GameManager.rm.Purge();
			}

			if (Runner.IsPlayer) // [PLAYER] * -> Pregame
			{
				GameManager.Instance.TasksCompleted = 0;
				GameManager.Instance.localTasks.Clear();
				GameManager.Instance.taskAmounts.Clear();

				GameManager.im.gameUI.InitPregame(Runner);
				GameManager.im.gameUI.colorUI.Init();

				if (TaskUI.ActiveUI) TaskUI.ActiveUI.CloseTask();
			}
		};

		StateMachine[EGameState.Play].onEnter = state =>
		{
			Debug.Log($"Entered {EGameState.Play} from {state}");

			if (Runner.IsServer) // [SERVER] * -> Play
			{
				PlayerRegistry.ForEach(
					obj => obj.Controller.cc.SetPosition(GameManager.Instance.mapData.GetSpawnPosition(obj.Index)));

				PlayerRegistry.ForEachWhere(
					p => p.Controller.IsSuspect,
					p => p.Controller.KillTimer = TickTimer.CreateFromSeconds(GameManager.Instance.Runner, 30));


				if (state == EGameState.Pregame) // [SERVER] Pregame -> Play
				{
					if (GameManager.Instance.Settings.numImposters > 0)
					{
						PlayerObject[] objs = PlayerRegistry.GetRandom(GameManager.Instance.Settings.numImposters);
						foreach (PlayerObject p in objs)
						{
							p.Controller.IsSuspect = true;
							Debug.Log($"[SPOILER]\n\n{p.GetStyledNickname} is suspect");
						}
					}

					PlayerRegistry.ForEach(pObj =>
					{
						pObj.Controller.EmergencyMeetingUses = GameManager.Instance.Settings.numEmergencyMeetings;
					});
				}
			}

			if (Runner.IsPlayer) // [PLAYER] * -> Play
			{
				PlayerMovement.Local.ClearInteractables();
				GameManager.im.gameUI.CloseOverlay(state == EGameState.VoteResults ? 0 : 3);

				if (state == EGameState.Pregame) // [PLAYER] Pregame -> Play
				{
					GameManager.Instance.mapData.hull.SetActive(false);
					GameManager.im.gameUI.InitGame();

					GameManager.Instance.localTasks = new List<TaskStation>(
						FindObjectsOfType<TaskStation>()
						.OrderBy(t => Random.value)
						.Take(GameManager.Instance.Settings.numTasks));

					foreach (TaskBase task in FindObjectsOfType<TaskBase>(true))
					{
						GameManager.Instance.taskAmounts.Add(task, (byte)GameManager.Instance.Tasks().Count(ts => ts.taskUI.task == task));
					}

					GameManager.im.gameUI.InitTaskUI();

					if (PlayerMovement.Local.IsSuspect)
					{
						GameManager.Instance.localTasks.Clear();
						GameManager.Instance.taskAmounts.Clear();
					}

					GameManager.vm.SetTalkChannel(VoiceManager.NONE);
				}
			}
		};

		StateMachine[EGameState.Meeting].onEnter = state =>
		{
			Debug.Log($"Entered {EGameState.Meeting} from {state}");

			if (Runner.IsServer) { }

			if (Runner.IsPlayer) // [PLAYER] * -> Meeting
			{
				if (TaskUI.ActiveUI)
					TaskUI.ActiveUI.CloseTask();

				GameManager.im.gameUI.votingScreen.SetActive(true);

				if (PlayerMovement.Local.IsDead == false)
					GameManager.vm.SetTalkChannel(VoiceManager.GLOBAL);
			}
		};

		StateMachine[EGameState.Meeting].onExit = state =>
		{
			if (Runner.IsServer) // [SERVER] Meeting -> *
			{
				GameManager.Instance.MeetingCaller = null;
			}

			if (Runner.IsPlayer) // [PLAYER] Meeting -> *
			{
				
				GameManager.im.gameUI.votingScreen.SetActive(false);
				if (PlayerMovement.Local.IsDead == false)
					GameManager.vm.SetTalkChannel(VoiceManager.NONE);
			}
		};

		StateMachine[EGameState.VoteResults].onEnter = state =>
		{
			if (Runner.IsServer) // [SERVER] * => VoteResults
			{
				if (GameManager.Instance.VoteResult is PlayerObject pObj)
				{
					pObj.Controller.IsDead = true;
					pObj.Controller.Server_UpdateDeadState();

					int numCrew = PlayerRegistry.CountWhere(p => !p.Controller.IsDead && p.Controller.IsSuspect == false);
					int numSus = PlayerRegistry.CountWhere(p => !p.Controller.IsDead && p.Controller.IsSuspect == true);

					if (numCrew <= numSus)
					{	// impostors win if they can't be outvoted in a meeting
						Server_DelaySetState(EGameState.ImpostorWin, 3);
					}
					else if (numSus == 0)
					{	// crew wins if all impostors have been ejected
						Server_DelaySetState(EGameState.CrewWin, 3);
					}
					else
					{	// return to play if the game isn't over
						Server_DelaySetState(EGameState.Play, 3);
					}
				}
				else
				{	// return to play if there was nobody ejected
					Server_DelaySetState(EGameState.Play, 2);
				}
			}

			if (Runner.IsPlayer) // [PLAYER] * => VoteResults
			{
				GameManager.im.gameUI.EjectOverlay(GameManager.Instance.VoteResult);
			}
		};

		StateMachine[EGameState.CrewWin].onEnter = state =>
		{
			Debug.Log($"Entered {EGameState.CrewWin} from {state}");
			
			if (Runner.IsServer) // [SERVER] * -> CrewWin
			{
				Server_DelaySetState(EGameState.Pregame, 3);
			}

			if (Runner.IsPlayer) // [PLAYER] * -> CrewWin
			{
				GameManager.im.gameUI.CrewmateWinOverlay();
				GameManager.vm.SetTalkChannel(VoiceManager.GLOBAL);
			}
		};

		StateMachine[EGameState.CrewWin].onExit = state => GameManager.im.gameUI.CloseOverlay();

		StateMachine[EGameState.ImpostorWin].onEnter = state =>
		{
			Debug.Log($"Entered {EGameState.ImpostorWin} from {state}");

			if (Runner.IsServer) // [SERVER] * -> ImpostorWin
			{
				Server_DelaySetState(EGameState.Pregame, 3);
			}

			if (Runner.IsPlayer) // [PLAYER] * -> ImpostorWin
			{
				GameManager.im.gameUI.ImpostorWinOverlay();
				GameManager.vm.SetTalkChannel(VoiceManager.GLOBAL);
			}
		};

		StateMachine[EGameState.ImpostorWin].onExit = state => GameManager.im.gameUI.CloseOverlay();
	}

	public override void FixedUpdateNetwork()
	{
		if (Runner.IsServer)
		{
			if (Delay.Expired(Runner))
			{
				Delay = TickTimer.None;
				Server_SetState(DelayedState);
			}
		}

		if (Runner.IsForward)
			StateMachine.Update(Current, Previous);
	}

	public void Server_SetState(EGameState st)
	{
		if (Current == st) return;
		Previous = Current;
		Current = st;
	}
	
	public void Server_DelaySetState(EGameState newState, float delay)
	{
		Debug.Log($"Delay state change to {newState} for {delay}s");
		Delay = TickTimer.CreateFromSeconds(Runner, delay);
		DelayedState = newState;
	}
}