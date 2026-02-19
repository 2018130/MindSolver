using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShufflePiece : MonoBehaviour
{
    [SerializeField]
    private float openTime = 3f;

    [SerializeField]
    private int shuffleCount = 5;
    [SerializeField]
    private float shuffleDuration = 1f;
    [SerializeField]
    private float shuffleDelayTime = 1f;

    [SerializeField]
    private Transform[] pieces;

    [SerializeField]
    private Sprite backPieceImage;


    private void Start()
    {
        pieces = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            pieces[i] = transform.GetChild(i);
        }
        StartCoroutine(StartShuffle());
    }

    private void OnDestroy()
    {
        pieces = null;
    }

    private IEnumerator StartShuffle()
    {
        yield return new WaitForSeconds(openTime);

        // 뒤집기
        for(int i = 0; i < pieces.Length; i++)
        {
            pieces[i].GetComponent<RememberPiece>().ChangeToBackImage();
        }

        yield return new WaitForSeconds(shuffleDelayTime);

        int count = 0;
        while(shuffleCount > count)
        {
            int idx1 = UnityEngine.Random.Range(0, pieces.Length);
            int idx2 = UnityEngine.Random.Range(0, pieces.Length);

            if(idx1 != idx2)
            {
                Vector3 pos1 = pieces[idx1].position;
                Vector3 pos2 = pieces[idx2].position;
                float timer = 0f;

                // 자리 교체
                while(shuffleDuration > timer)
                {
                    timer += Time.deltaTime;

                    yield return null;

                    pieces[idx1].position = Vector3.Lerp(pos1, pos2, timer / shuffleDuration);
                    pieces[idx2].position = Vector3.Lerp(pos2, pos1, timer / shuffleDuration);
                }
                pieces[idx1].position = pos2;
                pieces[idx2].position = pos1;
                count++;

                yield return new WaitForSeconds(shuffleDelayTime);
            }
        }
    }
}
