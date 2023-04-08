using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidersTask : TaskBase
{
	public override string Name => "Sliders";

	public Slider[] targetSliders;
	public Slider[] inputSliders;
	public float errorMargin = 0.02f;

	public override void ResetTask()
	{
		foreach(Slider slider in inputSliders)
		{
			slider.interactable = true;
			slider.value = Random.value;
		}

		foreach (Slider slider in targetSliders)
		{
			slider.value = Random.value;
		}
	}

	public void ReleaseSlider(int index)
	{
		if (Mathf.Abs(inputSliders[index].value - targetSliders[index].value) <= errorMargin)
		{
			inputSliders[index].value = targetSliders[index].value;
			inputSliders[index].interactable = false;
			
			CheckSliders();
		}
	}

	void CheckSliders()
	{
		foreach (Slider slider in inputSliders)
		{
			if (slider.interactable) return;
		}
		StartCoroutine(DelayCompleted());
	}

	IEnumerator DelayCompleted()
	{
		yield return new WaitForSeconds(0.5f);
		Completed();
	}
}
