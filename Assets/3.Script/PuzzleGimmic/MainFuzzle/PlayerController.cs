using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer front;

    [SerializeField]
    private SpriteRenderer back;

    [SerializeField]
    private float moveDurationPerTile = 0.5f;

    public IEnumerator MoveTo(Vector3 destPosition)
    {
        float timer = 0f;
        Vector3 originPosition = transform.position;

        Vector2 moveDir = new Vector2(destPosition.x - originPosition.x, destPosition.y - originPosition.y);

        SetMoveDirEffect(moveDir);

        while (timer < moveDurationPerTile)
        {
            timer += Time.deltaTime;
            yield return null;

            transform.position = Vector3.Lerp(originPosition, destPosition, timer / moveDurationPerTile);
        }

        transform.position = destPosition;
    }

    private void SetMoveDirEffect(Vector2 moveDir)
    {
        if (moveDir.x > 0)
        {
            // 오른쪽 위
            if (moveDir.y > 0)
            {
                front.gameObject.SetActive(false);
                back.gameObject.SetActive(true);

                back.flipX = true;
            }
            //오른쪽 아래
            else
            {
                front.gameObject.SetActive(true);
                back.gameObject.SetActive(false);

                front.flipX = false;
            }
        }
        else
        {
            // 왼쪽 위
            if (moveDir.y > 0)
            {
                front.gameObject.SetActive(false);
                back.gameObject.SetActive(true);

                back.flipX = false;
            }
            // 왼쪽 아래
            else
            {
                front.gameObject.SetActive(true);
                back.gameObject.SetActive(false);

                front.flipX = true;
            }
        }
    }
}
