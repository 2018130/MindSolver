using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeWaterChecker : MonoBehaviour
{
    [SerializeField]
    private LayerMask waterLayer;

    private PuzzleMissonListener puzzleMissonListener;

    private void Start()
    {
        puzzleMissonListener = GetComponentInParent<PuzzleMissonListener>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == waterLayer)
        {
            if(collision.TryGetComponent<PipeWater>(out PipeWater pipeWater))
            {
                if(pipeWater.ColorType == ColorType.Mixed)
                {
                    puzzleMissonListener.EndPuzzle(true);
                }
            }
        }
    }
}
