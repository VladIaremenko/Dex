using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettingsUI : MonoBehaviour
{
    public Dropdown graphicsDropdown;

    private void Awake()
    {
        InitGraphicsDropdown();
    }

    public void InitGraphicsDropdown()
    {
        string[] names = QualitySettings.names;
        List<string> options = new List<string>();

        for (int i = 0; i < names.Length; i++)
        {
            options.Add(names[i]);
        }
        graphicsDropdown.AddOptions(options);
		graphicsDropdown.onValueChanged.AddListener(index => SetGraphicsQuality(index));

        graphicsDropdown.value = graphicsDropdown.options.Count - 1;
        //QualitySettings.SetQualityLevel(graphicsDropdown.options.Count - 1);
    }

    public void SetGraphicsQuality(int value)
    {
        QualitySettings.SetQualityLevel(value);
		Debug.Log($"Set graphics quality to {QualitySettings.names[value]}");
    }
}
