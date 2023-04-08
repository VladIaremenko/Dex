using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NicknameSetter : MonoBehaviour
{
	public TMP_InputField nicknameField;
	//public TMP_Text inputFieldText;

    private void OnEnable()
    {
		GetNickname();
	}

    public void GetNickname()
    {
		if (PlayerPrefs.HasKey("nickname"))
		{
			string nickname = PlayerPrefs.GetString("nickname");
			nicknameField.SetTextWithoutNotify(nickname);
		}
		else
		{
			SetNickname("Player");
		}
	}

	public void SetNickname(string nickname)
	{
		if (string.IsNullOrEmpty(nickname))
		{
			PlayerPrefs.DeleteKey("nickname");
		}
		else
		{
			PlayerPrefs.SetString("nickname", nickname);
		}
	}
}
