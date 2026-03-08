using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : NetworkBehaviour
{
    public static GameUIManager Singleton;

    [SerializeField]
    private List<Sprite> redCutscene = new List<Sprite>();
    [SerializeField]
    private List<Sprite> blueCutscene = new List<Sprite>();

    [SerializeField]
    private Image cutsceneBG;
    [SerializeField]
    private Image cutsceneImg;

    [SerializeField]
    private TMP_Text clearText;

    [SerializeField]
    private GameObject canMoveText;

    [SerializeField]
    private Button startPathFinding_Btn;

    [Header("Puzzle")]
    [SerializeField]
    private GameObject maxDrawLineText;

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name == "Stage")
        {
            startPathFinding_Btn.onClick.AddListener(StageManager.SingletonManager.StartPathFinding);
        }
        else if(SceneManager.GetActiveScene().name == "Tutorial")
        {
            startPathFinding_Btn.onClick.AddListener(TutorialSceneManager.singleton.StartPathfinding);
        }

        startPathFinding_Btn.onClick.AddListener(() => SetPathFindingBtn(false));
    }

    public void SetCanMoveText(int moveCount)
    {
        string str = "";
        if(moveCount != -1)
        {
            str = "상자 제거 가능 횟수\n" + moveCount;
        }
        canMoveText.GetComponentInChildren<TMP_Text>().text = str;
    }

    public void SetclearText(string str, bool isNetwork = true)
    {
        if(isNetwork)
        {
            if(IsSpawned)
                SetclearText_ClientRpc(str);
        }
        else
        {
            clearText.text = str;
        }
    }

    public void SetActiveMainUI(bool active)
    {
        canMoveText.gameObject.SetActive(active);
    }

    [ClientRpc]
    private void SetclearText_ClientRpc(string str)
    {
        clearText.text = str;
    }

    public void SetMaxLineText(int maxLineCount)
    {
        if(maxLineCount > 0)
        {
            maxDrawLineText.gameObject.SetActive(true);
        }
        else
        {
            maxDrawLineText.gameObject.SetActive(false);
        }

        maxDrawLineText.GetComponentInChildren<TMP_Text>().text = "남은 획수 : " + maxLineCount;
    }

    public void SetColorPaintingProgressText(float progress)
    {
        if (progress > 0)
        {
            maxDrawLineText.gameObject.SetActive(true);
        }
        else
        {
            maxDrawLineText.gameObject.SetActive(false);
        }

        maxDrawLineText.GetComponentInChildren<TMP_Text>().text = $"현재 진행도: {progress * 100:F1}%";
    }

    public void SetOpenText(int openedCount)
    {
        if (openedCount < 3)
        {
            maxDrawLineText.gameObject.SetActive(true);
        }
        else
        {
            maxDrawLineText.gameObject.SetActive(false);
        }

        maxDrawLineText.GetComponentInChildren<TMP_Text>().text = $"남은 뒤집기 수: {3 - openedCount}";
    }

    public void SetPictureText(bool active)
    {
            maxDrawLineText.gameObject.SetActive(active);

        maxDrawLineText.GetComponentInChildren<TMP_Text>().text = $"오른쪽 그림과 같은 위치를 찾아주세요. 마우스 클릭시 시작!!";
    }
    public void SetObjectFindingText(bool active)
    {
        maxDrawLineText.gameObject.SetActive(active);

        maxDrawLineText.GetComponentInChildren<TMP_Text>().text = $"물건들 사이에서 네잎클로버를 찾아주세요!!";
    }

    public void SetPathFindingBtn(bool active)
    {
        startPathFinding_Btn.gameObject.SetActive(active);
    }

    public void ViewCutscene(bool isHost, int idx)
    {
        if(idx == -1)
        {
            cutsceneImg.enabled = false;
            cutsceneBG.enabled = false;
            return;
        }
        cutsceneBG.enabled = true;
        cutsceneImg.enabled = true;

        if (isHost)
        {
            cutsceneImg.sprite = redCutscene[idx];
        }
        else
        {
            cutsceneImg.sprite = blueCutscene[idx];
        }
    }
}
