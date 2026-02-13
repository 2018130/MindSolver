using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public float GetValue()
    {
        return slider.value;
    }

    public void SetValue(float value)
    {
        slider.value = value;
    }
}
