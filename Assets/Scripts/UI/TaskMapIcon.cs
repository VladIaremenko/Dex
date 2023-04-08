using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMapIcon : MonoBehaviour
{
	[HideInInspector] public Transform target;
	public TaskStation task;
    public Vector3 offset;

	public void Init(TaskStation taskStation)
	{
		task = taskStation;
		target = taskStation.transform;
		transform.position = target.position + offset;
		transform.rotation = Quaternion.LookRotation(-Vector3.up, Vector3.forward);
	}
}
