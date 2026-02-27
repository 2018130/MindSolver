using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeWaterChecker : MonoBehaviour
{
    [SerializeField]
    private LayerMask waterLayer;

    [SerializeField]
    private ColorType colorType;

    private PuzzleMissonListener puzzleMissonListener;

    private void Start()
    {
        puzzleMissonListener = GetComponentInParent<PuzzleMissonListener>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & waterLayer.value) != 0)
        {
            if (collision.TryGetComponent<PipeWater>(out PipeWater pipeWater))
            {
                if (pipeWater.ColorType == colorType)
                {
                    puzzleMissonListener.EndPuzzle(true);
                }
            }
        }
    }
}
