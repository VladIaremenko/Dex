using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Photon.Voice.Fusion;

public class PlayerObject : NetworkBehaviour
{
	public static PlayerObject Local { get; private set; }

	[Networked]
	public PlayerRef Ref { get; set; }
	[Networked]
	public byte Index { get; set; }
	[Networked(OnChanged = nameof(NicknameChanged))]
	public NetworkString<_16> Nickname { get; set; }
	[Networked(OnChanged = nameof(ColorChanged))]
	public byte ColorIndex { get; set; }

	public Color GetColor => GameManager.rm.playerColours[ColorIndex];
	public string GetStyledNickname => $"<color=#{ColorUtility.ToHtmlStringRGB(GetColor)}>{Nickname}</color>";

	[field: Header("References"), SerializeField] public PlayerMovement Controller { get; private set; }
	[field: SerializeField] public VoiceNetworkObject VoiceObject { get; private set; }
	[field: SerializeField] public SphereCollider KillRadiusTrigger { get; private set; }


	public void Server_Init(PlayerRef pRef, byte index, byte color)
	{
		Debug.Assert(Runner.IsServer);

		Ref = pRef;
		Index = index;
		ColorIndex = color;
	}

	public override void Spawned()
	{
		base.Spawned();

		if (Object.HasStateAuthority)
		{
			PlayerRegistry.Server_Add(Runner, Object.InputAuthority, this);
		}

		if (Object.HasInputAuthority)
		{
			Local = this;
			Rpc_SetNickname(PlayerPrefs.GetString("nickname"));
		}
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	void Rpc_SetNickname(string nick)
	{
		Nickname = nick;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_SetColor(byte c)
	{
		if (PlayerRegistry.IsColorAvailable(c))
			ColorIndex = c;
	}


	[Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
	public void Rpc_Kill(PlayerRef killer)
	{
		if (Controller.IsSuspect) return;
		
		PlayerObject src = PlayerRegistry.GetPlayer(killer);

		if (Vector3.Distance(transform.position, Controller.cc.Collider.ClosestPoint(src.transform.position)) <= KillRadiusTrigger.radius)
		{
			Controller.IsDead = true;
			Runner.Spawn(GameManager.rm.deadPlayer.GetComponent<NetworkObject>(), transform.position, transform.rotation)
					.GetComponent<DeadPlayer>().Ref = Ref;
			src.Controller.KillTimer = TickTimer.CreateFromSeconds(Runner, 30);
			Debug.Log($"[SPOILER]\n\n{src.GetStyledNickname} killed {GetStyledNickname}");

			Controller.Server_UpdateDeadState();
			GameManager.OnPlayerKilled();
		}
		else
		{
			Debug.LogWarning($"[SPOILER]\n\n{src.GetStyledNickname} is too far away to kill {GetStyledNickname}");
		}
	}




	static void NicknameChanged(Changed<PlayerObject> changed)
	{
		changed.Behaviour.GetComponent<PlayerData>().SetNickname(changed.Behaviour.Nickname.Value);
	}

	static void ColorChanged(Changed<PlayerObject> changed)
	{
		changed.Behaviour.GetComponent<PlayerData>().SetColour(changed.Behaviour.GetColor);
		if (Local!=null)
		{
			if (Local.Controller.activeInteractable is ColorKiosk ck)
			{
				ck.colorUI.Refresh();
			}
		}
	}
}
