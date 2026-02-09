using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRigidbody : MonoBehaviour
{
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

    public void AddForce(Vector2 hitPoint)
    {
        Debug.Log($"{gameObject.name} add force");
        float forceX = accelerationAmount / (col.bounds.center.x - hitPoint.x);
        float forceY = accelerationAmount / (col.bounds.center.y - hitPoint.y);

        acceleration.x = forceX;
        acceleration.y = forceY;
    }

    private void ApplyDamping()
    {
        Debug.Log($"{gameObject.name} damping");
        int signX = acceleration.x > 0 ? 1 : -1;
        int signY = acceleration.y > 0 ? 1 : -1;
        acceleration.x -= signX * damping * Time.deltaTime;
        acceleration.y -= signY * damping * Time.deltaTime;

        transform.position += acceleration * Time.deltaTime * Time.deltaTime;
    }
}
