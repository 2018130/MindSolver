using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneManager : MonoBehaviour
{
    [SerializeField]
    private PathFinding redPathFinding;
    [SerializeField]
    private PathFinding bluePathFinding;

    [SerializeField]
    private DialogueManager dialogueManager;

    private void Start()
    {
        StartCoroutine(Step01_co());
    }

    private IEnumerator Step01_co()
    {
        dialogueManager.PrintDialogue();

        yield return new WaitUntil(() => dialogueManager.IsDialogueEnded);

        GameUIManager.Singleton.SetPathFindingBtn(true);
    }
    //
}
