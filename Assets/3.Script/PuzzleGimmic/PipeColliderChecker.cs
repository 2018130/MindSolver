using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeColliderChecker : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private List<Direction> rightDir;

    [SerializeField]
    private float delayTime = 0.01f;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((targetLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            StartCoroutine(CheckErrorWater_co(collision));
        }
    }

    private IEnumerator CheckErrorWater_co(Collision2D collision)
    {
        yield return new WaitForSeconds(delayTime);

        CheckErrorWater(collision);
    }

    private void CheckErrorWater(Collision2D collision)
    {
        float xVector = collision.transform.position.x - transform.position.x;
        float yVector = collision.transform.position.y - transform.position.y;
        Vector2 colExtends = col.bounds.extents;


        if (collision.transform.position.y >= transform.position.y - colExtends.y &&
            collision.transform.position.y <= transform.position.y + colExtends.y)
        {
            if (xVector > 0)
            {
                if (!CheckDir(Direction.Right))
                {
                    WaterSpawner.ReturnToPool(collision.gameObject);
                }
            }
            else
            {
                if (!CheckDir(Direction.Left))
                {
                    WaterSpawner.ReturnToPool(collision.gameObject);
                }
            }
        }

        if (collision.transform.position.x >= transform.position.x - colExtends.x &&
            collision.transform.position.x <= transform.position.x + colExtends.x)
        {
            if (yVector > 0)
            {
                if (!CheckDir(Direction.Up))
                {
                    WaterSpawner.ReturnToPool(collision.gameObject);
                }
            }
            else
            {
                if (!CheckDir(Direction.Down))
                {
                    WaterSpawner.ReturnToPool(collision.gameObject);
                }
            }
        }
    }

    private bool CheckDir(Direction dir)
    {
        for (int i = 0; i < rightDir.Count; i++)
        {
            if (rightDir[i] == dir)
            {
                return true;
            }
        }

        return false;
    }

    public void Rotate(int dir)
    {
        for (int i = 0; i < rightDir.Count; i++)
        {
            int holeDir = (int)rightDir[i] + 1;
            if (holeDir > 3)
                holeDir = 0;

            rightDir[i] = (Direction)holeDir;
        }
    }
}
