using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueData
{
    public int ID;
    public string Name;
    public string Dialogue;
    public bool HasChoice;
    public string AcceptDialogue;
    public int AcceptID;
    public string RejectDialogue;
    public int RejectID;
    public int Favorability;
    public string ImagePath;
}

public class DialogueManager : MonoBehaviour, ISceneContextBuilt
{
    [Header("Reference")]
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TMP_Text dialogueText;
    [SerializeField]
    private Button[] chooseButton;
    [SerializeField]
    private Image backgroundImg;
    [SerializeField]
    private GameObject dialogue;
    [SerializeField]
    private string dataFileName;

    [Space(10f)]

    [Header("Word Setting")]
    [SerializeField]
    private float wordPrintSpeed = 0.1f;
    [SerializeField]
    private int wordPrintCountPerCycle = 1;

    [Space(10f)]

    [Header("Dialogue")]
    [SerializeField]
    private List<DialogueData> dialogueDatas;
    // 다이얼로그 데이터 값
    private Queue<DialogueData> dialogueQueue = new Queue<DialogueData>();
    private bool isPrintAnyDialogue = false;
    public bool IsDialogueEnded { get; set; } = true;

    public event Action<int> OnDialogueStarted;
    [SerializeField]
    private int nextPrintDialogueID = 1;
    [SerializeField]
    private float endDelay = 3f;
    public int Priority { get; set; } = 2;

    private void Awake()
    {
        dialogueDatas = CsvReader.LoadCsvData(dataFileName);
    }

    private void PrintDialogue(DialogueData dialogueData)
    {
        if (dialogueData != null)
        {
            dialogueQueue.Enqueue(dialogueData);
        }

        if (!isPrintAnyDialogue)
        {
            StartCoroutine(PrintDialogue_co());
        }
    }

    public void PrintDialogue(int id = -1)
    {
        if(id == -1)
        {
            id = nextPrintDialogueID;
        }

        DialogueData data = dialogueDatas.Find(x => x.ID == id);

        if (!isPrintAnyDialogue)
        {
            IsDialogueEnded = false;
            PrintDialogue(data);
        }
    }

    private IEnumerator PrintDialogue_co()
    {
        if (dialogueQueue.Count > 0)
        {
            isPrintAnyDialogue = true;

            bool isClickedAnyKey = false;
            DialogueData currentDialogue = dialogueQueue.Dequeue();

            // 초기 설정
            OnDialogueStarted?.Invoke(currentDialogue.ID);
            dialogueText.text = "";
            nameText.text = currentDialogue.Name;
            Debug.Log($"Print dialogue, Current dialogue data : {currentDialogue.Name}");
            SetButtonActive(currentDialogue,
                () =>
                {
                    PrintDialogue(currentDialogue.AcceptID);
                    nextPrintDialogueID = currentDialogue.AcceptID;
                    isClickedAnyKey = true;

                    chooseButton[0].interactable = false;
                    chooseButton[1].interactable = false;
                },
                () =>
                {
                    PrintDialogue(currentDialogue.RejectID);
                    nextPrintDialogueID = currentDialogue.RejectID;
                    isClickedAnyKey = true;

                    chooseButton[0].interactable = false;
                    chooseButton[1].interactable = false;
                });

            // 이미지
            if(currentDialogue.ImagePath != "")
            {
                Sprite loadedSprite = Resources.Load<Sprite>(currentDialogue.ImagePath);

                if (loadedSprite != null && backgroundImg != null)
                {
                    backgroundImg.sprite = loadedSprite;
                    backgroundImg.gameObject.SetActive(true);
                }
            }

            SetDialogueActive(true);

            //다이얼로그
            int index = 0;
            while (index < currentDialogue.Dialogue.Length)
            {
                string strPerCycle = "";
                for (int i = 0; i < wordPrintCountPerCycle; i++)
                {
                    if (index + i >= currentDialogue.Dialogue.Length)
                    {
                        break;
                    }
                    strPerCycle += currentDialogue.Dialogue[index + i];
                }

                dialogueText.text += strPerCycle;

                yield return new WaitForSeconds(1 / wordPrintSpeed);

                index += wordPrintCountPerCycle;
            }

            // 옵저버 패턴
            yield return new WaitUntil(() =>
            {
                if (!currentDialogue.HasChoice)
                {
                    isClickedAnyKey = InputManager.Singleton.LeftButtonClicked;
                }

                return isClickedAnyKey;
            });

            isPrintAnyDialogue = false;

            if (!currentDialogue.HasChoice)
            {
                nextPrintDialogueID++;
            }

            if (currentDialogue.Name == "EOF" || currentDialogue.AcceptID == -1)
            {
                yield return new WaitForSeconds(endDelay);

                Debug.Log($"한 다이얼로그 사이클이 끝났습니다!");
                IsDialogueEnded = true;
                SetDialogueActive(false);

                yield break;
            }
            else
            {
                PrintDialogue(nextPrintDialogueID);
            }
        }
    }

    private void SetButtonActive(DialogueData currentDialogue, UnityEngine.Events.UnityAction acceptChoiceCallback = null, UnityEngine.Events.UnityAction rejectChoideCallback = null)
    {
        if (currentDialogue.HasChoice)
        {
            if (currentDialogue.AcceptDialogue.Length != 0)
            {
                chooseButton[0].gameObject.SetActive(true);
            }
            else
            {
                chooseButton[0].gameObject.SetActive(false);
            }
            if (currentDialogue.RejectDialogue.Length != 0)
            {
                chooseButton[1].gameObject.SetActive(true);
            }
            else
            {
                chooseButton[1].gameObject.SetActive(false);
            }
            chooseButton[0].interactable = true;
            chooseButton[1].interactable = true;

            chooseButton[0].GetComponentInChildren<TMP_Text>().text = currentDialogue.AcceptDialogue;
            chooseButton[1].GetComponentInChildren<TMP_Text>().text = currentDialogue.RejectDialogue;

            chooseButton[0].onClick.RemoveAllListeners();
            chooseButton[1].onClick.RemoveAllListeners();

            chooseButton[0].onClick.AddListener(acceptChoiceCallback);
            chooseButton[1].onClick.AddListener(rejectChoideCallback);
        }
        else
        {
            chooseButton[0].gameObject.SetActive(false);
            chooseButton[1].gameObject.SetActive(false);
        }
    }

    private void SetDialogueActive(bool active)
    {
        dialogue.SetActive(active);
        if(backgroundImg != null)
        {
            backgroundImg.gameObject.SetActive(active);
        }
    }

    public void OnSceneContextBuilt()
    {
        PrintDialogue(nextPrintDialogueID);
    }
}
