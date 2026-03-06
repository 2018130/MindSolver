using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerController : MonoBehaviour
{
    private SpriteRenderer mySpriteRenderer; // 멤버 변수 이름 변경 (충돌 방지)
    private int defaultSortingOrder;
    [SerializeField]
    private float rayDistance = 0.5f;

    private void Awake()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        defaultSortingOrder = mySpriteRenderer.sortingOrder;
    }

    private void FixedUpdate()
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(transform.position, transform.position + Vector3.down * rayDistance, 1 << LayerMask.NameToLayer("Ground"));
        Debug.DrawLine(transform.position, transform.position + Vector3.down * rayDistance, Color.red, 1f);
        
        for (int i = 0; i < cols.Length; i++)
        {
            Renderer obstacleRenderer = cols[i].GetComponentInParent<Renderer>();

            if (cols[i].CompareTag("Obstacle") &&
                obstacleRenderer != null)
            {
                mySpriteRenderer.sortingOrder = obstacleRenderer.sortingOrder - 1;
                return;
            }
        }

        mySpriteRenderer.sortingOrder = defaultSortingOrder;
    }
}