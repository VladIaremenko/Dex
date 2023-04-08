using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LocationZone : MonoBehaviour
{
	public string displayName;

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.parent && other.transform.parent.TryGetComponent(out PlayerObject pObj) && pObj == PlayerObject.Local)
		{
			GameManager.im.gameUI.SetRoomText(displayName);
		}
	}
}
