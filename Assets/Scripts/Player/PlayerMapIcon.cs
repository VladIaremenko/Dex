using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMapIcon : MonoBehaviour
{
	[HideInInspector] public Transform target;
	public PlayerObject owner;
	public Vector3 offset;

	public void Init(PlayerObject owner)
	{
		target = owner.transform;
		this.owner = owner;
		transform.position = target.position + offset;
		transform.rotation = Quaternion.LookRotation(-Vector3.up, Vector3.forward);
	}

	private void Update()
	{
		if (owner) transform.position = target.position + offset;
	}

	private void OnDisable()
	{
		if (GameManager.Instance.Object.IsValid == false ||
			(GameManager.State.Current != GameState.EGameState.Play
			&& GameManager.State.Current != GameState.EGameState.Meeting))
		{
			Destroy(gameObject);
		}
	}
}
