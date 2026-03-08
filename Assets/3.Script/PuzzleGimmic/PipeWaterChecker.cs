using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeWaterChecker : MonoBehaviour
{
    private LayerMask waterLayer;

    private ColorType colorType;

    private PuzzleMissonListener puzzleMissonListener;

    private void Start()
    {
        puzzleMissonListener = GetComponentInParent<PuzzleMissonListener>();
        waterLayer = LayerMask.NameToLayer("Water");
        colorType = NetworkPlayer.IsServerPlayer ? ColorType.Red : ColorType.Blue;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Object triggered layer : {collision.gameObject.layer} {waterLayer.value}");
        if (collision.gameObject.layer == waterLayer.value)
        {
            if (collision.TryGetComponent<PipeWater>(out PipeWater pipeWater))
            {
                if (pipeWater.ColorType == colorType)
                {
                    WaterSpawner_Single.ReturnToPool(collision.gameObject);
                    puzzleMissonListener.EndPuzzle(true);
                }
            }
        }
    }
}
