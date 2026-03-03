using System;
using Unity.Netcode;
using UnityEngine;

public class PuzzleMissionTrigger : NetworkBehaviour, IInteractable
{
    enum NetworkType
    {
        Host,
        Client
    }

    [SerializeField]
    private bool isInteracted = false;

    [SerializeField]
    private NetworkType networkOwner;

    private PuzzleMissonListener readyPuzzle;

    private void Start()
    {
        if(networkOwner == NetworkType.Host)
        {
            if(!IsServer)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
        else
        {
            if(IsServer)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    public void EndInteract()
    {
        if (!isInteracted && GameManager.Singleton.GameState != GameState.Puzzle)
        {
            isInteracted = true;

            readyPuzzle = StageManager.SingletonManager.GetNextPuzzle();
            readyPuzzle?.StartPuzzle(this);
        }
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
    }


    public void EndPuzzle(bool isClear)
    {
        if(isClear)
        {
            Debug.Log($"Clear puzzle listened {readyPuzzle.gameObject}");
            StageManager.SingletonManager.ReduceRemainRemoveObstacleCount();
            Destroy(gameObject);
        }
        else
        {
            isInteracted = false;
            StageManager.SingletonManager.InsertPuzzle(readyPuzzle);
        }

        GameManager.Singleton.ChangeState(GameState.Playing);
        StageManager.SingletonManager.ClosePuzzleQueue();
    }
}
