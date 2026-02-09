using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRigidbody : MonoBehaviour, IInteractable
{
    [SerializeField]
    private float maxAccelerationAmount = 100f;
    [SerializeField]
    private float accelerationAmount = 3f;

    [SerializeField]
    private Vector3 acceleration = Vector3.zero;

    [SerializeField]
    private float damping = 1f;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if(acceleration.sqrMagnitude > 0.01f)
        {
            ApplyDamping();
        }
    }

    private void ApplyDamping()
    {
        int signX = acceleration.x > 0 ? 1 : -1;
        int signY = acceleration.y > 0 ? 1 : -1;
        acceleration.x -= signX * damping * Time.deltaTime;
        acceleration.y -= signY * damping * Time.deltaTime;

        transform.position += acceleration * Time.deltaTime * Time.deltaTime;
    }

    public void Interact(Vector2 worldPositionFromMousePosition)
    {
        float forceX = accelerationAmount / (col.bounds.center.x - worldPositionFromMousePosition.x);
        forceX = Mathf.Clamp(forceX, -maxAccelerationAmount, maxAccelerationAmount);
        float forceY = accelerationAmount / (col.bounds.center.y - worldPositionFromMousePosition.y);
        forceY = Mathf.Clamp(forceY, -maxAccelerationAmount, maxAccelerationAmount);

        acceleration.x = forceX;
        acceleration.y = forceY;
    }

    public void EndInteract()
    {

    }
}
