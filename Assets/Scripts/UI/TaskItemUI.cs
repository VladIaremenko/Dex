using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TaskItemUI : MonoBehaviour
{
    public TMP_Text taskText;
	public TaskBase Task { get; private set; }

    public void SetTask(TaskBase task)
    {
		Task = task;
    }

	public void SetCount(int numFinished, int numTotal)
	{
		string count = (numTotal > 1) ? $" ({numFinished}/{numTotal})" : "";
		taskText.text = Task.Name + count;
		if (PlayerMovement.Local.IsSuspect)
			taskText.color = new Color32(230, 140, 150, 255);
		else if (numFinished == numTotal)
			taskText.color = Color.green;
	}

	//public void Complete()
	//{
	//	taskText.color = Color.green;
	//}
}
