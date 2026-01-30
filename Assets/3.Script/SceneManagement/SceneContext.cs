using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneContext : MonoBehaviour
{
    [SerializeField]
    private float gameDeltaTime = 1f;
    public float GameDeltaTime => gameDeltaTime;

    public void Initialize()
    {

    }
}
