using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TemperatureTask : TaskBase
{
	public override string Name => "Thermostat";
	public TMP_Text logText;
	public TMP_Text readingText;

	int currentTemperature;
	int targetTemperature;
	int delta = 0;

	public override void ResetTask()
	{
		delta = 0;
		targetTemperature = Random.Range(-99, 99);
		readingText.text = $"{targetTemperature}";
		currentTemperature = Random.Range(-99, 99);
		logText.text = $"{currentTemperature}";
	}

	private IEnumerator UpdateTemperature()
	{
		float t = Time.time;
		while (delta != 0)
		{
			float heldFor = (Time.time - t);

			currentTemperature += heldFor < 3 ? delta : delta * 10;
			logText.text = $"{currentTemperature}";

			yield return new WaitForSeconds(heldFor < 1 ? 0.2f : heldFor < 3 ? 0.08f : 0.5f);
		}

		if (currentTemperature == targetTemperature)
		{
			Completed();
		}
	}

	public void SetDelta(int delta)
	{
		this.delta = delta;
		if (delta != 0)
			StartCoroutine(UpdateTemperature());
	}
}
