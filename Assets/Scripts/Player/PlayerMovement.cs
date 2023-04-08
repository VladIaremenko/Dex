using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.KCC;
using Helpers.Physics;
using Helpers.Bits;
using System.Linq;

[OrderBefore(typeof(NetworkTransformAnchor))]
public class PlayerMovement : NetworkBehaviour
{
	public static PlayerMovement Local { get; protected set; }

	readonly FixedInput LocalInput = new FixedInput();

	public float lookTurnRate = 1.5f;
	
	[field: SerializeField] List<Interactable> nearbyInteractables { get; } = new List<Interactable>();

	// the interactable you are actively interacting with, if any
	[field: SerializeField] public Interactable activeInteractable { get; private set; } = null;

	public void EndInteraction()
	{
		if (activeInteractable && activeInteractable.CanInteract == false)
			GameManager.im.gameUI.useButton.interactable = false;
		activeInteractable = null;
	}

	[Networked(OnChanged = nameof(OnDeadChanged))]
	public bool IsDead { get; set; }
	
	[Networked]
	public bool IsSuspect { get; set; }

	[Networked]
	public byte EmergencyMeetingUses { get; set; }

	[Networked(OnChanged = nameof(OnKillTimerChanged))]
	public TickTimer KillTimer { get; set; }

	static void OnDeadChanged(Changed<PlayerMovement> changed)
	{
		PlayerMovement self = changed.Behaviour;

		if (self.Object.HasInputAuthority)
		{
			if (self.IsDead && TaskUI.ActiveUI) TaskUI.ActiveUI.CloseTask();

			// show nicknames to ghosts
			if (self.IsDead) GameManager.im.nicknameHolder.gameObject.SetActive(true);

			// update ghost visibility
			PlayerRegistry.ForEachWhere(p => p.Controller.IsDead, p => p.GetComponent<PlayerData>().SetGhost(true));

			// set voice channels
			if (self.IsDead)
			{
				GameManager.vm.SetTalkChannel(VoiceManager.GHOST);
				GameManager.vm.JoinListenChannel(VoiceManager.GHOST);
			}
			else
			{
				GameManager.vm.SetTalkChannel(VoiceManager.GLOBAL);
				GameManager.vm.ClearListenChannel(VoiceManager.GHOST);
			}
		}

		self.GetComponent<PlayerData>().SetGhost(self.IsDead);

		if (GameManager.State.Current == GameState.EGameState.Play && (Local.IsDead || Local.IsSuspect))
		{
			AudioManager.Play("SFX_Kill", AudioManager.MixerTarget.SFX, self.transform.position);
		}
	}

	static void OnKillTimerChanged(Changed<PlayerMovement> changed)
	{
		if (changed.Behaviour == Local && !changed.Behaviour.KillTimer.ExpiredOrNotRunning(GameManager.Instance.Runner))
		{
			GameManager.im.gameUI.StartKillTimer();
		}
	}

	public void Server_UpdateDeadState()
	{
		//if (IsDead) GameManager.Instance.OnPlayerKilled();

		cc.SetLayer(LayerMask.NameToLayer(IsDead ? "Ghost" : "Player"));
		cc.SetLayerMask(cc.Settings.CollisionLayerMask.Flag("GhostPassable", !IsDead));
		cc.SetLayerMask(cc.Settings.CollisionLayerMask.Flag("Player", GameManager.Instance.Settings.playerCollision & !IsDead));
	}

	public KCC cc { get; protected set; }

	public bool TransformLocal = false;

	/// <summary>
	/// If object is not using <see cref="NetworkCharacterController"/>, this controls how much change is applied to the transform/rigidbody.
	/// </summary>
	public float Speed = 6f;

	bool ShowSpeed => this;

	Collider[] physicsResults = new Collider[8];

	public override void Spawned()
	{
		if (Object.HasInputAuthority)
		{
			Local = this;
		}
		cc = GetComponent<KCC>();

		if (cc)
		{
			cc.OnCollisionEnter += (kcc, coll) =>
			{
				//if (kcc.IsInFixedUpdate)
				{
					if (coll.NetworkObject.TryGetComponent(out Interactable hitInteractable) && hitInteractable.CanInteract)
					{
						nearbyInteractables.AddUnique(hitInteractable);

						if (Object.HasInputAuthority)
						{
							if (hitInteractable is DeadPlayer _)
								GameManager.im.gameUI.reportButton.interactable = true;
							else
								GameManager.im.gameUI.useButton.interactable = true;
						}
					}
					else if (Object.HasInputAuthority
						&& !coll.NetworkObject.transform.IsChildOf(transform)
						&& coll.NetworkObject.gameObject.layer == LayerMask.NameToLayer("PlayerRadius"))
					{
						GameManager.im.gameUI.killButton.interactable = true;
					}
				}
			};

			cc.OnCollisionExit += (kcc, coll) =>
			{
				//if (kcc.IsInFixedUpdate)
				if (coll.NetworkObject)
				{
					if (coll.NetworkObject.TryGetComponent(out Interactable hitInteractable))
					{
						nearbyInteractables.Remove(hitInteractable);
						if (Object.HasInputAuthority)
						{
							bool canReport = false;
							bool canUse = false;

							if (nearbyInteractables.Count > 0)
							{
								foreach (Interactable interactable in nearbyInteractables)
								{
									if (interactable is DeadPlayer _)
									{
										canReport = true;
										break;
									}
								}

								if (!canReport) canUse = true;
							}

							if (!canReport)
							{
								GameManager.im.gameUI.reportButton.interactable = false;
							}

							if (!canUse)
							{
								GameManager.im.gameUI.useButton.interactable = false;
							}
						}
					}
					else if (Object.HasInputAuthority
						&& coll.NetworkObject.gameObject.layer == LayerMask.NameToLayer("PlayerRadius"))
					{
						GameManager.im.gameUI.killButton.interactable = false;
					}
				}
			};
		}
	}

	void Update()
	{
		LocalInput.PollDown(KeyCode.E);
		if (Runner.IsServer) LocalInput.PollDown(KeyCode.KeypadEnter);
	}

	public override void FixedUpdateNetwork()
	{
		if (Runner.IsServer)
		{
			if (LocalInput.GetDown(KeyCode.KeypadEnter))
			{
				GameManager.Instance.Server_StartGame();
				Debug.Log("Starting Game...");
			}
		}

		// ---

		if (Runner.Config.PhysicsEngine == NetworkProjectConfig.PhysicsEngines.None)
		{
			return;
		}

		Vector3 direction = default;
		if (activeInteractable == null
			&& GameManager.im.gameUI.messageScreen.activeInHierarchy == false
			&& GameManager.im.gameUI.votingScreen.activeInHierarchy == false
			&& GetInput(out NetworkInputPrototype input))
		{
			// BUTTON_WALK is representing left mouse button
			if (input.IsDown(NetworkInputPrototype.BUTTON_WALK))
			{
				direction = new Vector3(
					Mathf.Cos((float)input.Yaw * Mathf.Deg2Rad),
					0,
					Mathf.Sin((float)input.Yaw * Mathf.Deg2Rad)
				);
			}
			else
			{
				if (input.IsDown(NetworkInputPrototype.BUTTON_FORWARD))
				{
					direction += TransformLocal ? transform.forward : Vector3.forward;
				}

				if (input.IsDown(NetworkInputPrototype.BUTTON_BACKWARD))
				{
					direction -= TransformLocal ? transform.forward : Vector3.forward;
				}

				if (input.IsDown(NetworkInputPrototype.BUTTON_LEFT))
				{
					direction -= TransformLocal ? transform.right : Vector3.right;
				}

				if (input.IsDown(NetworkInputPrototype.BUTTON_RIGHT))
				{
					direction += TransformLocal ? transform.right : Vector3.right;
				}

				direction = direction.normalized;
			}

			if (Object.HasInputAuthority && Runner.IsResimulation == false)
			{
				if (LocalInput.GetDown(KeyCode.E))
				{
					TryKill();
					TryUse(true);
					TryUse(false);
				}
			}
		}

		cc.SetInputDirection(direction);
		cc.SetKinematicVelocity(direction * Speed);
		
		if (direction != Vector3.zero)
		{
			Quaternion targetQ = Quaternion.AngleAxis(Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg - 90, Vector3.down);
			cc.SetLookRotation(Quaternion.RotateTowards(transform.rotation, targetQ, lookTurnRate * 360 * Runner.DeltaTime));
		}

		if (Runner.IsResimulation == false)
			LocalInput.Clear();
	}

	public void TryUse(bool task)
	{
		if (nearbyInteractables.Count > 0)
		{
			if (task)
			{
				Interactable interactable = nearbyInteractables
					.Where(inter => inter is DeadPlayer _ == false)
					.OrderBy(inter => Vector3.Distance(inter.transform.position, transform.position))
					.Take(1)
					.SingleOrDefault();

				if (interactable)
				{
					if (interactable is TaskStation station)
					//if (nearbyInteractables is TaskStation station)
					{
						if (GameManager.Instance.HasTask(station))
						{
							if (!IsDead || station.isGhostAccessible)
							{
								activeInteractable = interactable;
								activeInteractable.Interact();
								if (activeInteractable.isInteractionInstant)
									activeInteractable = null;
							}
						}
					}
					else if (interactable is SettingsKiosk settings)
					//else if (nearbyInteractables is SettingsKiosk settings)
					{
						if (Runner.IsServer)
						{
							activeInteractable = interactable;
							activeInteractable.Interact();
							if (activeInteractable.isInteractionInstant)
								activeInteractable = null;
						}
					}
					else
					{
						if (!IsDead || interactable.isGhostAccessible)
						{
							activeInteractable = interactable;
							activeInteractable.Interact();
							if (activeInteractable.isInteractionInstant)
								activeInteractable = null;
						}
					}
				}
			}
			else
			{
				Interactable interactable = nearbyInteractables
					.Where(inter => inter is DeadPlayer _)
					.OrderBy(inter => Vector3.Distance(inter.transform.position, transform.position))
					.Take(1)
					.SingleOrDefault();

				if (interactable)
				//if (!(nearbyInteractables is TaskStation _) && !(nearbyInteractables is SettingsKiosk _))
				{
					if (!IsDead || interactable.isGhostAccessible)
					{
						activeInteractable = interactable;
						activeInteractable.Interact();
						if (activeInteractable.isInteractionInstant)
							activeInteractable = null;
					}
				}
			}


			// if not task station, or is task station and has task
			//if (!(nearbyInteractable is TaskStation station && GameManager.instance.HasTask(station.taskUI.task) == false))
			//{
			//	if (!IsDead || nearbyInteractable.isGhostAccessible)
			//	{
			//		activeInteractable = nearbyInteractable;
			//		activeInteractable.Interact();
			//		if (activeInteractable.isInteractionInstant)
			//			activeInteractable = null;
			//	}
			//	else
			//	{
			//		Debug.Log("Ghosts can't use that");
			//	}
			//}
			//else
			//{
			//	Debug.Log("I don't have this task");
			//}
		}
	}

	public void TryKill()
	{
		if (IsSuspect && KillTimer.ExpiredOrNotRunning(Runner))
		{
			var points = cc.Collider.GetPoints();
			int overlaps = Physics.OverlapCapsuleNonAlloc(
				points.point0, points.point1,
				cc.Collider.radius, physicsResults,
				LayerMask.GetMask("PlayerRadius"));

			if (overlaps > 0)
			{
				for (int i = 0; i < overlaps; i++)
				{
					var el = physicsResults[i];

					if (el != null
						&& el.GetComponentInParent<PlayerObject>() is PlayerObject pl
						&& pl != PlayerObject.Local
						&& pl.Controller.IsSuspect == false
						&& pl.Controller.IsDead == false)
					{
						Debug.Log("Call RPC Kill");
						pl.Rpc_Kill(Runner.LocalPlayer);
						break;
					}
				}
			}
		}
	}

	public void ClearInteractables()
	{
		nearbyInteractables.Clear();
	}
}