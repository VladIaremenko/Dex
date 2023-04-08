using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionText : MonoBehaviour
{
	public TMP_Text text;

	void Awake()
	{
		text.text = $"v{Application.version}";
	}
}
