using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum MissionType
{
    Single,
    Multi,
}

public class PuzzleMissonListener : MonoBehaviour
{
    [SerializeField]
    private MissionType missionType = MissionType.Single;
    public MissionType MissionType => missionType;

    private PuzzleMissionTrigger sender;

    public void StartPuzzle(PuzzleMissionTrigger sender)
    {
        gameObject.SetActive(true);
        this.sender = sender;

        if(missionType == MissionType.Multi)
        {
            ThreadDrawer drawer = GetComponentInChildren<ThreadDrawer>();
            if(drawer != null)
            {
                drawer.InitThread();
            }
        }
    }

    public void EndPuzzle(bool isClear)
    {
        gameObject.SetActive(false);

        if(isClear && sender != null)
        {
            sender.ClearPuzzle();
        }
    }
}
