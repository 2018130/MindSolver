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
        this.sender = sender;

        if(missionType == MissionType.Multi)
        {
            ThreadDrawer drawer = GetComponentInChildren<ThreadDrawer>();
            if(drawer != null)
            {
                drawer.StartGame();
            }
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void EndPuzzle(bool isClear)
    {
        if (missionType == MissionType.Multi)
        {
            if (isClear && sender != null)
            {
                sender.ClearPuzzle();
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
