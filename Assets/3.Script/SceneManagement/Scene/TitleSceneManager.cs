using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneManager : MonoBehaviour
{
    public static TitleSceneManager Singleton;

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        SceneChangeManager.Singleton.ChangeScene(SceneType.GameScene);
    }
}
