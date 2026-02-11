using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour, IInteractable
{
    private Collider2D[] childColliders;

    private void Start()
    {
        childColliders = transform.GetComponentsInChildren<Collider2D>();
    }

    private void SetPuzzleCollider(bool active)
    {
        foreach(var childCol in childColliders)
        {
            if(childCol.TryGetComponent<PuzzleCollider>(out PuzzleCollider puzzleCollider))
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
