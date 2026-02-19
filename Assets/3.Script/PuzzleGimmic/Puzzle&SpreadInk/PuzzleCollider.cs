using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCollider : MonoBehaviour
{
    private GameObject piece;

    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private int puzzleLinkId;

    private bool isPuzzleLinked;

    private static int linkedPuzzleCount = 0;

    private void Awake()
    {
        piece = transform.parent.gameObject;
    }

    private void OnEnable()
    {
        isPuzzleLinked = false;
        linkedPuzzleCount = 0;
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
                    linkedPuzzleCount++;
                    Debug.Log($"linkedPuzzleCount : {linkedPuzzleCount}");
                    if(linkedPuzzleCount >= PuzzleController.s_puzzleCount - 1)
                    {
                        PuzzleMissonListener puzzleMissonListener = GetComponentInParent<PuzzleMissonListener>();
                        if (puzzleMissonListener != null)
                        {
                            puzzleMissonListener.EndPuzzle(true);
                        }
                    }
                }
            }
        }
    }
}
