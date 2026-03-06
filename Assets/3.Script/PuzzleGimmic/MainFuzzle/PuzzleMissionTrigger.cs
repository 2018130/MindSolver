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

    Coroutine CallingPuzzleMission;

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
        if(CallingPuzzleMission == null)
        {
            CallingPuzzleMission = StartCoroutine(CallListener());
        }
    }

    private IEnumerator CallListener()
    {
        PaperManager.singleton.StartPaperUnfoldAnimation();

        WaitWhile waitWhile = new WaitWhile(() => PaperManager.singleton.IsPaperOpening);

        yield return waitWhile;

        if (IsSpawned)
        {
            if (StageManager.SingletonManager.RemainRemoveObstacleCount <= 0)
            {
                CallingPuzzleMission = null;
                yield break;
            }

            if (!isInteracted && GameManager.Singleton.GameState != GameState.Puzzle)
            {
                isInteracted = true;
                readyPuzzle = StageManager.SingletonManager.GetNextPuzzle();
                readyPuzzle?.StartPuzzle(this);
            }
        }
        else
        {
            readyPuzzle?.StartPuzzle(this);
        }
        CallingPuzzleMission = null;
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
    }

    public void SetTouchable(bool active)
    {
        if(col == null)
        {
            col = GetComponent<Collider2D>();
        }
        col.enabled = active;
    }

    public void EndPuzzle(bool isClear)
    {
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
            TutorialSceneManager.singleton.EndOfMission();
            gameObject.SetActive(false);
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
