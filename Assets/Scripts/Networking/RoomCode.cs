using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomCode
{
	public static string Create(int length = 4)
	{
		char[] chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

		string str = "";
		for (int i = 0; i < length; i++)
		{
			str += chars[Random.Range(0, chars.Length)];
		}
		return str;
	}
}
