using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState
{
    Playing,
    Puzzle,
    UI,
    Dialogue
}

public class GameManager : SingletonBehaviour<GameManager>
{
    public bool IsInitialized { get; set; }
    [SerializeField]
    // 게임 씬 호출 이후 무조건 초기화 되어 있어야 함
    private SceneContext currentSceneContext;
    public SceneContext CurrentSceneContext => currentSceneContext;

    [SerializeField]
    private GameState gameState = GameState.Playing;
    public GameState GameState => gameState;

    public event Action<GameState> OnChangedGameState;

    public void Initialize()
    {
        currentSceneContext = GameObject.FindAnyObjectByType<SceneContext>();
        if (currentSceneContext != null)
        {
            currentSceneContext.Initialize();
            Debug.Log("GameManager Initialized");
        }
        CallOnSceneContextBuilt();
    }

    private void CallOnSceneContextBuilt()
    {
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        ISceneContextBuilt[] sceneContextBuilts = allMonoBehaviours
            .OfType<ISceneContextBuilt>()
            .ToArray();
        sceneContextBuilts = sceneContextBuilts.OrderBy(x => x.Priority).ToArray();

        foreach (var sceneContextBuilt in sceneContextBuilts)
        {
            sceneContextBuilt.OnSceneContextBuilt();
        }
        IsInitialized = true;
    }

    public void ChangeState(GameState newState)
    {
        if (gameState == newState)
            return;

        gameState = newState;
        OnChangedGameState?.Invoke(gameState);
        switch (gameState)
        {
            case GameState.Playing:
                Time.timeScale = 1;
                GameUIManager.Singleton.SetActiveMainUI(true);
                break;
            case GameState.UI:
                Time.timeScale = 0;
                break;
            case GameState.Puzzle:
                GameUIManager.Singleton.SetActiveMainUI(false);
                break;
        }
        Debug.Log($"{gameState}");
    }
}
