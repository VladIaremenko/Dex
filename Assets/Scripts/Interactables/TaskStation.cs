using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TaskStation : Interactable
{
	public TaskUI taskUI;

	public override bool CanInteract => GameManager.Instance.HasTask(this);

	public override void Interact()
	{
		taskUI.Begin();
		Debug.Log("Began task");
	}
}
