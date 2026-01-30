using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod]
    static void GameInitializer()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene("BootStrapScene");
        SceneManager.LoadScene(currentScene.name);
    }
}
