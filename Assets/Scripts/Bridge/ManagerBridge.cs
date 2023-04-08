using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerBridge : MonoBehaviour
{
	public void BackToMenu()
	{
		UIScreen.Focus(GameManager.im.mainMenuScreen);
	}

	public void QuitGame()
	{
		GameManager.QuitGame();
	}
}
