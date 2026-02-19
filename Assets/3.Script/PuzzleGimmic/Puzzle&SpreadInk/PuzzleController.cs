using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour, IInteractable
{
    private Transform puzzleParentTransform;

    private Vector3 initPos;
    private Collider2D[] childColliders;

    public static int s_puzzleCount;

    private void Awake()
    {
        puzzleParentTransform = transform.parent;
        initPos = transform.position;
    }
    private void Start()
    {
        childColliders = transform.GetComponentsInChildren<Collider2D>();
        if(s_puzzleCount == 0)
        {
            s_puzzleCount = FindObjectsByType<PuzzleController>(FindObjectsSortMode.None).Length;
            Debug.Log("puzzleCount : " + s_puzzleCount);
        }
    }

    private void OnEnable()
    {
        transform.SetParent(puzzleParentTransform);
        transform.position = initPos;
        SetPuzzleCollider(true);
    }

    private void SetPuzzleCollider(bool active)
    {
        foreach(var childCol in childColliders)
        {
            if(childCol.TryGetComponent(out PuzzleCollider puzzleCollider))
            {
                childCol.enabled = active;
            }
        }
    }

    public void Interact(Vector2 worldPosFromMousePosition)
    {
        SetPuzzleCollider(false);
        Collider2D[] pieces = Physics2D.OverlapPointAll(worldPosFromMousePosition);

        foreach (var piece in pieces)
        {
            Transform prePiece = null;
            Transform curPiece = piece.transform;

            while (curPiece.TryGetComponent(out PuzzleController puzzleController))
            {
                prePiece = curPiece;
                curPiece = curPiece.transform.parent;
            }

            if (prePiece != null)
            {
                prePiece.position = worldPosFromMousePosition;
            }
        }
    }

    public void EndInteract()
    {
        SetPuzzleCollider(true);
    }
}
