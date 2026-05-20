using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PuzzleMissionTrigger : NetworkBehaviour, IInteractable
{
    enum NetworkType
    {
        Host,
        Client
    }
    private Collider2D col;

    [SerializeField]
    private bool isInteracted = false;

    [SerializeField]
    private NetworkType networkOwner;

    [SerializeField]
    private PuzzleMissonListener readyPuzzle;

    private void Start()
    {
        col = GetComponent<Collider2D>();

        if (IsSpawned)
        {
            if (networkOwner == NetworkType.Host)
            {
                if (!NetworkPlayer.IsServerPlayer)
                {
                    SetTouchable(false);
                }
            }
            else
            {
                if (NetworkPlayer.IsServerPlayer)
                {
                    SetTouchable(false);
                }
            }
        }
        else
        {
            SetTouchable(false);
        }
    }

    public void EndInteract()
    {
        if ((StageManager.SingletonManager != null && 
            StageManager.SingletonManager.RemainRemoveObstacleCount <= 0) ||
            (TutorialSceneManager.singleton != null && 
            !TutorialSceneManager.singleton.isPlayMission))
        {
            return;
        }

        if (!isInteracted && GameManager.Singleton.GameState == GameState.Playing)
        {
            isInteracted = true;
            StartCoroutine(CallListener());
        }
    }

    private IEnumerator CallListener()
    {
        PaperManager.singleton.StartPaperUnfoldAnimation();

        WaitWhile waitWhile = new WaitWhile(() => PaperManager.singleton.IsPaperOpening);

        yield return waitWhile;

        if (IsSpawned)
        {
            readyPuzzle = StageManager.SingletonManager.GetNextPuzzle();
            readyPuzzle?.StartPuzzle(this);
        }
        else
        {
            readyPuzzle?.StartPuzzle(this);
        }
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
    }

    public void SetTouchable(bool active)
    {
        if (col == null)
        {
            col = GetComponent<Collider2D>();
        }
        col.enabled = active;
    }

    public void EndPuzzle(bool isClear)
    {
        PaperManager.singleton.StartDissolveEffect();
        if (IsSpawned)
        {
            if (isClear)
            {
                Debug.Log($"Clear puzzle listened {readyPuzzle.gameObject}");
                StageManager.SingletonManager.ReduceRemainRemoveObstacleCount();

                if (IsSpawned)
                {
                    Destroy_ServerRpc();
                }
            }
            else
            {
                isInteracted = false;
                StageManager.SingletonManager.InsertPuzzle(readyPuzzle);
            }

            GameManager.Singleton.ChangeState(GameState.Playing);
            StageManager.SingletonManager.ClosePuzzleQueue();
        }
        else
        {
            GameManager.Singleton.ChangeState(GameState.Playing);
            if(isClear)
            {
                TutorialSceneManager.singleton.EndOfMission();
                gameObject.SetActive(false);
            }
            else
            {
                GameUIManager.Singleton.SetCanMoveText(1);
                isInteracted = false;
            }
        }
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
