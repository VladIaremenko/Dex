using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Door : NetworkBehaviour
{
	public BoxCollider activationArea;
	public Transform leftDoor;
	public Transform rightDoor;
	public float openDistance = 2f;
	public float openDuration = 1f;
	public float holdDuration = 1f;
	public AnimationCurve openCurve = AnimationCurve.Linear(0, 0, 1, 1);
	public AnimationCurve closeCurve = AnimationCurve.Linear(0, 0, 1, 1);

	[Networked]
	public bool IsOpen { get; set; }

	[Networked(OnChanged = nameof(TickActivatedChanged))]
	public int TickActivated { get; set; }

	bool isAnimating = false;

	public override void Render()
	{
		if (isAnimating)
		{
			float animProgress = (float)(Runner.SimulationRenderTime - TickActivated * Runner.Simulation.Config.DeltaTime) / openDuration;

			float sourcePos = IsOpen ? 0 : openDistance;
			float targetPos = IsOpen ? openDistance : 0;
			leftDoor.localPosition = new Vector3(
				Mathf.Lerp(sourcePos, targetPos, 
					(IsOpen ? openCurve : closeCurve).Evaluate(animProgress)),
				0, 0);
			rightDoor.localPosition = new Vector3(
				Mathf.Lerp(-sourcePos, -targetPos,
					(IsOpen ? openCurve : closeCurve).Evaluate(animProgress)),
				0, 0);

			if (animProgress >= 1)
			{
				leftDoor.localPosition = new Vector3(targetPos, 0, 0);
				rightDoor.localPosition = new Vector3(-targetPos, 0, 0);
				isAnimating = false;
			}
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (Runner.IsServer)
		{
			// reduce polling frequency for performance
			if (Runner.Simulation.Tick % 10 == 0)
			{
				// only allow a change in state if the door is not actively opening/closing
				if ((Runner.Simulation.Tick - TickActivated) * Runner.Simulation.Config.DeltaTime > openDuration + holdDuration)
				{
					bool playerNearby = PlayerRegistry.Any(p =>
						p.Controller.IsDead == false &&
						activationArea.ClosestPoint(p.transform.position) == p.transform.position);

					if (IsOpen != playerNearby)
					{
						IsOpen = playerNearby;
						TickActivated = Runner.Simulation.Tick;
					}
				}
			}
		}
	}

	static void TickActivatedChanged(Changed<Door> changed)
	{
		changed.Behaviour.isAnimating = true;

		AudioManager.Play($"SFX_Door{(changed.Behaviour.IsOpen ? "Open" : "Close")}", AudioManager.MixerTarget.SFX, changed.Behaviour.transform.position);
	}
}
