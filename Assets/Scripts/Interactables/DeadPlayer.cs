using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Helpers.Collections;

public class DeadPlayer : Interactable
{
    public Renderer[] modelMeshes; // For setting Colour

	[Networked(OnChanged = nameof(OnRefChanged))]
	public PlayerRef Ref { get; set; }

	public override void Spawned()
	{
		base.Spawned();
		GameManager.rm.Manage(Object);
	}

	public void SetColour(Color col)
    {
        Debug.Log($"{this} : set color {col}");

		Material bodyColorMat = Instantiate(GameManager.rm.playerBodyMaterial);
		bodyColorMat.color = col;
        modelMeshes.ForEach(ren =>
		{
			Material[] mats = ren.sharedMaterials;
			for (int i = 0; i < mats.Length; i++)
			{
				if (mats[i] == GameManager.rm.playerBodyMaterial)
				{
					mats[i] = bodyColorMat;
				}
			}
			ren.sharedMaterials = mats;
		});
	}

	static void OnRefChanged(Changed<DeadPlayer> changed)
	{
		PlayerObject pObj = PlayerRegistry.GetPlayer(changed.Behaviour.Ref);
		if (pObj != null)
			changed.Behaviour.SetColour(GameManager.rm.playerColours[pObj.ColorIndex]);
	}

	public override void Interact()
	{
		GameManager.Instance.Rpc_CallMeeting(Runner.LocalPlayer, Object);
	}

	public override bool CanInteract => PlayerMovement.Local.IsDead == false;
}