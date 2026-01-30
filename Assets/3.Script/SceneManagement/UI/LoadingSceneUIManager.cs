using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneUIManager : MonoBehaviour
{
    [SerializeField]
    private float minLoadingTime;
    [SerializeField]
    private Slider loadingProgress_Slider;

    private void Awake()
    {
        Debug.Log($"Called loading scene ui manager awake");
    }
    public void SetLoadingProgress(float value)
    {
        if(loadingProgress_Slider != null)
        {
            loadingProgress_Slider.value = value;
        }
    }
}
