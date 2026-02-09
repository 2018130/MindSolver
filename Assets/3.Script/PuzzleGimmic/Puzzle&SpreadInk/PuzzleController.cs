using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    private Collider2D[] childColliders;

    private void Start()
    {
        childColliders = transform.GetComponentsInChildren<Collider2D>();
    }

    private void Update()
    {
        if(InputManager.Singleton.LeftButtonClicked)
        {
            MoveTo();
        }
        else
        {
            SetPuzzleCollider(true);
        }
    }
    private void MoveTo()
    {
        SetPuzzleCollider(false);
        Vector3 worldPosWithMouse = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
        worldPosWithMouse.z = 0;
        Collider2D[] pieces = Physics2D.OverlapPointAll(worldPosWithMouse);

        foreach(var piece in pieces)
        {
            Transform prePiece = null;
            Transform curPiece = piece.transform;

            while(curPiece.TryGetComponent(out PuzzleController puzzleController))
            {
                prePiece = curPiece;
                curPiece = curPiece.transform.parent;
            }

            if(prePiece != null)
            {
                prePiece.position = worldPosWithMouse;
            }
        }
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
}
