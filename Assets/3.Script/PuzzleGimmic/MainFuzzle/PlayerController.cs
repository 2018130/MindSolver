using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private SpriteRenderer front;

    [SerializeField]
    private SpriteRenderer back;

    [SerializeField]
    private float moveDurationPerTile = 0.5f;

    public override void OnDestroy()
    {
        StopAllCoroutines();
    }

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
                if(IsSpawned)
                {
                    ViewFrontDir_ClientRpc(false, true);
                }
                else
                {
                    front.gameObject.SetActive(false);
                    back.gameObject.SetActive(true);

                    front.flipX = true;
                }
            }
            //오른쪽 아래
            else
            {
                if (IsSpawned)
                {
                    ViewFrontDir_ClientRpc(true, false);
                }
                else
                {
                    front.gameObject.SetActive(true);
                    back.gameObject.SetActive(false);

                    front.flipX = true;
                }
            }
        }
        else
        {
            // 왼쪽 위
            if (moveDir.y > 0)
                {
                    if (IsSpawned)
                    {
                        ViewFrontDir_ClientRpc(false, false);
                    }
                else
                {
                    front.gameObject.SetActive(false);
                    back.gameObject.SetActive(true);

                    front.flipX = false;
                }
            }
            // 왼쪽 아래
            else
                    {
                        if (IsSpawned)
                        {
                            ViewFrontDir_ClientRpc(true, true);
                        }
                else
                {
                    front.gameObject.SetActive(true);
                    back.gameObject.SetActive(false);

                    front.flipX = true;
                }
            }
        }
    }

    [ClientRpc]
    private void ViewFrontDir_ClientRpc(bool isFront, bool flip)
    {
        front.gameObject.SetActive(isFront);
        back.gameObject.SetActive(!isFront);

        if(isFront)
        {
            front.flipX = flip;
        }
        else
        {
            back.flipX = flip;
        }
    }
}
