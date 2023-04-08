using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FixRotation : EditorWindow
{
	[MenuItem("CONTEXT/Transform/Fix Reversed Rotation", false, 0)]
	public static void DoFix(MenuCommand command)
	{
		Transform ctx = (Transform)command.context;
		Undo.RecordObject(ctx, $"Fix Reversed Rotation ({ctx.gameObject.name})");
		ctx.localEulerAngles = new Vector3(ctx.localEulerAngles.x, ctx.localEulerAngles.y - 180f, ctx.localEulerAngles.z);
		ctx.localPosition = new Vector3(-ctx.localPosition.x, ctx.localPosition.y, -ctx.localPosition.z);
	}
}
