using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomIconSelector : MonoBehaviour
{
    public Sprite[] icons;
    public Image image;

    private void OnEnable()
    {
        SetIcon();
    }

    public void SetIcon()
    {
        image.sprite = icons[Random.Range(0, icons.Length)];
    }
}
