using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChecker : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(LayerMask.LayerToName(collision.gameObject.layer) == "Water")
        {
            WaterSpawner_Single.ReturnToPool(collision.gameObject);
        }
    }
}
