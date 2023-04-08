using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskBase : MonoBehaviour
{
	public TaskUI taskUI;
	public abstract string Name { get; }

	public abstract void ResetTask();
	public virtual void Completed()
	{
		AudioManager.Play("taskCompleteUI", AudioManager.MixerTarget.UI);
		GameManager.Instance.CompleteTask();
		taskUI.Complete();
	}
}
