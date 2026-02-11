using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThreadHandle : NetworkBehaviour
{
    [SerializeField]
    private float speed = 1f;

    [SerializeField]
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(InputManager.Singleton.LeftButtonClicked && IsOwner)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(InputManager.Singleton.MousePosition);
            Interact(worldPos);
        }
    }

    public void Interact(Vector3 worldPosFromMousePosition)
    {
        Vector3 dir = worldPosFromMousePosition - transform.position;
        dir = dir.normalized;

        rb.MovePosition(dir * speed + transform.position);
    }
}
