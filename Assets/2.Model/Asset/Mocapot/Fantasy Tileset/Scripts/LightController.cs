using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    private Light2D _light2D;
    [SerializeField] private float _minimumIntensity;
    [SerializeField] private float _maximumIntensity;

    // Start is called before the first frame update
    void Start()
    {
        _light2D = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _light2D.intensity = Mathf.PingPong(Time.time * 0.5f, _maximumIntensity - _minimumIntensity) + _minimumIntensity;
    }
}
