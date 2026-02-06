using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{
    [SerializeField]
    private LayerMask pipeLayer;

    private WaterSpawner waterSpawner;

    private void Start()
    {
        InputManager.Singleton.OnClickedLeftBtn += RotateTargetPipe;
        waterSpawner = FindAnyObjectByType<WaterSpawner>();
    }

    public void RotateTargetPipe()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);

        Collider2D target = Physics2D.OverlapCircle(worldPosition, 0.01f, pipeLayer);
        if(target != null)
        {

#if UNITY_EDITOR
            target.GetComponent<Pipe>().RotateCW();
#else
            target.GetComponent<Pipe>().RotateCW_ServerRpc();
#endif
        }
    }

}
