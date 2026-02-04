using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    TitleScene,
    GameScene,
    LoadingScene,
    NetworkRelayScene,
    LobbyScene,
    TutorialScene
}

public class SceneChangeManager : SingletonBehaviour<SceneChangeManager>
{
    public void ChangeScene(SceneType sceneType, float minLoadingTime = 3f)
    {
        StartCoroutine(ChangeScene_co(sceneType, minLoadingTime));
    }

    private IEnumerator ChangeScene_co(SceneType sceneType, float minLoadingTime)
    {
        float timer = 0f;
        float progressValue = 0f;
        SceneManager.LoadScene((int)SceneType.LoadingScene);

        yield return null;

        LoadingSceneUIManager loadingSceneUIManager = FindAnyObjectByType<LoadingSceneUIManager>();

        AsyncOperation ao = SceneManager.LoadSceneAsync((int)sceneType);

        ao.allowSceneActivation = false;
        GameManager.Singleton.IsInitialized = false;

        while (!ao.isDone)
        {
            yield return null;

            progressValue = ao.progress > timer / minLoadingTime ?
                timer / minLoadingTime : ao.progress;
            loadingSceneUIManager.SetLoadingProgress(progressValue);

            if (timer >= minLoadingTime && ao.progress >= 0.9f)
            {
                ao.allowSceneActivation = true;
            }

            timer += Time.deltaTime;
        }

        GameManager.Singleton.Initialize();
    }
    public void ChangeSceneByNetwork(string sceneName, float minLoadingTime = 3f)
    {
        StartCoroutine(ChangeSceneByNetwork_co(sceneName, minLoadingTime));
    }

    private IEnumerator ChangeSceneByNetwork_co(string sceneName, float minLoadingTime)
    {
        float timer = 0f;
        float progressValue = 0f;
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        yield return null;

        LoadingSceneUIManager loadingSceneUIManager = FindAnyObjectByType<LoadingSceneUIManager>();

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        /*
        GameManager.Singleton.IsInitialized = false;

        while (!ao.isDone)
        {
            yield return null;

            progressValue = ao.progress > timer / minLoadingTime ?
                timer / minLoadingTime : ao.progress;
            loadingSceneUIManager.SetLoadingProgress(progressValue);

            if (timer >= minLoadingTime && ao.progress >= 0.9f)
            {
                ao.allowSceneActivation = true;
            }

            timer += Time.deltaTime;
        }
        */
        GameManager.Singleton.Initialize();
    }
}
