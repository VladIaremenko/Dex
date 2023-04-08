using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollingRawImage : MonoBehaviour
{
    RawImage rawImage;
    public float xSpeed, ySpeed;
    private float xVal, yVal;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        xVal += Time.deltaTime * xSpeed;
        yVal += Time.deltaTime * ySpeed;
        rawImage.uvRect = new Rect(xVal, yVal, rawImage.uvRect.width, rawImage.uvRect.height);
    }
}
