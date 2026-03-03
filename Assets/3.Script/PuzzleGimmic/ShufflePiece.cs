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
    private List<RememberPiece> pieces = new List<RememberPiece>();

    [SerializeField]
    private Sprite backPieceImage;

    PuzzleMissonListener missonListener;
    private int openedPieceCount = 0;

    private void Start()
    {
        missonListener = GetComponentInParent<PuzzleMissonListener>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).TryGetComponent(out RememberPiece remember))
            {
                pieces.Add(remember);
            }
        }
    }

    private void OnEnable()
    {
        openedPieceCount = 0;
        GameUIManager.Singleton.SetOpenText(openedPieceCount);

        for(int i = 0; i < pieces.Count; i++)
        {
            float minX = pieces[i].transform.position.x;
            int minIdx = i;
            for(int j = i; j < pieces.Count; j++)
            {
                if(minX > pieces[j].transform.position.x)
                {
                    minIdx = j;
                    minX = pieces[j].transform.position.x;
                }
            }

            Vector3 tmp = pieces[i].transform.position;
            pieces[i].transform.position = pieces[minIdx].transform.position;
            pieces[minIdx].transform.position = tmp;
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
        for(int i = 0; i < pieces.Count; i++)
        {
            pieces[i].GetComponent<RememberPiece>().ChangeToBackImage();
        }

        yield return new WaitForSeconds(shuffleDelayTime);

        int count = 0;
        while(shuffleCount > count)
        {
            int idx1 = UnityEngine.Random.Range(0, pieces.Count);
            int idx2 = UnityEngine.Random.Range(0, pieces.Count);

            if(idx1 != idx2)
            {
                Vector3 pos1 = pieces[idx1].transform.position;
                Vector3 pos2 = pieces[idx2].transform.position;
                float timer = 0f;

                // 자리 교체
                while(shuffleDuration > timer)
                {
                    timer += Time.deltaTime;

                    yield return null;

                    pieces[idx1].transform.position = Vector3.Lerp(pos1, pos2, timer / shuffleDuration);
                    pieces[idx2].transform.position = Vector3.Lerp(pos2, pos1, timer / shuffleDuration);
                }
                pieces[idx1].transform.position = pos2;
                pieces[idx2].transform.position = pos1;
                count++;

                yield return new WaitForSeconds(shuffleDelayTime);
            }
        }
    }

    public void IncreaseOpenPieceCount(RememberPiece rememberPiece)
    {
        if (pieces[openedPieceCount] != rememberPiece)
        {
            GameUIManager.Singleton.SetOpenText(3);
            missonListener.EndPuzzle(false);
            return;
        }

        openedPieceCount++;
        GameUIManager.Singleton.SetOpenText(openedPieceCount);

        if(openedPieceCount == 3)
        {
            missonListener.EndPuzzle(true);
        }

    }
}
