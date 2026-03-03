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
            if(!NetworkPlayer.IsServerPlayer)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
        else
        {
            if(NetworkPlayer.IsServerPlayer)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    public void EndInteract()
    {
        if (StageManager.SingletonManager.RemainRemoveObstacleCount <= 0)
            return;

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
            Destroy_ServerRpc();
        }
        else
        {
            isInteracted = false;
            StageManager.SingletonManager.InsertPuzzle(readyPuzzle);
        }

        GameManager.Singleton.ChangeState(GameState.Playing);
        StageManager.SingletonManager.ClosePuzzleQueue();
    }

    [ServerRpc(RequireOwnership = false)]
    private void Destroy_ServerRpc()
    {
        Destroy_ClientRpc();
    }

    [ClientRpc]
    private void Destroy_ClientRpc()
    {
        Destroy(gameObject);
    }
}
