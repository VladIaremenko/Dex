using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NumberSequenceTask : TaskBase
{
	public override string Name => "Number Sequence";
	public Button[] buttons;
	int lastPushed = 0;

	public void PushButton(int number)
	{
		buttons[number - 1].interactable = false;

		if (number == lastPushed + 1)
		{
			lastPushed = number;
			if (number == buttons.Length)
			{
				Completed();
			}
		}
		else
		{
			ResetTask();
		}
	}

	public override void ResetTask()
	{
		lastPushed = 0;
		foreach(Button button in buttons.ToList().OrderBy(b => Random.value))
		{
			button.interactable = true;
			button.transform.SetAsFirstSibling();
		}
	}
}
