using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers.Collections;

public class PlayerData : MonoBehaviour
{
	PlayerObject pObj;

	public Animator anim;
	public Renderer[] modelParts;
    public Transform uiPoint;

	public float animationBlending = 5f;

	WorldCanvasNickname nicknameUI;
	public Material BodyMaterial { get; private set; }
	public Material TrimMaterial { get; private set; }
	public Material VisorMaterial { get; private set; }

	float moveSpeed = 0;

	private void Awake()
	{
		pObj = GetComponent<PlayerObject>();

		BodyMaterial = Instantiate(GameManager.rm.playerBodyMaterial);
		TrimMaterial = Instantiate(GameManager.rm.playerTrimMaterial);
		VisorMaterial = Instantiate(GameManager.rm.playerVisorMaterial);

		modelParts.ForEach(ren =>
		{
			Material[] mats = ren.sharedMaterials;
			for (int i = 0; i < mats.Length; i++)
			{
				if (mats[i] == GameManager.rm.playerBodyMaterial)
					mats[i] = BodyMaterial;
				else if (mats[i] == GameManager.rm.playerTrimMaterial)
					mats[i] = TrimMaterial;
				else if (mats[i] == GameManager.rm.playerVisorMaterial)
					mats[i] = VisorMaterial;
				else
					Debug.LogWarning($"{ren} is using an unmanaged material {mats[i]}");
			}
			ren.sharedMaterials = mats;
		});
		Debug.Log($"Finished initializing materials", gameObject);
	}

	private void Update()
	{
		if (TryGetComponent(out PlayerMovement p))
		{
			moveSpeed = Mathf.Min(1f, Mathf.MoveTowards(moveSpeed, p.cc.RenderData.RealSpeed, Time.deltaTime * animationBlending));
			anim.SetFloat("MoveSpeed", moveSpeed);
		}
	}

	public void SetNickname(string nickname)
    {
		if (nicknameUI == null)
		{
			nicknameUI = Instantiate(
				GameManager.rm.worldCanvasNicknamePrefab,
				uiPoint.transform.position,
				Quaternion.identity,
				GameManager.im.nicknameHolder);
			nicknameUI.target = uiPoint;
		}
		nicknameUI.worldNicknameText.text = nickname;
    }
	
    public void SetColour(Color col)
    {
		Debug.Log($"{pObj.Nickname} changed color <color=#{ColorUtility.ToHtmlStringRGB(col)}>\u2588</color>");
		BodyMaterial.color = col;
    }

	public void SetGhost(bool on)
	{
		if (on)
		{
			if (PlayerMovement.Local.IsDead)
			{
				// semi-visible
				modelParts.ForEach(m => m.enabled = true);
				BodyMaterial.shader = GameManager.rm.ghostShader;
				TrimMaterial.shader = GameManager.rm.ghostShader;
				VisorMaterial.shader = GameManager.rm.ghostShader;
			}
			else
			{
				// full invisible
				modelParts.ForEach(m => m.enabled = false);
			}
		}
		else
		{
			modelParts.ForEach(m => m.enabled = true);
			BodyMaterial.shader = GameManager.rm.playerBodyMaterial.shader;
			TrimMaterial.shader = GameManager.rm.playerTrimMaterial.shader;
			VisorMaterial.shader = GameManager.rm.playerVisorMaterial.shader;
		}
	}
}
