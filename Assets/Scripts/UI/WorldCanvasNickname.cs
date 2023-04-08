using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldCanvasNickname : MonoBehaviour
{
    /// <summary>
    /// Make this always point towards the main camera and follow the target if there is one.
    /// If there is no target to follow, we destroy this object to clean up.
    /// </summary>
    /// 
    public TMP_Text worldNicknameText;
    [HideInInspector] public Transform target;
    public Vector3 offset;

    private void LateUpdate()
    {
        if (target)
        {
            transform.position = target.position + offset;
            transform.rotation = Camera.main.transform.rotation;
        }
        else
        {
            StartCoroutine(WaitAndDestroy());
        }
    }

    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(3);
        if (target != null && !target.Equals(null))
        {
            //continue following the target
            yield return null;
        }
        else //there has been no target to follow for 3 seconds so Destroy this:
        {
            Destroy(gameObject);
        }
    }

}
