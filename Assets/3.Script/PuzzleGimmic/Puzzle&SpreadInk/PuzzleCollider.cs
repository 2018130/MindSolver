using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCollider : MonoBehaviour
{
    private GameObject piece;

    private Collider2D col;

    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private int puzzleLinkId;

    private bool isPuzzleLinked;

    private void Awake()
    {
        piece = transform.parent.gameObject;
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PuzzleCollider>(out PuzzleCollider puzzleController))
        {
            if(!puzzleController.isPuzzleLinked)
            {
                if (puzzleController.puzzleLinkId == puzzleLinkId)
                {
                    puzzleController.isPuzzleLinked = true;
                    puzzleController.piece.transform.position = piece.transform.position + puzzleController.offset;
                    puzzleController.piece.transform.SetParent(piece.transform);

                    isPuzzleLinked = true;
                }
            }
        }
    }
}
